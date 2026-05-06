using FluentAssertions;
using TravelService.Application.Abstractions;
using TravelService.Application.Queries.GetBookingFinancialSummary;
using TravelService.Domain.Aggregates;
using TravelService.Domain.Enums;
using TravelService.Domain.Repositories;

namespace TravelService.Tests;

public sealed class BookingFinancialSummaryTests
{
    [Fact]
    public async Task Handle_ShouldReturnPartiallyPaidSummary()
    {
        var booking = Booking.CreateFromAcceptedQuotation(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "VOY-BKG-2026-0001", "Italy Trip", "Italy", DateTimeOffset.UtcNow.AddDays(10), DateTimeOffset.UtcNow.AddDays(15), 2, "USD", 5000m);
        var payments = new List<BookingPayment>
        {
            CreatePayment(booking.TenantId, booking.Id, 2000m, "USD", BookingPaymentStatus.Paid, DateTimeOffset.UtcNow.AddDays(-5), DateTimeOffset.UtcNow.AddDays(-4)),
            CreatePayment(booking.TenantId, booking.Id, 3000m, "USD", BookingPaymentStatus.Scheduled, DateTimeOffset.UtcNow.AddDays(7), null)
        };
        var handler = new GetBookingFinancialSummaryQueryHandler(new SingleBookingRepository(booking), new StubBookingPaymentRepository(payments), new AllowAllFeatureGate());

        var result = await handler.Handle(new GetBookingFinancialSummaryQuery(booking.TenantId, booking.Id), CancellationToken.None);

        result.Should().NotBeNull();
        result!.PaidAmount.Should().Be(2000m);
        result.OutstandingAmount.Should().Be(3000m);
        result.PaymentStatus.Should().Be("PartiallyPaid");
        result.InvoiceCount.Should().Be(2);
    }

    private static BookingPayment CreatePayment(Guid tenantId, Guid bookingId, decimal amount, string currency, BookingPaymentStatus status, DateTimeOffset dueDate, DateTimeOffset? paidAt)
    {
        var payment = BookingPayment.Schedule(tenantId, bookingId, "Deposit", dueDate, amount, currency, null);
        if (status == BookingPaymentStatus.Paid)
        {
            payment.MarkPaid(paidAt, "BankTransfer", null, null, null);
        }

        return payment;
    }

    private sealed class SingleBookingRepository(Booking booking) : IBookingRepository
    {
        public Task AddAsync(Booking booking, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task<Booking?> GetByIdAsync(Guid id, CancellationToken cancellationToken) => Task.FromResult(id == booking.Id ? booking : null);
        public Task<Booking?> GetByAcceptedRevisionIdAsync(Guid acceptedRevisionId, CancellationToken cancellationToken) => Task.FromResult<Booking?>(null);
        public Task<IReadOnlyList<Booking>> ListByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken) => Task.FromResult<IReadOnlyList<Booking>>([booking]);
        public Task UpdateAsync(Booking booking, CancellationToken cancellationToken) => Task.CompletedTask;
    }

    private sealed class StubBookingPaymentRepository(IReadOnlyList<BookingPayment> payments) : IBookingPaymentRepository
    {
        public Task AddAsync(BookingPayment payment, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task<BookingPayment?> GetByIdAsync(Guid id, CancellationToken cancellationToken) => Task.FromResult(payments.SingleOrDefault(x => x.Id == id));
        public Task<IReadOnlyList<BookingPayment>> ListByBookingIdAsync(Guid bookingId, CancellationToken cancellationToken) => Task.FromResult(payments.Where(x => x.BookingId == bookingId).ToList() as IReadOnlyList<BookingPayment>);
        public Task UpdateAsync(BookingPayment payment, CancellationToken cancellationToken) => Task.CompletedTask;
    }

    private sealed class AllowAllFeatureGate : IFeatureGate
    {
        public Task EnsureEnabledAsync(string featureKey, Guid tenantId, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task EnsureEnabledAsync(string featureKey, Guid tenantId, Guid? userId, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task<bool> IsEnabledAsync(string featureKey, Guid tenantId, CancellationToken cancellationToken) => Task.FromResult(true);
        public Task<bool> IsEnabledAsync(string featureKey, Guid tenantId, Guid? userId, CancellationToken cancellationToken) => Task.FromResult(true);
        public Task<int?> GetLimitAsync(string featureKey, Guid tenantId, CancellationToken cancellationToken) => Task.FromResult<int?>(null);
        public Task<int?> GetLimitAsync(string featureKey, Guid tenantId, Guid? userId, CancellationToken cancellationToken) => Task.FromResult<int?>(null);
    }
}
