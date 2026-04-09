using FluentAssertions;
using TravelService.Application.Abstractions;
using TravelService.Application.Commands.DeleteBookingDocument;
using TravelService.Application.Commands.UploadBookingDocument;
using TravelService.Domain.Aggregates;
using TravelService.Domain.Repositories;

namespace TravelService.Tests;

public sealed class BookingDocumentCommandTests
{
    [Fact]
    public async Task UploadBookingDocument_ShouldPersistDocument_AndUploadFile()
    {
        var booking = CreateBooking();
        var traveler = Traveler.Create(booking.Id, booking.TenantId, "Jane", "Doe", null, null, null, null, null, null, null, null, null, null, null, true);
        var documentRepository = new InMemoryBookingDocumentRepository();
        var fileStorage = new RecordingFileStorage();
        var handler = new UploadBookingDocumentCommandHandler(new InMemoryBookingRepository(booking), new InMemoryTravelerRepository(traveler), documentRepository, fileStorage, new AllowAllFeatureGate(), new NoOpActivityWriter(), new FakeActorContext(booking.TenantId), new NoOpUnitOfWork());

        var result = await handler.Handle(new UploadBookingDocumentCommand(booking.TenantId, booking.Id, traveler.Id, "voucher.pdf", "application/pdf", 128, "Voucher", true, "Customer voucher", [1,2,3]), CancellationToken.None);

        result.DocumentId.Should().NotBe(Guid.Empty);
        documentRepository.Documents.Should().ContainSingle();
        documentRepository.Documents[0].IsCustomerVisible.Should().BeTrue();
        fileStorage.UploadedStorageKeys.Should().ContainSingle();
    }

    [Fact]
    public async Task DeleteBookingDocument_ShouldSoftDeleteDocument_AndDeleteStoredFile()
    {
        var booking = CreateBooking();
        var document = BookingDocument.Create(booking.Id, null, booking.TenantId, "tenant/path/voucher.pdf", "voucher.pdf", "application/pdf", 128, "Voucher", true, null);
        var fileStorage = new RecordingFileStorage();
        var repository = new InMemoryBookingDocumentRepository(document);
        var handler = new DeleteBookingDocumentCommandHandler(new InMemoryBookingRepository(booking), repository, fileStorage, new NoOpActivityWriter(), new FakeActorContext(booking.TenantId), new NoOpUnitOfWork());

        await handler.Handle(new DeleteBookingDocumentCommand(booking.TenantId, booking.Id, document.Id), CancellationToken.None);

        fileStorage.DeletedStorageKeys.Should().ContainSingle().Which.Should().Be("tenant/path/voucher.pdf");
        (await repository.ListByBookingIdAsync(booking.Id, CancellationToken.None)).Should().BeEmpty();
    }

    private static Booking CreateBooking()
        => Booking.CreateFromAcceptedQuotation(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "VOY-BKG-2026-000001", "Rome Trip", "Rome", DateTimeOffset.UtcNow.AddDays(10), DateTimeOffset.UtcNow.AddDays(15), 2, "USD", 2500m);

    private sealed class InMemoryBookingRepository(Booking booking) : IBookingRepository
    {
        public Task AddAsync(Booking booking, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task<Booking?> GetByIdAsync(Guid id, CancellationToken cancellationToken) => Task.FromResult(id == booking.Id ? booking : null);
        public Task<Booking?> GetByAcceptedRevisionIdAsync(Guid acceptedRevisionId, CancellationToken cancellationToken) => Task.FromResult<Booking?>(null);
        public Task<IReadOnlyList<Booking>> ListByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken) => Task.FromResult<IReadOnlyList<Booking>>([booking]);
        public Task UpdateAsync(Booking booking, CancellationToken cancellationToken) => Task.CompletedTask;
    }

    private sealed class InMemoryTravelerRepository(params Traveler[] travelers) : ITravelerRepository
    {
        private readonly List<Traveler> _travelers = travelers.ToList();
        public Task AddAsync(Traveler traveler, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task<Traveler?> GetByIdAsync(Guid bookingId, Guid travelerId, CancellationToken cancellationToken) => Task.FromResult(_travelers.SingleOrDefault(x => x.BookingId == bookingId && x.Id == travelerId && x.DeletedAt is null));
        public Task<IReadOnlyList<Traveler>> ListByBookingIdAsync(Guid bookingId, CancellationToken cancellationToken) => Task.FromResult<IReadOnlyList<Traveler>>(_travelers.Where(x => x.BookingId == bookingId && x.DeletedAt is null).ToList());
        public Task UpdateAsync(Traveler traveler, CancellationToken cancellationToken) => Task.CompletedTask;
    }

    private sealed class InMemoryBookingDocumentRepository(params BookingDocument[] documents) : IBookingDocumentRepository
    {
        public List<BookingDocument> Documents { get; } = documents.ToList();
        public Task AddAsync(BookingDocument document, CancellationToken cancellationToken) { Documents.Add(document); return Task.CompletedTask; }
        public Task<BookingDocument?> GetByIdAsync(Guid bookingId, Guid documentId, CancellationToken cancellationToken) => Task.FromResult(Documents.SingleOrDefault(x => x.BookingId == bookingId && x.Id == documentId && x.DeletedAt is null));
        public Task<IReadOnlyList<BookingDocument>> ListByBookingIdAsync(Guid bookingId, CancellationToken cancellationToken) => Task.FromResult<IReadOnlyList<BookingDocument>>(Documents.Where(x => x.BookingId == bookingId && x.DeletedAt is null).ToList());
        public Task UpdateAsync(BookingDocument document, CancellationToken cancellationToken) => Task.CompletedTask;
    }

    private sealed class RecordingFileStorage : IFileStorage
    {
        public List<string> UploadedStorageKeys { get; } = [];
        public List<string> DeletedStorageKeys { get; } = [];
        public Task<string> UploadAsync(Stream stream, string path, string contentType, CancellationToken cancellationToken) { UploadedStorageKeys.Add(path); return Task.FromResult(path); }
        public Task DeleteAsync(string storageKey, CancellationToken cancellationToken) { DeletedStorageKeys.Add(storageKey); return Task.CompletedTask; }
        public Task<string> GetReadUrlAsync(string storageKey, CancellationToken cancellationToken) => Task.FromResult($"https://files.test/{storageKey}");
        public Task<string> GetSignedReadUrlAsync(string storageKey, TimeSpan ttl, CancellationToken cancellationToken) => Task.FromResult($"https://files.test/{storageKey}?ttl={(int)ttl.TotalSeconds}");
    }

    private sealed class AllowAllFeatureGate : IFeatureGate
    {
        public Task EnsureEnabledAsync(string featureKey, Guid tenantId, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task<bool> IsEnabledAsync(string featureKey, Guid tenantId, CancellationToken cancellationToken) => Task.FromResult(true);
        public Task<int?> GetLimitAsync(string featureKey, Guid tenantId, CancellationToken cancellationToken) => Task.FromResult<int?>(null);
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
