using IdentityService.Application.ReadModels;
using MediatR;

namespace IdentityService.Application.Queries.GetTenantById;

public sealed record GetTenantByIdQuery(Guid TenantId) : IRequest<TenantReadModel?>;
