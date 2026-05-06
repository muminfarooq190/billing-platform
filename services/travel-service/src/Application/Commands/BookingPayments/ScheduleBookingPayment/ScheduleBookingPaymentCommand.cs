using MediatR;

namespace TravelService.Application.Commands.BookingPayments.ScheduleBookingPayment;

public sealed record ScheduleBookingPaymentCommand(Guid TenantId, Guid BookingId, string? MilestoneLabel, DateTimeOffset DueDate, decimal Amount, string Currency, string? Notes, Guid? ActorUserId) : IRequest<Guid>;
