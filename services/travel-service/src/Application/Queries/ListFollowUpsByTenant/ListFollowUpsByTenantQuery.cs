using TravelService.Application.Queries.GetFollowUpById;
using MediatR;

namespace TravelService.Application.Queries.ListFollowUpsByTenant;

public sealed record ListFollowUpsByTenantQuery(Guid TenantId) : IRequest<IReadOnlyList<FollowUpReadModel>>;
