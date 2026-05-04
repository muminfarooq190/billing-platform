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
        const string sql = "SELECT @TenantId AS TenantId, COALESCE(SUM(CASE WHEN \"Status\"='Paid' THEN (total::jsonb ->> 'Amount')::numeric ELSE 0 END),0) AS TotalRevenue, COALESCE(SUM(CASE WHEN \"Status\" IN ('Issued','Overdue') THEN (total::jsonb ->> 'Amount')::numeric ELSE 0 END),0) AS OutstandingAmount, COALESCE(SUM(CASE WHEN \"Status\"='Overdue' THEN (total::jsonb ->> 'Amount')::numeric ELSE 0 END),0) AS OverdueAmount, COUNT(*) FILTER (WHERE \"Status\"='Paid')::int AS PaidInvoicesCount, COUNT(*) FILTER (WHERE \"Status\" IN ('Issued','Overdue'))::int AS UnpaidInvoicesCount, MAX(total::jsonb ->> 'Currency') AS Currency FROM invoices WHERE \"TenantId\"=@TenantId AND deleted_at IS NULL;";
        var dashboard = await connection.QuerySingleAsync<BillingDashboardReadModel>(new CommandDefinition(sql, new { request.TenantId }, cancellationToken: cancellationToken));
        await cacheService.SetAsync(cacheKey, dashboard, TimeSpan.FromSeconds(60), cancellationToken);
        return dashboard;
    }
}
