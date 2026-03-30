using Dapper;
using BillingService.Application.Abstractions;
using BillingService.Application.ReadModels;
using MediatR;

namespace BillingService.Application.Queries.ListInvoicesByTenant;

public sealed class ListInvoicesByTenantQueryHandler(IReadDbConnectionFactory connectionFactory) : IRequestHandler<ListInvoicesByTenantQuery, IReadOnlyList<InvoiceReadModel>>
{
    public async Task<IReadOnlyList<InvoiceReadModel>> Handle(ListInvoicesByTenantQuery request, CancellationToken cancellationToken)
    {
        using var connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        const string sql = "SELECT id, subscription_id AS SubscriptionId, tenant_id AS TenantId, status, total_amount AS TotalAmount, total_currency AS Currency, due_date AS DueDate, paid_at AS PaidAt FROM invoices WHERE tenant_id=@TenantId AND deleted_at IS NULL AND (@Status IS NULL OR status=@Status) ORDER BY created_at DESC OFFSET @Offset LIMIT @Limit;";
        var rows = await connection.QueryAsync<InvoiceReadModel>(new CommandDefinition(sql, new { request.TenantId, Status = request.Status, Offset = (request.Page - 1) * request.PageSize, Limit = request.PageSize }, cancellationToken: cancellationToken));
        return rows.ToList();
    }
}
