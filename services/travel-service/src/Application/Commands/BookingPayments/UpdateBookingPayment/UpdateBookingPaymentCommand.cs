using MediatR;

namespace TravelService.Application.Commands.BookingPayments.UpdateBookingPayment;

public sealed record UpdateBookingPaymentCommand(Guid TenantId, Guid BookingId, Guid PaymentId, string? MilestoneLabel, DateTimeOffset DueDate, decimal Amount, string Currency, string? Notes) : IRequest;
