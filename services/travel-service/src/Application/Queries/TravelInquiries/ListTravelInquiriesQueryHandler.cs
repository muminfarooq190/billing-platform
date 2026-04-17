using System.Text;
using Dapper;
using MediatR;
using TravelService.Application.Abstractions;

namespace TravelService.Application.Queries.TravelInquiries;

public sealed class ListTravelInquiriesQueryHandler(IReadDbConnectionFactory connectionFactory) : IRequestHandler<ListTravelInquiriesQuery, IReadOnlyList<TravelInquiryListItemReadModel>>
{
    public async Task<IReadOnlyList<TravelInquiryListItemReadModel>> Handle(ListTravelInquiriesQuery request, CancellationToken cancellationToken)
    {
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        await using var connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken) as IAsyncDisposable;
        var dbConnection = (System.Data.IDbConnection)connection!;

        var sql = new StringBuilder(@"SELECT id, tenant_id AS TenantId, source, status, full_name AS FullName, email, phone, destination,
travel_date AS TravelDate, return_date AS ReturnDate, travellers, budget_amount AS BudgetAmount, budget_currency AS BudgetCurrency,
assigned_to_user_id AS AssignedToUserId, converted_quotation_id AS ConvertedQuotationId, created_at AS CreatedAt, updated_at AS UpdatedAt
FROM travel_inquiries WHERE tenant_id = @TenantId AND deleted_at IS NULL");

        if (!string.IsNullOrWhiteSpace(request.Status))
            sql.Append(" AND status = @Status");
        if (!string.IsNullOrWhiteSpace(request.Source))
            sql.Append(" AND source = @Source");
        if (request.AssignedToUserId.HasValue)
            sql.Append(" AND assigned_to_user_id = @AssignedToUserId");
        if (!string.IsNullOrWhiteSpace(request.Destination))
            sql.Append(" AND destination ILIKE @Destination");
        if (!string.IsNullOrWhiteSpace(request.Query))
            sql.Append(" AND (full_name ILIKE @Query OR email ILIKE @Query OR phone ILIKE @Query OR destination ILIKE @Query)");

        sql.Append(" ORDER BY created_at DESC OFFSET @Offset LIMIT @Limit");

        var results = await dbConnection.QueryAsync<TravelInquiryListItemReadModel>(sql.ToString(), new
        {
            request.TenantId,
            request.Status,
            request.Source,
            request.AssignedToUserId,
            Destination = string.IsNullOrWhiteSpace(request.Destination) ? null : $"%{request.Destination.Trim()}%",
            Query = string.IsNullOrWhiteSpace(request.Query) ? null : $"%{request.Query.Trim()}%",
            Offset = (page - 1) * pageSize,
            Limit = pageSize
        });

        return results.ToList().AsReadOnly();
    }
}
