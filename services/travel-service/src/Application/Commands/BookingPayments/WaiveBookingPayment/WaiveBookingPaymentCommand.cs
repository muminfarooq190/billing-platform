using MediatR;

namespace TravelService.Application.Commands.BookingPayments.WaiveBookingPayment;

public sealed record WaiveBookingPaymentCommand(Guid TenantId, Guid BookingId, Guid PaymentId, string? Reason, Guid? ActorUserId) : IRequest;
