using MediatR;

namespace TravelService.Application.Queries.GetAuditLog;

public sealed record GetAuditLogQuery(Guid TenantId, string EntityType, Guid EntityId, int Page = 1, int PageSize = 20) : IRequest<AuditLogPageReadModel>;

public sealed record AuditLogReadModel(
    Guid Id,
    string EntityType,
    Guid EntityId,
    string Action,
    Guid? ActorUserId,
    string? IpAddress,
    string? UserAgent,
    string? BeforeJson,
    string? AfterJson,
    string? MetadataJson,
    DateTimeOffset OccurredAt);

public sealed record AuditLogPageReadModel(
    IReadOnlyList<AuditLogReadModel> Items,
    int Page,
    int PageSize,
    int TotalCount);
