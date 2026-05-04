using MediatR;

namespace TravelService.Application.Commands.BookingPayments.RefundBookingPayment;

public sealed record RefundBookingPaymentCommand(Guid TenantId, Guid BookingId, Guid PaymentId, string? Notes, Guid? ActorUserId) : IRequest;
