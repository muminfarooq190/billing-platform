using FluentAssertions;
using TravelService.Application.Abstractions;
using TravelService.Application.Commands.DeleteQuotationAttachment;
using TravelService.Application.Commands.UploadQuotationAttachment;
using TravelService.Domain.Aggregates;
using TravelService.Domain.Exceptions;
using TravelService.Domain.Repositories;

namespace TravelService.Tests;

public sealed class QuotationAttachmentCommandTests
{
    [Fact]
    public async Task UploadQuotationAttachment_ShouldPersistAttachment_AndUploadFile()
    {
        var quotation = CreateQuotation();
        quotation.AddLineItem("Hotel", 500m, 1, "USD");
        var quotationRepository = new InMemoryQuotationRepository(quotation);
        var revisionRepository = new InMemoryQuotationRevisionRepository();
        var attachmentRepository = new InMemoryQuotationAttachmentRepository();
        var fileStorage = new RecordingFileStorage();
        var handler = new UploadQuotationAttachmentCommandHandler(quotationRepository, revisionRepository, attachmentRepository, fileStorage, new NoOpActivityWriter(), new FakeActorContext(quotation.TenantId), new NoOpUnitOfWork());

        var result = await handler.Handle(new UploadQuotationAttachmentCommand(
            quotation.TenantId,
            quotation.Id,
            null,
            "brochure.pdf",
            "application/pdf",
            128,
            "Pdf",
            "Customer brochure",
            true,
            1,
            [1, 2, 3]), CancellationToken.None);

        result.AttachmentId.Should().NotBe(Guid.Empty);
        attachmentRepository.Attachments.Should().ContainSingle();
        attachmentRepository.Attachments[0].IsCustomerVisible.Should().BeTrue();
        fileStorage.UploadedStorageKeys.Should().ContainSingle();
        fileStorage.UploadedStorageKeys[0].Should().Contain($"tenant/{quotation.TenantId:D}/quotations/{quotation.Id:D}/");
    }

