using IdentityService.Application.ReadModels;
using MediatR;

namespace IdentityService.Application.Queries.GetUserById;

public sealed record GetUserByIdQuery(Guid UserId) : IRequest<UserReadModel>;
