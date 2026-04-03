using IdentityService.Application.ReadModels;
using MediatR;

namespace IdentityService.Application.Queries.GetTenantByEmail;

public sealed record GetTenantByEmailQuery(string Email) : IRequest<TenantReadModel?>;
