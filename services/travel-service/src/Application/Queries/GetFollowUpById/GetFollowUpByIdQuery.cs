using MediatR;

namespace TravelService.Application.Queries.GetFollowUpById;

public sealed record GetFollowUpByIdQuery(Guid Id) : IRequest<FollowUpReadModel?>;

public sealed record FollowUpReadModel(
    Guid Id,
    Guid TenantId,
    Guid CustomerContactId,
    string CustomerName,
    string Subject,
    string Notes,
    string Priority,
    string Status,
    DateTimeOffset DueDate,
    Guid? AssignedToUserId,
    DateTimeOffset? CompletedAt,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
