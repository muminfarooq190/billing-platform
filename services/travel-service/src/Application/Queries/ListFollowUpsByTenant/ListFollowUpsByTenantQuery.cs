using TravelService.Application.Queries.GetFollowUpById;
using MediatR;

namespace TravelService.Application.Queries.ListFollowUpsByTenant;

public sealed record ListFollowUpsByTenantQuery(
    Guid TenantId,
    int Page = 1,
    int PageSize = 20,
    string? Status = null,
    string? CustomerName = null,
    DateTimeOffset? DueDateFrom = null,
    DateTimeOffset? DueDateTo = null)
    : IRequest<IReadOnlyList<FollowUpReadModel>>;
