using FluentAssertions;
using TravelService.Application.Abstractions;
using TravelService.Application.Commands.CreateBookingItinerary;
using TravelService.Application.Commands.CreateItinerary;
using TravelService.Domain.Aggregates;
using TravelService.Domain.Repositories;
using TravelService.Tests.TestDoubles;

namespace TravelService.Tests;

public sealed class BookingItineraryOwnershipTests
{
    [Fact]
    public async Task CreateBookingItinerary_ShouldLinkItineraryToBooking()
    {
        var booking = Booking.CreateFromAcceptedQuotation(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "VOY-BKG-2026-000001", "Bali Escape", "Bali", DateTimeOffset.UtcNow.AddDays(20), DateTimeOffset.UtcNow.AddDays(25), 2, "USD", 1200m);
        var bookingRepository = new InMemoryBookingRepository(booking);
        var itineraryRepository = new InMemoryItineraryRepository();
        var handler = new CreateBookingItineraryCommandHandler(bookingRepository, itineraryRepository, new NoOpCommunicationWorkflowClient(), new NoOpActivityWriter(), new NoOpUnitOfWork());

        var itineraryId = await handler.Handle(new CreateBookingItineraryCommand(
            booking.TenantId,
            booking.Id,
            "Bali Confirmed Plan",
            booking.Destination,
            booking.StartDate,
            booking.EndDate,
            booking.TravellersCount,
            booking.Currency,
            [new ItineraryItemDto(1, "Other", "Arrival", "Airport pickup", "Bali", null, null, 0m, "USD")]), CancellationToken.None);

        itineraryId.Should().NotBeEmpty();
        itineraryRepository.Items.Should().ContainSingle();
        itineraryRepository.Items[0].BookingId.Should().Be(booking.Id);
    }

    [Fact]
    public async Task LegacyCreateItinerary_ShouldRemainQuoteLinkedButNotBookingLinked()
    {
        var quotation = Quotation.Create(Guid.NewGuid(), Guid.NewGuid(), "Jane Doe", "Bali Quote", "Bali", DateTimeOffset.UtcNow.AddDays(20), DateTimeOffset.UtcNow.AddDays(25), 2, "USD", "notes");
        var itineraryRepository = new InMemoryItineraryRepository();
        var handler = new CreateItineraryCommandHandler(itineraryRepository, new InMemoryQuotationRepository(quotation), new NoOpUnitOfWork());

        var itineraryId = await handler.Handle(new CreateItineraryCommand(
            quotation.TenantId,
            quotation.CustomerContactId,
            quotation.CustomerName,
            quotation.Title,
            quotation.Destination,
            quotation.TravelDate,
            quotation.ReturnDate,
            quotation.Travellers,
            quotation.Currency,
            quotation.Id,
            [new ItineraryItemDto(1, "Other", "Draft idea", "Draft plan", "Bali", null, null, 0m, "USD")]), CancellationToken.None);

        itineraryId.Should().NotBeEmpty();
        itineraryRepository.Items.Should().ContainSingle();
        itineraryRepository.Items[0].QuotationId.Should().Be(quotation.Id);
        itineraryRepository.Items[0].BookingId.Should().BeNull();
    }

    private sealed class InMemoryBookingRepository(Booking booking) : IBookingRepository
    {
        public Task AddAsync(Booking booking, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task<Booking?> GetByIdAsync(Guid id, CancellationToken cancellationToken) => Task.FromResult(id == booking.Id ? booking : null);
        public Task<Booking?> GetByAcceptedRevisionIdAsync(Guid acceptedRevisionId, CancellationToken cancellationToken) => Task.FromResult<Booking?>(null);
        public Task<IReadOnlyList<Booking>> ListByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken) => Task.FromResult<IReadOnlyList<Booking>>([booking]);
        public Task UpdateAsync(Booking booking, CancellationToken cancellationToken) => Task.CompletedTask;
    }

    private sealed class InMemoryQuotationRepository(Quotation quotation) : IQuotationRepository
    {
        public Task AddAsync(Quotation quotation, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task<Quotation?> GetByIdAsync(Guid id, CancellationToken cancellationToken) => Task.FromResult(id == quotation.Id ? quotation : null);
        public Task<IReadOnlyList<Quotation>> ListByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken) => Task.FromResult<IReadOnlyList<Quotation>>([quotation]);
        public Task UpdateAsync(Quotation quotation, CancellationToken cancellationToken) => Task.CompletedTask;
    }

    private sealed class InMemoryItineraryRepository : IItineraryRepository
    {
        public List<Itinerary> Items { get; } = [];
        public Task AddAsync(Itinerary itinerary, CancellationToken cancellationToken)
        {
            Items.Add(itinerary);
            return Task.CompletedTask;
        }
        public Task<Itinerary?> GetByIdAsync(Guid id, CancellationToken cancellationToken) => Task.FromResult(Items.SingleOrDefault(x => x.Id == id));
        public Task<IReadOnlyList<Itinerary>> ListByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken) => Task.FromResult<IReadOnlyList<Itinerary>>(Items.Where(x => x.TenantId == tenantId).ToList());
        public Task UpdateAsync(Itinerary itinerary, CancellationToken cancellationToken) => Task.CompletedTask;
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
