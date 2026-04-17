using TravelService.Domain.Common;
using TravelService.Domain.Enums;
using TravelService.Domain.Exceptions;

namespace TravelService.Domain.Aggregates;

public sealed class DraftTripConcept : AggregateRoot
{
    private readonly List<DraftTripConceptDay> _days = [];

    private DraftTripConcept() { }

    private DraftTripConcept(
        Guid tenantId,
        Guid travelInquiryId,
        string title,
        string destination,
        string? summary,
        DateTimeOffset? startDate,
        DateTimeOffset? endDate,
        int? travellers,
        string? currency,
        decimal? budgetAmount,
        string? optionLabel,
        string? notes,
        Guid? createdByUserId)
    {
        if (tenantId == Guid.Empty)
            throw new DomainException("TenantId is required.");
        if (travelInquiryId == Guid.Empty)
            throw new DomainException("Travel inquiry id is required.");
        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException("Concept title is required.");
        if (string.IsNullOrWhiteSpace(destination))
            throw new DomainException("Concept destination is required.");
        if (startDate.HasValue && endDate.HasValue && endDate.Value < startDate.Value)
            throw new DomainException("Concept end date must be on or after start date.");
        if (travellers.HasValue && travellers.Value <= 0)
            throw new DomainException("Travellers must be greater than zero when provided.");
        if (budgetAmount.HasValue && budgetAmount.Value < 0)
            throw new DomainException("Budget amount cannot be negative.");

        Id = Guid.NewGuid();
        TenantId = tenantId;
        TravelInquiryId = travelInquiryId;
        Title = title.Trim();
        Destination = destination.Trim();
        Summary = Normalize(summary);
        StartDate = startDate;
        EndDate = endDate;
        Travellers = travellers;
        Currency = NormalizeCurrency(currency);
        BudgetAmount = budgetAmount;
        ConceptStatus = DraftTripConceptStatus.Draft;
        IsPrimary = false;
        OptionLabel = Normalize(optionLabel);
        Notes = Normalize(notes);
        CreatedByUserId = createdByUserId;
        CreatedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public Guid TravelInquiryId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Destination { get; private set; } = string.Empty;
    public string? Summary { get; private set; }
    public DateTimeOffset? StartDate { get; private set; }
    public DateTimeOffset? EndDate { get; private set; }
    public int? Travellers { get; private set; }
    public string? Currency { get; private set; }
    public decimal? BudgetAmount { get; private set; }
    public DraftTripConceptStatus ConceptStatus { get; private set; }
    public bool IsPrimary { get; private set; }
    public string? OptionLabel { get; private set; }
    public string? Notes { get; private set; }
    public Guid? CreatedByUserId { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public DateTimeOffset? DeletedAt { get; private set; }
    public IReadOnlyCollection<DraftTripConceptDay> Days => _days.AsReadOnly();

    public static DraftTripConcept Create(
        Guid tenantId,
        Guid travelInquiryId,
        string title,
        string destination,
        string? summary,
        DateTimeOffset? startDate,
        DateTimeOffset? endDate,
        int? travellers,
        string? currency,
        decimal? budgetAmount,
        string? optionLabel,
        string? notes,
        Guid? createdByUserId)
        => new(tenantId, travelInquiryId, title, destination, summary, startDate, endDate, travellers, currency, budgetAmount, optionLabel, notes, createdByUserId);

    public void AddDay(int dayNumber, string title, string? description, string? location, string? overnightLocation)
    {
        if (dayNumber <= 0)
            throw new DomainException("Day number must be greater than zero.");
        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException("Concept day title is required.");

        _days.Add(DraftTripConceptDay.Create(dayNumber, title, description, location, overnightLocation));
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void MarkPrimary()
    {
        if (ConceptStatus == DraftTripConceptStatus.Archived)
            throw new DomainException("Archived concepts cannot be marked as primary.");

        IsPrimary = true;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void ClearPrimary()
    {
        if (!IsPrimary)
            return;

        IsPrimary = false;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void MarkReadyForQuote()
    {
        if (ConceptStatus == DraftTripConceptStatus.Archived)
            throw new DomainException("Archived concepts cannot move to ReadyForQuote.");

        ConceptStatus = DraftTripConceptStatus.ReadyForQuote;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Archive()
    {
        ConceptStatus = DraftTripConceptStatus.Archived;
        IsPrimary = false;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    private static string? Normalize(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string? NormalizeCurrency(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var normalized = value.Trim().ToUpperInvariant();
        if (normalized.Length != 3)
            throw new DomainException("Concept currency must be a 3-letter ISO code.");

        return normalized;
    }
}

public sealed class DraftTripConceptDay
{
    private DraftTripConceptDay() { }

    private DraftTripConceptDay(int dayNumber, string title, string? description, string? location, string? overnightLocation)
    {
        Id = Guid.NewGuid();
        DayNumber = dayNumber;
        Title = title.Trim();
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        Location = string.IsNullOrWhiteSpace(location) ? null : location.Trim();
        OvernightLocation = string.IsNullOrWhiteSpace(overnightLocation) ? null : overnightLocation.Trim();
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }
    public int DayNumber { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string? Location { get; private set; }
    public string? OvernightLocation { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    public static DraftTripConceptDay Create(int dayNumber, string title, string? description, string? location, string? overnightLocation)
        => new(dayNumber, title, description, location, overnightLocation);
}
