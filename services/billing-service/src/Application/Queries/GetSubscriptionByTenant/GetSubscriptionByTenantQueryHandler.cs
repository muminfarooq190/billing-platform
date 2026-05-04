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
        const string sql = "SELECT \"Id\" AS Id, \"TenantId\" AS TenantId, plan_type AS PlanType, billing_cycle AS BillingCycle, \"Status\" AS Status, start_date AS StartsAt, cancelled_at AS EndsAt, current_period_start AS CurrentPeriodStart, current_period_end AS CurrentPeriodEnd, next_billing_date AS NextBillingDate FROM subscriptions WHERE \"TenantId\"=@TenantId AND deleted_at IS NULL LIMIT 1;";
        return await connection.QuerySingleOrDefaultAsync<SubscriptionReadModel>(new CommandDefinition(sql, new { request.TenantId }, cancellationToken: cancellationToken));
    }
}
