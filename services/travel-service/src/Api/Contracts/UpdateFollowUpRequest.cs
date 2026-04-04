namespace TravelService.Api.Contracts;

public sealed record UpdateFollowUpRequest(
    string Subject,
    string Notes,
    string Priority,
    DateTimeOffset DueDate,
    Guid? AssignedToUserId,
    string Status);
