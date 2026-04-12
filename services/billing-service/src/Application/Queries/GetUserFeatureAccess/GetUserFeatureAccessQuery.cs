using BillingService.Application.ReadModels;
using MediatR;

namespace BillingService.Application.Queries.GetUserFeatureAccess;

public sealed record GetUserFeatureAccessQuery(Guid TenantId, Guid UserId) : IRequest<IReadOnlyList<UserFeatureAccessReadModel>>;
