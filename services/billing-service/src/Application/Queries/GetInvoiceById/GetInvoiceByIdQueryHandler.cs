using Dapper;
using BillingService.Application.Abstractions;
using BillingService.Application.ReadModels;
using MediatR;

namespace BillingService.Application.Queries.GetInvoiceById;

public sealed class GetInvoiceByIdQueryHandler(IReadDbConnectionFactory connectionFactory, ICacheService cacheService) : IRequestHandler<GetInvoiceByIdQuery, InvoiceReadModel?>
{
    public async Task<InvoiceReadModel?> Handle(GetInvoiceByIdQuery request, CancellationToken cancellationToken)
    {
        var cacheKey = $"billing:invoice:{request.InvoiceId}";
        var cached = await cacheService.GetAsync<InvoiceReadModel>(cacheKey, cancellationToken);
        if (cached is not null) return cached;

        using var connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        const string sql = "SELECT \"Id\" AS Id, \"SubscriptionId\" AS SubscriptionId, \"TenantId\" AS TenantId, invoice_number AS InvoiceNumber, \"Status\" AS Status, (total::jsonb ->> 'Amount')::numeric AS TotalAmount, CASE WHEN \"Status\"='Paid' THEN (total::jsonb ->> 'Amount')::numeric ELSE 0 END AS PaidAmount, CASE WHEN \"Status\"='Paid' THEN 0 ELSE (total::jsonb ->> 'Amount')::numeric END AS DueAmount, total::jsonb ->> 'Currency' AS Currency, due_date AS DueDate, paid_at AS PaidAt FROM invoices WHERE \"Id\"=@InvoiceId AND deleted_at IS NULL;";
        var result = await connection.QuerySingleOrDefaultAsync<InvoiceReadModel>(new CommandDefinition(sql, new { request.InvoiceId }, cancellationToken: cancellationToken));
        if (result is not null)
        {
            await cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(5), cancellationToken);
        }

        return result;
    }
}
