using Dapper;
using TravelService.Application.Abstractions;
using TravelService.Application.Queries.GetQuotationById;
using MediatR;
using System.Text;

namespace TravelService.Application.Queries.ListQuotationsByTenant;

public sealed class ListQuotationsByTenantQueryHandler(IReadDbConnectionFactory connectionFactory) : IRequestHandler<ListQuotationsByTenantQuery, IReadOnlyList<QuotationReadModel>>
{
    public async Task<IReadOnlyList<QuotationReadModel>> Handle(ListQuotationsByTenantQuery request, CancellationToken cancellationToken)
    {
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        await using var connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken) as IAsyncDisposable;
        var dbConnection = (System.Data.IDbConnection)connection!;

        var sql = new StringBuilder(@"SELECT id,
                                            tenant_id AS TenantId,
                                            customer_contact_id AS CustomerContactId,
                                            customer_name AS CustomerName,
                                            title,
                                            destination,
                                            travel_date AS TravelDate,
                                            return_date AS ReturnDate,
                                            travellers,
                                            currency,
                                            notes,
                                            status,
                                            valid_until AS ValidUntil,
                                            current_revision_number AS CurrentRevisionNumber,
                                            accepted_revision_id AS AcceptedRevisionId,
                                            COALESCE((SELECT SUM(unit_price * quantity) FROM quotation_line_items WHERE quotation_id = quotations.id), 0) AS TotalAmount,
                                            last_sent_at AS LastSentAt,
                                            last_viewed_at AS LastViewedAt,
                                            expired_at AS ExpiredAt,
                                            rejected_at AS RejectedAt,
                                            created_at AS CreatedAt,
                                            updated_at AS UpdatedAt
                                     FROM quotations
                                     WHERE tenant_id = @TenantId AND deleted_at IS NULL");

        if (!string.IsNullOrWhiteSpace(request.Status))
            sql.Append(" AND status = @Status");
        if (!string.IsNullOrWhiteSpace(request.CustomerName))
            sql.Append(" AND customer_name ILIKE @CustomerName");
        if (request.TravelDateFrom.HasValue)
            sql.Append(" AND travel_date >= @TravelDateFrom");
        if (request.TravelDateTo.HasValue)
            sql.Append(" AND travel_date <= @TravelDateTo");

        sql.Append(" ORDER BY created_at DESC OFFSET @Offset LIMIT @Limit");

        var results = await dbConnection.QueryAsync<QuotationReadModel>(sql.ToString(), new
        {
            request.TenantId,
            request.Status,
            CustomerName = string.IsNullOrWhiteSpace(request.CustomerName) ? null : $"%{request.CustomerName.Trim()}%",
            request.TravelDateFrom,
            request.TravelDateTo,
            Offset = (page - 1) * pageSize,
            Limit = pageSize
        });

        return results.ToList().AsReadOnly();
    }
}
