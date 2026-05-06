using MediatR;

namespace TravelService.Application.Commands.BookingPayments.MarkBookingPaymentPaid;

public sealed record MarkBookingPaymentPaidCommand(Guid TenantId, Guid BookingId, Guid PaymentId, DateTimeOffset? PaidAt, string PaymentMethod, string? ProviderReference, string? Notes, Guid? ActorUserId) : IRequest;
