using Dapper;
using MediatR;
using TravelService.Application.Abstractions;

namespace TravelService.Application.Queries.TravelInquiries;

public sealed class GetTravelInquiryHistoryQueryHandler(IReadDbConnectionFactory connectionFactory) : IRequestHandler<GetTravelInquiryHistoryQuery, IReadOnlyList<TravelInquiryHistoryReadModel>>
{
    public async Task<IReadOnlyList<TravelInquiryHistoryReadModel>> Handle(GetTravelInquiryHistoryQuery request, CancellationToken cancellationToken)
    {
        await using var connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken) as IAsyncDisposable;
        var dbConnection = (System.Data.IDbConnection)connection!;

        var rows = await dbConnection.QueryAsync<TravelInquiryHistoryReadModel>(@"
SELECT id, travel_inquiry_id AS TravelInquiryId, tenant_id AS TenantId, from_status AS FromStatus, to_status AS ToStatus,
       reason, changed_by_user_id AS ChangedByUserId, created_at AS CreatedAt
FROM travel_inquiry_status_history
WHERE tenant_id = @TenantId AND travel_inquiry_id = @InquiryId
ORDER BY created_at DESC", new { request.TenantId, request.InquiryId });

        return rows.ToList().AsReadOnly();
    }
}
