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
        const string sql = "SELECT id, subscription_id AS SubscriptionId, tenant_id AS TenantId, status, total_amount AS TotalAmount, total_currency AS Currency, due_date AS DueDate, paid_at AS PaidAt FROM invoices WHERE id=@InvoiceId AND deleted_at IS NULL;";
        var result = await connection.QuerySingleOrDefaultAsync<InvoiceReadModel>(new CommandDefinition(sql, new { request.InvoiceId }, cancellationToken: cancellationToken));
        if (result is not null)
        {
            await cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(5), cancellationToken);
        }

        return result;
    }
}
