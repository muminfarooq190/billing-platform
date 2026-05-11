using Dapper;
using MediatR;
using TravelService.Application.Abstractions;

namespace TravelService.Application.Queries.GetWorkQueue;

public sealed record ListWorkflowHubQuery(Guid TenantId, int Page = 1, int PageSize = 50) : IRequest<IReadOnlyList<WorkflowHubItemReadModel>>;

public sealed record WorkflowHubItemReadModel(
    Guid InquiryId,
    string? InquiryStatus,
    string? FullName,
    string? Email,
    string? Phone,
    string? Destination,
    DateTimeOffset? TravelDate,
    DateTimeOffset? ReturnDate,
    int? Travellers,
    decimal? BudgetAmount,
    string? BudgetCurrency,
    Guid? AssignedToUserId,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    Guid? QuotationId,
    string? QuotationStatus,
    Guid? AcceptedRevisionId,
    DateTimeOffset? QuotationUpdatedAt,
    Guid? BookingId,
    string? BookingStatus,
    Guid? ItineraryId,
    string? ItineraryStatus,
    DateTimeOffset? BookingUpdatedAt
);

public sealed class ListWorkflowHubQueryHandler(IReadDbConnectionFactory connectionFactory) : IRequestHandler<ListWorkflowHubQuery, IReadOnlyList<WorkflowHubItemReadModel>>
{
    public async Task<IReadOnlyList<WorkflowHubItemReadModel>> Handle(ListWorkflowHubQuery request, CancellationToken cancellationToken)
    {
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);
        var offset = (page - 1) * pageSize;

        await using var connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken) as IAsyncDisposable;
        var dbConnection = (System.Data.IDbConnection)connection!;

        var items = await dbConnection.QueryAsync<WorkflowHubItemReadModel>(
            @"SELECT i.id AS InquiryId,
                      i.status AS InquiryStatus,
                      i.full_name AS FullName,
                      i.email AS Email,
                      i.phone AS Phone,
                      i.destination AS Destination,
                      i.travel_date AS TravelDate,
                      i.return_date AS ReturnDate,
                      i.travellers AS Travellers,
                      i.budget_amount AS BudgetAmount,
                      i.budget_currency AS BudgetCurrency,
                      i.assigned_to_user_id AS AssignedToUserId,
                      i.created_at AS CreatedAt,
                      i.updated_at AS UpdatedAt,
                      q.id AS QuotationId,
                      q.status AS QuotationStatus,
                      q.accepted_revision_id AS AcceptedRevisionId,
                      q.updated_at AS QuotationUpdatedAt,
                      b.id AS BookingId,
                      b.status AS BookingStatus,
                      b.itinerary_id AS ItineraryId,
                      b.itinerary_status AS ItineraryStatus,
                      b.updated_at AS BookingUpdatedAt
               FROM travel_inquiries i
               LEFT JOIN quotations q ON q.id = i.converted_quotation_id AND q.deleted_at IS NULL
               LEFT JOIN (
                    SELECT bk.id,
                           bk.quotation_id,
                           bk.status,
                           last_itinerary.id AS itinerary_id,
                           last_itinerary.status AS itinerary_status,
                           bk.updated_at
                    FROM bookings bk
                    LEFT JOIN LATERAL (
                        SELECT it.id, it.status
                        FROM itineraries it
                        WHERE it.booking_id = bk.id AND it.deleted_at IS NULL
                        ORDER BY it.updated_at DESC NULLS LAST
                        LIMIT 1
                    ) last_itinerary ON TRUE
                    WHERE bk.deleted_at IS NULL
               ) b ON b.quotation_id = q.id
               WHERE i.tenant_id = @TenantId
                 AND i.deleted_at IS NULL
               ORDER BY i.created_at DESC
               LIMIT @PageSize OFFSET @Offset",
            new { request.TenantId, PageSize = pageSize, Offset = offset });

        return items.ToList().AsReadOnly();
    }
}
