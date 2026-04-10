using FluentAssertions;
using TravelService.Application.Abstractions;
using TravelService.Application.Queries.GetBookingFinancialSummary;
using TravelService.Domain.Aggregates;
using TravelService.Domain.Repositories;

namespace TravelService.Tests;

public sealed class BookingFinancialSummaryTests
{
    [Fact]
    public async Task Handle_ShouldReturnPartiallyPaidSummary()
    {
        var booking = Booking.CreateFromAcceptedQuotation(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "VOY-BKG-2026-0001", "Italy Trip", "Italy", DateTimeOffset.UtcNow.AddDays(10), DateTimeOffset.UtcNow.AddDays(15), 2, "USD", 5000m);
        var invoices = new List<BookingInvoiceDto>
        {
            new(Guid.NewGuid(), booking.TenantId, "Paid", 2000m, "USD", DateTimeOffset.UtcNow.AddDays(-5), DateTimeOffset.UtcNow.AddDays(-4)),
            new(Guid.NewGuid(), booking.TenantId, "Issued", 3000m, "USD", DateTimeOffset.UtcNow.AddDays(7), null)
        };
        var handler = new GetBookingFinancialSummaryQueryHandler(new SingleBookingRepository(booking), new StubBillingFinanceClient(invoices), new AllowAllFeatureGate());

        var result = await handler.Handle(new GetBookingFinancialSummaryQuery(booking.TenantId, booking.Id), CancellationToken.None);

        result.Should().NotBeNull();
        result!.PaidAmount.Should().Be(2000m);
        result.OutstandingAmount.Should().Be(3000m);
        result.PaymentStatus.Should().Be("PartiallyPaid");
        result.InvoiceCount.Should().Be(2);
    }

    private sealed class SingleBookingRepository(Booking booking) : IBookingRepository
    {
        public Task AddAsync(Booking booking, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task<Booking?> GetByIdAsync(Guid id, CancellationToken cancellationToken) => Task.FromResult(id == booking.Id ? booking : null);
        public Task<Booking?> GetByAcceptedRevisionIdAsync(Guid acceptedRevisionId, CancellationToken cancellationToken) => Task.FromResult<Booking?>(null);
        public Task<IReadOnlyList<Booking>> ListByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken) => Task.FromResult<IReadOnlyList<Booking>>([booking]);
        public Task UpdateAsync(Booking booking, CancellationToken cancellationToken) => Task.CompletedTask;
    }

    private sealed class StubBillingFinanceClient(IReadOnlyList<BookingInvoiceDto> invoices) : IBillingFinanceClient
    {
        public Task<IReadOnlyList<BookingInvoiceDto>> GetInvoicesAsync(Guid tenantId, CancellationToken cancellationToken)
            => Task.FromResult(invoices.Where(x => x.TenantId == tenantId).ToList() as IReadOnlyList<BookingInvoiceDto>);
    }

    private sealed class AllowAllFeatureGate : IFeatureGate
    {
        public Task EnsureEnabledAsync(string featureKey, Guid tenantId, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task<bool> IsEnabledAsync(string featureKey, Guid tenantId, CancellationToken cancellationToken) => Task.FromResult(true);
        public Task<int?> GetLimitAsync(string featureKey, Guid tenantId, CancellationToken cancellationToken) => Task.FromResult<int?>(null);
    }
}
