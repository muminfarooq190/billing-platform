using Dapper;
using MediatR;
using TravelService.Application.Abstractions;
using TravelService.Application.Queries.QuotationRevisions;

namespace TravelService.Application.Queries.ListQuotationRevisions;

public sealed class ListQuotationRevisionsQueryHandler(IReadDbConnectionFactory connectionFactory) : IRequestHandler<ListQuotationRevisionsQuery, IReadOnlyList<QuotationRevisionSummaryReadModel>>
{
    public async Task<IReadOnlyList<QuotationRevisionSummaryReadModel>> Handle(ListQuotationRevisionsQuery request, CancellationToken cancellationToken)
    {
        await using var connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken) as IAsyncDisposable;
        var dbConnection = (System.Data.IDbConnection)connection!;

        const string sql = @"
SELECT id,
       quotation_id AS QuotationId,
       tenant_id AS TenantId,
       revision_number AS RevisionNumber,
       status,
       title,
       destination,
       travel_date AS TravelDate,
       return_date AS ReturnDate,
       travellers,
       currency,
       valid_until AS ValidUntil,
       subtotal_amount AS SubtotalAmount,
       tax_amount AS TaxAmount,
       total_amount AS TotalAmount,
       created_at AS CreatedAt
FROM quotation_revisions
WHERE tenant_id = @TenantId AND quotation_id = @QuotationId
ORDER BY revision_number DESC;";

        var results = await dbConnection.QueryAsync<QuotationRevisionSummaryReadModel>(sql, new { request.TenantId, request.QuotationId });
        return results.ToList().AsReadOnly();
    }
}
