using Dapper;
using MediatR;
using TravelService.Application.Abstractions;

namespace TravelService.Application.Queries.DraftTripConcepts;

public sealed class ListDraftTripConceptsByInquiryQueryHandler(IReadDbConnectionFactory connectionFactory) : IRequestHandler<ListDraftTripConceptsByInquiryQuery, IReadOnlyList<DraftTripConceptListItemReadModel>>
{
    public async Task<IReadOnlyList<DraftTripConceptListItemReadModel>> Handle(ListDraftTripConceptsByInquiryQuery request, CancellationToken cancellationToken)
    {
        await using var connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken) as IAsyncDisposable;
        var dbConnection = (System.Data.IDbConnection)connection!;

        var rows = await dbConnection.QueryAsync<DraftTripConceptListItemReadModel>(@"
SELECT id, tenant_id AS TenantId, travel_inquiry_id AS TravelInquiryId, title, destination, concept_status AS ConceptStatus,
       is_primary AS IsPrimary, option_label AS OptionLabel, start_date AS StartDate, end_date AS EndDate,
       travellers, budget_amount AS BudgetAmount, currency, updated_at AS UpdatedAt
FROM draft_trip_concepts
WHERE tenant_id = @TenantId AND travel_inquiry_id = @InquiryId AND deleted_at IS NULL
ORDER BY is_primary DESC, updated_at DESC", new { request.TenantId, request.InquiryId });

        return rows.ToList().AsReadOnly();
    }
}
