namespace TravelService.Api.Contracts;

public sealed record CreateBookingFromQuotationRequest(Guid? AssignedToUserId, string? InternalNotes);
