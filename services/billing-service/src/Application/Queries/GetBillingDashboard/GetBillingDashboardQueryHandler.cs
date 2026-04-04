using Dapper;
using BillingService.Application.Abstractions;
using BillingService.Application.ReadModels;
using MediatR;

namespace BillingService.Application.Queries.GetBillingDashboard;

public sealed class GetBillingDashboardQueryHandler(IReadDbConnectionFactory connectionFactory, ICacheService cacheService) : IRequestHandler<GetBillingDashboardQuery, BillingDashboardReadModel>
{
    public async Task<BillingDashboardReadModel> Handle(GetBillingDashboardQuery request, CancellationToken cancellationToken)
    {
        var cacheKey = $"billing:dashboard:{request.TenantId}";
        var cached = await cacheService.GetAsync<BillingDashboardReadModel>(cacheKey, cancellationToken);
        if (cached is not null) return cached;

        using var connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        const string sql = "SELECT @TenantId AS TenantId, COALESCE(SUM(CASE WHEN \"Status\"='Paid' THEN (total::jsonb ->> 'Amount')::numeric ELSE 0 END),0) AS Mrr, COALESCE(SUM(CASE WHEN \"Status\" IN ('Issued','Overdue') THEN (total::jsonb ->> 'Amount')::numeric ELSE 0 END),0) AS Outstanding, COUNT(*) FILTER (WHERE \"Status\"='Overdue')::int AS OverdueCount FROM invoices WHERE \"TenantId\"=@TenantId AND deleted_at IS NULL;";
        var dashboard = await connection.QuerySingleAsync<BillingDashboardReadModel>(new CommandDefinition(sql, new { request.TenantId }, cancellationToken: cancellationToken));
        await cacheService.SetAsync(cacheKey, dashboard, TimeSpan.FromSeconds(60), cancellationToken);
        return dashboard;
    }
}
