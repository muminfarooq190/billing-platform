using Dapper;
using BillingService.Application.Abstractions;
using BillingService.Application.ReadModels;
using MediatR;

namespace BillingService.Application.Queries.GetSubscriptionByTenant;

public sealed class GetSubscriptionByTenantQueryHandler(IReadDbConnectionFactory connectionFactory) : IRequestHandler<GetSubscriptionByTenantQuery, SubscriptionReadModel?>
{
    public async Task<SubscriptionReadModel?> Handle(GetSubscriptionByTenantQuery request, CancellationToken cancellationToken)
    {
        using var connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        const string sql = "SELECT id, tenant_id AS TenantId, plan_type AS PlanType, billing_cycle AS BillingCycle, status, next_billing_date AS NextBillingDate FROM subscriptions WHERE tenant_id=@TenantId AND deleted_at IS NULL LIMIT 1;";
        return await connection.QuerySingleOrDefaultAsync<SubscriptionReadModel>(new CommandDefinition(sql, new { request.TenantId }, cancellationToken: cancellationToken));
    }
}
