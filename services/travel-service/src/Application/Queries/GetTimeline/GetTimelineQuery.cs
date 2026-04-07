using MediatR;

namespace TravelService.Application.Queries.GetTimeline;

public sealed record GetTimelineQuery(Guid TenantId, string EntityType, Guid EntityId, int Page = 1, int PageSize = 20) : IRequest<TimelinePageReadModel>;

public sealed record ActivityEntryReadModel(
    Guid Id,
    string EntityType,
    Guid EntityId,
    string ActivityType,
    string Summary,
    string? DetailJson,
    Guid? ActorUserId,
    DateTimeOffset OccurredAt);

public sealed record TimelinePageReadModel(
    IReadOnlyList<ActivityEntryReadModel> Items,
    int Page,
    int PageSize,
    int TotalCount);
