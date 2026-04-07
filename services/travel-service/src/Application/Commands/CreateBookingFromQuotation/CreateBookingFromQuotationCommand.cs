using MediatR;

namespace TravelService.Application.Commands.CreateBookingFromQuotation;

public sealed record CreateBookingFromQuotationCommand(Guid TenantId, Guid QuotationId, Guid? AssignedToUserId, string? InternalNotes) : IRequest<CreateBookingFromQuotationResult>;

public sealed record CreateBookingFromQuotationResult(Guid BookingId, string BookingNumber);
