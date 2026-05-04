using MediatR;

namespace TravelService.Application.Queries.BookingPayments;

public sealed record ListBookingPaymentsQuery(Guid TenantId, Guid BookingId) : IRequest<IReadOnlyList<BookingPaymentReadModel>>;
