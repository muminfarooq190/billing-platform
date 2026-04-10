namespace TravelService.Api.Contracts;

public sealed record CreateBookingChangeRequestRequest(string ChangeType, string Reason);
public sealed record DecideBookingChangeRequestRequest(string? Reason);
