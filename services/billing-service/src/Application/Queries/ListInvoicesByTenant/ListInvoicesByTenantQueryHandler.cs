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
        const string sql = "SELECT \"Id\" AS Id, \"SubscriptionId\" AS SubscriptionId, \"TenantId\" AS TenantId, \"Status\" AS Status, (total::jsonb ->> 'Amount')::numeric AS TotalAmount, total::jsonb ->> 'Currency' AS Currency, due_date AS DueDate, paid_at AS PaidAt FROM invoices WHERE \"TenantId\"=@TenantId AND deleted_at IS NULL AND (@Status IS NULL OR \"Status\"=@Status) ORDER BY created_at DESC OFFSET @Offset LIMIT @Limit;";
        var rows = await connection.QueryAsync<InvoiceReadModel>(new CommandDefinition(sql, new { request.TenantId, Status = request.Status, Offset = (request.Page - 1) * request.PageSize, Limit = request.PageSize }, cancellationToken: cancellationToken));
        return rows.ToList();
    }
}
