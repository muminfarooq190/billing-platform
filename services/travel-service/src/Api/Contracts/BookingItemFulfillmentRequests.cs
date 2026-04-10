namespace TravelService.Api.Contracts;

public sealed record RequestBookingItemConfirmationRequest(DateTimeOffset? ConfirmationDeadline, string? Notes);
public sealed record ConfirmBookingItemRequest(string ConfirmationNumber, DateTimeOffset? ConfirmedAt, string? Notes);
public sealed record IssueBookingItemRequest(string? VoucherNumber, DateTimeOffset? IssuedAt, string? Notes);
