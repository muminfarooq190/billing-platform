using BillingService.Application.ReadModels;
using MediatR;

namespace BillingService.Application.Queries.GetMyFeatureAccess;

public sealed record GetMyFeatureAccessQuery(Guid TenantId, Guid UserId) : IRequest<IReadOnlyList<UserFeatureAccessReadModel>>;
