using Dapper;
using MediatR;
using TravelService.Application.Abstractions;

namespace TravelService.Application.Queries.DraftTripConcepts;

public sealed class GetDraftTripConceptByIdQueryHandler(IReadDbConnectionFactory connectionFactory) : IRequestHandler<GetDraftTripConceptByIdQuery, DraftTripConceptDetailReadModel?>
{
    public async Task<DraftTripConceptDetailReadModel?> Handle(GetDraftTripConceptByIdQuery request, CancellationToken cancellationToken)
    {
        await using var connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken) as IAsyncDisposable;
        var dbConnection = (System.Data.IDbConnection)connection!;

        var concept = await dbConnection.QuerySingleOrDefaultAsync<DraftTripConceptDetailReadModel>(@"
SELECT id, tenant_id AS TenantId, travel_inquiry_id AS TravelInquiryId, title, destination, concept_status AS ConceptStatus,
       is_primary AS IsPrimary, option_label AS OptionLabel, start_date AS StartDate, end_date AS EndDate,
       travellers, budget_amount AS BudgetAmount, currency, updated_at AS UpdatedAt, summary, notes
FROM draft_trip_concepts
WHERE tenant_id = @TenantId AND travel_inquiry_id = @InquiryId AND id = @ConceptId AND deleted_at IS NULL", new { request.TenantId, request.InquiryId, request.ConceptId });

        if (concept is null)
            return null;

        var days = await dbConnection.QueryAsync<DraftTripConceptDayReadModel>(@"
SELECT day_number AS DayNumber, title, description, location, overnight_location AS OvernightLocation
FROM draft_trip_concept_days
WHERE draft_trip_concept_id = @ConceptId
ORDER BY day_number", new { request.ConceptId });

        concept.Days = days.ToList();
        return concept;
    }
}
