using MediatR;

namespace TravelService.Application.Queries.GetWorkQueue;

public sealed record GetWorkQueueQuery(Guid TenantId, int Page = 1, int PageSize = 20) : IRequest<IReadOnlyList<WorkQueueItemReadModel>>;

public sealed record WorkQueueItemReadModel(
    Guid Id,
    string WorkType,
    string Subject,
    string Priority,
    string Status,
    DateTimeOffset? DueDate,
    Guid? AssignedToUserId,
    string QueueState);
