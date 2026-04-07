using FluentAssertions;
using TravelService.Application.Abstractions;
using TravelService.Application.Commands.AcceptQuotation;
using TravelService.Application.Commands.CreateQuotationRevision;
using TravelService.Application.Commands.DeleteQuotationAttachment;
using TravelService.Application.Commands.MarkPublicQuotationViewed;
using TravelService.Application.Commands.SendQuotation;
using TravelService.Domain.Aggregates;
using TravelService.Domain.Exceptions;
using TravelService.Domain.Repositories;

namespace TravelService.Tests;

public sealed class QuotationChecklistCoverageTests
{
    [Fact]
    public async Task CreateRevision_ListRevisions_SendQuote_ShouldSupportHappyPathFlow()
    {
        var quotation = CreateQuotation();
        quotation.AddLineItem("Flight", 1200m, 1, "USD");

        var quotationRepository = new InMemoryQuotationRepository(quotation);
        var revisionRepository = new InMemoryQuotationRevisionRepository();
        var createRevisionHandler = new CreateQuotationRevisionCommandHandler(quotationRepository, revisionRepository, new NoOpActivityWriter(), new NoOpUnitOfWork());

        var createResult = await createRevisionHandler.Handle(new CreateQuotationRevisionCommand(
            quotation.TenantId,
            quotation.Id,
            "Summer Europe Trip - Premium",
            "Italy",
            DateTimeOffset.UtcNow.AddDays(30),
            DateTimeOffset.UtcNow.AddDays(37),
            2,
            "USD",
            "Breakfast included",
            "Internal margin note",
            DateTimeOffset.UtcNow.AddDays(14),
            [new QuotationRevisionLineItemDto("Flight + Hotel", 2500m, 1, "USD")]), CancellationToken.None);

        revisionRepository.Revisions.Should().ContainSingle();
        quotation.CurrentRevisionNumber.Should().Be(1);

        var shareLinkRepository = new InMemoryQuotationShareLinkRepository();
        var historyRepository = new InMemoryQuotationStatusHistoryRepository();
        var sendHandler = new SendQuotationCommandHandler(quotationRepository, revisionRepository, shareLinkRepository, historyRepository, new NoOpUnitOfWork());

        var sendResult = await sendHandler.Handle(new SendQuotationCommand(
            quotation.TenantId,
            quotation.Id,
            createResult.RevisionId,
            "Email",
            "traveler@example.com",
            "Please review your updated proposal.",
            DateTimeOffset.UtcNow.AddDays(7)), CancellationToken.None);

        sendResult.Token.Should().NotBeNullOrWhiteSpace();
        quotation.Status.ToString().Should().Be("Sent");
        historyRepository.Items.Should().ContainSingle(x => x.ToStatus == "Sent");
    }

