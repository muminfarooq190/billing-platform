using MediatR;

namespace TravelService.Application.Commands.BookingFulfillment;

public sealed record ConfirmBookingItemCommand(Guid TenantId, Guid BookingId, Guid ItemId, string ConfirmationNumber, DateTimeOffset? ConfirmedAt, string? Notes) : IRequest;
