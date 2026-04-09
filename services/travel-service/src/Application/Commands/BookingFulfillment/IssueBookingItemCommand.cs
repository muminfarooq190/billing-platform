using MediatR;

namespace TravelService.Application.Commands.BookingFulfillment;

public sealed record IssueBookingItemCommand(Guid TenantId, Guid BookingId, Guid ItemId, string? VoucherNumber, DateTimeOffset? IssuedAt, string? Notes) : IRequest;