    [Fact]
    public async Task UploadQuotationAttachment_ShouldRejectUnsupportedContentType()
    {
        var quotation = CreateQuotation();
        var handler = new UploadQuotationAttachmentCommandHandler(
            new InMemoryQuotationRepository(quotation),
            new InMemoryQuotationRevisionRepository(),
            new InMemoryQuotationAttachmentRepository(),
            new RecordingFileStorage(),
            new NoOpActivityWriter(),
            new FakeActorContext(quotation.TenantId),
            new NoOpUnitOfWork());

        var act = async () => await handler.Handle(new UploadQuotationAttachmentCommand(
            quotation.TenantId,
            quotation.Id,
            null,
            "evil.exe",
            "application/octet-stream",
            32,
            "Document",
            null,
            false,
            0,
            [9]), CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>().WithMessage("*not allowed*");
    }

    [Fact]
    public async Task DeleteQuotationAttachment_ShouldSoftDelete_AndRemoveStoredFile()
    {
        var quotation = CreateQuotation();
        var attachment = QuotationAttachment.Create(
            quotation.Id,
            null,
            quotation.TenantId,
            "tenant/path/file.pdf",
            "file.pdf",
            "application/pdf",
            42,
            "Pdf",
            null,
            true,
            0);

        var fileStorage = new RecordingFileStorage();
        var handler = new DeleteQuotationAttachmentCommandHandler(
            new InMemoryQuotationRepository(quotation),
            new InMemoryQuotationAttachmentRepository(attachment),
            fileStorage,
            new NoOpActivityWriter(),
            new FakeActorContext(quotation.TenantId),
            new NoOpUnitOfWork());

        await handler.Handle(new DeleteQuotationAttachmentCommand(quotation.TenantId, quotation.Id, attachment.Id), CancellationToken.None);

        attachment.DeletedAt.Should().NotBeNull();
        fileStorage.DeletedStorageKeys.Should().ContainSingle().Which.Should().Be("tenant/path/file.pdf");
    }

    private static Quotation CreateQuotation()
        => Quotation.Create(Guid.NewGuid(), Guid.NewGuid(), "Ava", "Summer Trip", "Istanbul", DateTimeOffset.UtcNow.AddDays(20), DateTimeOffset.UtcNow.AddDays(25), 2, "USD", "notes");

    private sealed class InMemoryQuotationRepository(Quotation quotation) : IQuotationRepository
    {
        public Task AddAsync(Quotation quotation, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task<Quotation?> GetByIdAsync(Guid id, CancellationToken cancellationToken) => Task.FromResult(id == quotation.Id ? quotation : null);
        public Task<IReadOnlyList<Quotation>> ListByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken) => Task.FromResult<IReadOnlyList<Quotation>>([quotation]);
        public Task UpdateAsync(Quotation quotation, CancellationToken cancellationToken) => Task.CompletedTask;
    }

    private sealed class InMemoryQuotationRevisionRepository : IQuotationRevisionRepository
    {
        public Task AddAsync(QuotationRevision revision, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task<QuotationRevision?> GetByIdAsync(Guid quotationId, Guid revisionId, CancellationToken cancellationToken) => Task.FromResult<QuotationRevision?>(null);
        public Task<IReadOnlyList<QuotationRevision>> ListByQuotationIdAsync(Guid quotationId, CancellationToken cancellationToken) => Task.FromResult<IReadOnlyList<QuotationRevision>>([]);
    }

    private sealed class InMemoryQuotationAttachmentRepository : IQuotationAttachmentRepository
    {
        public InMemoryQuotationAttachmentRepository(params QuotationAttachment[] attachments)
        {
            Attachments = attachments.ToList();
        }

        public List<QuotationAttachment> Attachments { get; }

        public Task AddAsync(QuotationAttachment attachment, CancellationToken cancellationToken)
        {
            Attachments.Add(attachment);
            return Task.CompletedTask;
        }

        public Task<QuotationAttachment?> GetByIdAsync(Guid quotationId, Guid attachmentId, CancellationToken cancellationToken)
            => Task.FromResult(Attachments.SingleOrDefault(x => x.QuotationId == quotationId && x.Id == attachmentId && x.DeletedAt is null));

        public Task<IReadOnlyList<QuotationAttachment>> ListByQuotationIdAsync(Guid quotationId, CancellationToken cancellationToken)
            => Task.FromResult<IReadOnlyList<QuotationAttachment>>(Attachments.Where(x => x.QuotationId == quotationId && x.DeletedAt is null).ToList());

        public Task UpdateAsync(QuotationAttachment attachment, CancellationToken cancellationToken) => Task.CompletedTask;
    }

    private sealed class RecordingFileStorage : IFileStorage
    {
        public List<string> UploadedStorageKeys { get; } = [];
        public List<string> DeletedStorageKeys { get; } = [];

        public Task<string> UploadAsync(Stream stream, string path, string contentType, CancellationToken cancellationToken)
        {
            UploadedStorageKeys.Add(path);
            return Task.FromResult(path);
        }

        public Task DeleteAsync(string storageKey, CancellationToken cancellationToken)
        {
            DeletedStorageKeys.Add(storageKey);
            return Task.CompletedTask;
        }

        public Task<string> GetReadUrlAsync(string storageKey, CancellationToken cancellationToken)
            => Task.FromResult($"https://files.test/{storageKey}");

        public Task<string> GetSignedReadUrlAsync(string storageKey, TimeSpan ttl, CancellationToken cancellationToken)
            => Task.FromResult($"https://files.test/{storageKey}?ttl={(int)ttl.TotalSeconds}");
    }

    private sealed class NoOpActivityWriter : IActivityWriter
    {
        public Task WriteAsync(ActivityEntry entry, CancellationToken cancellationToken) => Task.CompletedTask;
    }

    private sealed class FakeActorContext(Guid tenantId) : IActorContext
    {
        public Guid? UserId { get; } = Guid.NewGuid();
        public Guid TenantId { get; } = tenantId;
        public string? IpAddress { get; } = "127.0.0.1";
        public string? UserAgent { get; } = "tests";
    }

    private sealed class NoOpUnitOfWork : IUnitOfWork
    {
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken) => Task.FromResult(1);
    }
}
