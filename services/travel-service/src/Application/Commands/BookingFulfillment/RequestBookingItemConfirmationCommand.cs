using MediatR;

namespace TravelService.Application.Commands.BookingFulfillment;

public sealed record RequestBookingItemConfirmationCommand(Guid TenantId, Guid BookingId, Guid ItemId, DateTimeOffset? ConfirmationDeadline, string? Notes) : IRequest;
