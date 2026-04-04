namespace TravelService.Api.Contracts;

public sealed record CreateFollowUpRequest(
    Guid TenantId,
    Guid CustomerContactId,
    string CustomerName,
    string Subject,
    string Notes,
    string Priority,
    DateTimeOffset DueDate,
    Guid? AssignedToUserId);
