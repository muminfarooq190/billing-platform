namespace TravelService.Application.Queries.DraftTripConcepts;

public class DraftTripConceptListItemReadModel
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid TravelInquiryId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Destination { get; set; } = string.Empty;
    public string ConceptStatus { get; set; } = string.Empty;
    public bool IsPrimary { get; set; }
    public string? OptionLabel { get; set; }
    public DateTimeOffset? StartDate { get; set; }
    public DateTimeOffset? EndDate { get; set; }
    public int? Travellers { get; set; }
    public decimal? BudgetAmount { get; set; }
    public string? Currency { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}

public sealed class DraftTripConceptDayReadModel
{
    public int DayNumber { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Location { get; set; }
    public string? OvernightLocation { get; set; }
}

public sealed class DraftTripConceptDetailReadModel : DraftTripConceptListItemReadModel
{
    public string? Summary { get; set; }
    public string? Notes { get; set; }
    public List<DraftTripConceptDayReadModel> Days { get; set; } = [];
}
