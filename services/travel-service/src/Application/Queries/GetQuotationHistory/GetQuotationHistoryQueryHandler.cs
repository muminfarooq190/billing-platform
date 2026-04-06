using Dapper;
using MediatR;
using TravelService.Application.Abstractions;

namespace TravelService.Application.Queries.GetQuotationHistory;

public sealed class GetQuotationHistoryQueryHandler(IReadDbConnectionFactory connectionFactory) : IRequestHandler<GetQuotationHistoryQuery, IReadOnlyList<QuotationHistoryReadModel>>
{
    public async Task<IReadOnlyList<QuotationHistoryReadModel>> Handle(GetQuotationHistoryQuery request, CancellationToken cancellationToken)
    {
        await using var connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken) as IAsyncDisposable;
        var dbConnection = (System.Data.IDbConnection)connection!;

        var results = await dbConnection.QueryAsync<QuotationHistoryReadModel>(
            @"SELECT id,
                     quotation_id AS QuotationId,
                     tenant_id AS TenantId,
                     from_status AS FromStatus,
                     to_status AS ToStatus,
                     reason,
                     changed_by_user_id AS ChangedByUserId,
                     created_at AS CreatedAt
              FROM quotation_status_history
              WHERE tenant_id = @TenantId AND quotation_id = @QuotationId
              ORDER BY created_at DESC, id DESC",
            new { request.TenantId, request.QuotationId });

        return results.ToList().AsReadOnly();
    }
}
