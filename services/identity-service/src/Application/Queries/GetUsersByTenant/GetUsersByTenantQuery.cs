using IdentityService.Application.ReadModels;
using MediatR;

namespace IdentityService.Application.Queries.GetUsersByTenant;

public sealed record GetUsersByTenantQuery(Guid TenantId) : IRequest<IReadOnlyList<UserReadModel>>;