    [Fact]
    public async Task SendQuotation_ShouldRejectRevisionFromAnotherTenant()
    {
        var quotation = CreateQuotation();
        quotation.AddLineItem("Flight", 1200m, 1, "USD");
        var revision = quotation.CreateRevision("Visible", "Internal");

        var foreignRevision = QuotationRevision.Create(
            quotation.Id,
            Guid.NewGuid(),
            1,
            "Draft",
            quotation.CustomerContactId,
            quotation.CustomerName,
            quotation.Title,
            quotation.Destination,
            quotation.TravelDate,
            quotation.ReturnDate,
            quotation.Travellers,
            quotation.Currency,
            quotation.Notes,
            "Visible",
            "Internal",
            quotation.ValidUntil,
            null,
            revision.LineItems.ToList());

        var handler = new SendQuotationCommandHandler(
            new InMemoryQuotationRepository(quotation),
            new InMemoryQuotationRevisionRepository(foreignRevision),
            new InMemoryQuotationShareLinkRepository(),
            new InMemoryQuotationStatusHistoryRepository(),
            new NoOpUnitOfWork());

        var act = async () => await handler.Handle(new SendQuotationCommand(
            quotation.TenantId,
            quotation.Id,
            foreignRevision.Id,
            "Email",
            "traveler@example.com",
            null,
            DateTimeOffset.UtcNow.AddDays(2)), CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>().WithMessage("*active tenant*");
    }

    [Fact]
    public async Task AcceptQuotation_ShouldUpdateQuoteToSpecificRevision()
    {
        var quotation = CreateQuotation();
        quotation.AddLineItem("Flight", 1200m, 1, "USD");
        var revision = quotation.CreateRevision("Visible", "Internal");
        quotation.Send();

        var handler = new AcceptQuotationCommandHandler(
            new InMemoryQuotationRepository(quotation),
            new InMemoryQuotationRevisionRepository(revision),
            new InMemoryQuotationStatusHistoryRepository(),
            new NoOpUnitOfWork());

        await handler.Handle(new AcceptQuotationCommand(quotation.TenantId, quotation.Id, revision.Id, "Approved by customer"), CancellationToken.None);

        quotation.AcceptedRevisionId.Should().Be(revision.Id);
        quotation.Status.ToString().Should().Be("Accepted");
    }

    [Fact]
    public async Task PublicQuotationViewed_ShouldFailForExpiredToken()
    {
        var quotation = CreateQuotation();
        quotation.AddLineItem("Flight", 300m, 1, "USD");
        var revision = quotation.CreateRevision("Visible", "Internal");
        quotation.SetShareToken("expired-token", DateTimeOffset.UtcNow.AddDays(-1));
        quotation.Send();

        var expiredLink = QuotationShareLink.Create(quotation.Id, revision.Id, quotation.TenantId, "expired-token", DateTimeOffset.UtcNow.AddDays(-1));
        var handler = new MarkPublicQuotationViewedCommandHandler(
            new InMemoryQuotationShareLinkRepository(expiredLink),
            new InMemoryQuotationRepository(quotation),
            new InMemoryQuotationStatusHistoryRepository(),
            new NoOpUnitOfWork());

        var updated = await handler.Handle(new MarkPublicQuotationViewedCommand("expired-token"), CancellationToken.None);

        updated.Should().BeFalse();
        quotation.LastViewedAt.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAttachment_ShouldSoftDeleteAndHideAttachmentFromRepositoryList()
    {
        var quotation = CreateQuotation();
        var attachment = QuotationAttachment.Create(
            quotation.Id,
            null,
            quotation.TenantId,
            "tenant/path/brochure.pdf",
            "brochure.pdf",
            "application/pdf",
            120,
            "Pdf",
            "Customer brochure",
            true,
            1);

        var attachmentRepository = new InMemoryQuotationAttachmentRepository(attachment);
        var handler = new DeleteQuotationAttachmentCommandHandler(
            new InMemoryQuotationRepository(quotation),
            attachmentRepository,
            new StubFileStorage(),
            new NoOpUnitOfWork());

        await handler.Handle(new DeleteQuotationAttachmentCommand(quotation.TenantId, quotation.Id, attachment.Id), CancellationToken.None);

        var remaining = await attachmentRepository.ListByQuotationIdAsync(quotation.Id, CancellationToken.None);
        remaining.Should().BeEmpty();
    }

    private static Quotation CreateQuotation()
        => Quotation.Create(Guid.NewGuid(), Guid.NewGuid(), "Ava", "Summer Trip", "Istanbul", DateTimeOffset.UtcNow.AddDays(20), DateTimeOffset.UtcNow.AddDays(25), 2, "USD", "notes");

    private sealed class StubFileStorage : IFileStorage
    {
        public Task<string> UploadAsync(Stream stream, string path, string contentType, CancellationToken cancellationToken) => Task.FromResult(path);
        public Task DeleteAsync(string storageKey, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task<string> GetReadUrlAsync(string storageKey, CancellationToken cancellationToken) => Task.FromResult($"https://files.test/{storageKey}");
        public Task<string> GetSignedReadUrlAsync(string storageKey, TimeSpan ttl, CancellationToken cancellationToken) => Task.FromResult($"https://files.test/{storageKey}?ttl={(int)ttl.TotalSeconds}");
    }

    private sealed class InMemoryQuotationRepository(Quotation quotation) : IQuotationRepository
    {
        public Task AddAsync(Quotation quotation, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task<Quotation?> GetByIdAsync(Guid id, CancellationToken cancellationToken) => Task.FromResult(id == quotation.Id ? quotation : null);
        public Task<IReadOnlyList<Quotation>> ListByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken) => Task.FromResult<IReadOnlyList<Quotation>>([quotation]);
        public Task UpdateAsync(Quotation quotation, CancellationToken cancellationToken) => Task.CompletedTask;
    }

    private sealed class InMemoryQuotationRevisionRepository(params QuotationRevision[] revisions) : IQuotationRevisionRepository
    {
        public List<QuotationRevision> Revisions { get; } = revisions.ToList();

        public Task AddAsync(QuotationRevision revision, CancellationToken cancellationToken)
        {
            Revisions.Add(revision);
            return Task.CompletedTask;
        }

        public Task<QuotationRevision?> GetByIdAsync(Guid quotationId, Guid revisionId, CancellationToken cancellationToken)
            => Task.FromResult(Revisions.SingleOrDefault(x => x.QuotationId == quotationId && x.Id == revisionId));

        public Task<IReadOnlyList<QuotationRevision>> ListByQuotationIdAsync(Guid quotationId, CancellationToken cancellationToken)
            => Task.FromResult<IReadOnlyList<QuotationRevision>>(Revisions.Where(x => x.QuotationId == quotationId).ToList());
    }

    private sealed class InMemoryQuotationShareLinkRepository(params QuotationShareLink[] shareLinks) : IQuotationShareLinkRepository
    {
        private readonly List<QuotationShareLink> _shareLinks = shareLinks.ToList();

        public Task AddAsync(QuotationShareLink shareLink, CancellationToken cancellationToken)
        {
            _shareLinks.Add(shareLink);
            return Task.CompletedTask;
        }

        public Task<QuotationShareLink?> GetActiveByTokenAsync(string token, CancellationToken cancellationToken)
            => Task.FromResult(_shareLinks.SingleOrDefault(x => x.Token == token && x.RevokedAt is null));

        public Task<IReadOnlyList<QuotationShareLink>> ListByQuotationIdAsync(Guid quotationId, CancellationToken cancellationToken)
            => Task.FromResult<IReadOnlyList<QuotationShareLink>>(_shareLinks.Where(x => x.QuotationId == quotationId).ToList());

        public Task UpdateAsync(QuotationShareLink shareLink, CancellationToken cancellationToken) => Task.CompletedTask;
    }

    private sealed class InMemoryQuotationAttachmentRepository(params QuotationAttachment[] attachments) : IQuotationAttachmentRepository
    {
        private readonly List<QuotationAttachment> _attachments = attachments.ToList();

        public Task AddAsync(QuotationAttachment attachment, CancellationToken cancellationToken)
        {
            _attachments.Add(attachment);
            return Task.CompletedTask;
        }

        public Task<QuotationAttachment?> GetByIdAsync(Guid quotationId, Guid attachmentId, CancellationToken cancellationToken)
            => Task.FromResult(_attachments.SingleOrDefault(x => x.QuotationId == quotationId && x.Id == attachmentId && x.DeletedAt is null));

        public Task<IReadOnlyList<QuotationAttachment>> ListByQuotationIdAsync(Guid quotationId, CancellationToken cancellationToken)
            => Task.FromResult<IReadOnlyList<QuotationAttachment>>(_attachments.Where(x => x.QuotationId == quotationId && x.DeletedAt is null).ToList());

        public Task UpdateAsync(QuotationAttachment attachment, CancellationToken cancellationToken) => Task.CompletedTask;
    }

    private sealed class InMemoryQuotationStatusHistoryRepository : IQuotationStatusHistoryRepository
    {
        public List<QuotationStatusHistory> Items { get; } = [];
        public Task AddAsync(QuotationStatusHistory history, CancellationToken cancellationToken)
        {
            Items.Add(history);
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<QuotationStatusHistory>> ListByQuotationIdAsync(Guid quotationId, CancellationToken cancellationToken)
            => Task.FromResult<IReadOnlyList<QuotationStatusHistory>>(Items.Where(x => x.QuotationId == quotationId).ToList());
    }

    private sealed class NoOpActivityWriter : IActivityWriter
    {
        public Task WriteAsync(ActivityEntry entry, CancellationToken cancellationToken) => Task.CompletedTask;
    }

    private sealed class NoOpUnitOfWork : IUnitOfWork
    {
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken) => Task.FromResult(1);
    }
}
