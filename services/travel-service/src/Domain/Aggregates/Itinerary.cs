using TravelService.Domain.Common;
using TravelService.Domain.Enums;
using TravelService.Domain.Events;
using TravelService.Domain.Exceptions;
using TravelService.Domain.ValueObjects;

namespace TravelService.Domain.Aggregates;

public sealed class Itinerary : AggregateRoot
{
    private readonly List<ItineraryItem> _items = [];
    private Itinerary() { }

    private Itinerary(Guid tenantId, Guid customerContactId, string customerName, string title, string destination, DateTimeOffset startDate, DateTimeOffset endDate, int travellers, string currency, Guid? quotationId, Guid? bookingId)
    {
        ValidateIdentity(tenantId, customerContactId);
        ValidateDetails(customerName, title, destination, startDate, endDate, travellers, currency);

        Id = Guid.NewGuid();
        TenantId = tenantId;
        CustomerContactId = customerContactId;
        CustomerName = customerName.Trim();
        Title = title.Trim();
        Destination = destination.Trim();
        StartDate = startDate;
        EndDate = endDate;
        Travellers = travellers;
        Currency = NormalizeCurrency(currency);
        QuotationId = quotationId;
        BookingId = bookingId;
        Status = ItineraryStatus.Draft;
        CreatedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
        AddDomainEvent(new ItineraryCreatedEvent(Id, TenantId, QuotationId));
    }

    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public Guid CustomerContactId { get; private set; }
    public string CustomerName { get; private set; } = string.Empty;
    public string Title { get; private set; } = string.Empty;
    public string Destination { get; private set; } = string.Empty;
    public DateTimeOffset StartDate { get; private set; }
    public DateTimeOffset EndDate { get; private set; }
    public int Travellers { get; private set; }
    public string Currency { get; private set; } = "USD";
    public Guid? QuotationId { get; private set; }
    public Guid? BookingId { get; private set; }
    public ItineraryStatus Status { get; private set; }
    public IReadOnlyList<ItineraryItem> Items => _items.AsReadOnly();
    public decimal TotalCost => _items.Sum(x => x.Cost);
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public DateTimeOffset? DeletedAt { get; private set; }

    public static Itinerary Create(Guid tenantId, Guid customerContactId, string customerName, string title, string destination, DateTimeOffset startDate, DateTimeOffset endDate, int travellers, string currency, Guid? quotationId, Guid? bookingId = null)
        => new(tenantId, customerContactId, customerName, title, destination, startDate, endDate, travellers, currency, quotationId, bookingId);

    public void AddItem(int dayNumber, ItineraryItemType itemType, string title, string description, string location, DateTimeOffset? startTime, DateTimeOffset? endTime, decimal cost, string currency)
    {
        if (Status != ItineraryStatus.Draft)
            throw new DomainException("Can only modify itinerary items while the itinerary is in draft.");

        var normalizedCurrency = NormalizeCurrency(currency);
        if (_items.Count > 0 && !string.Equals(normalizedCurrency, Currency, StringComparison.OrdinalIgnoreCase))
            throw new DomainException("Itinerary item currency must match itinerary currency.");

        _items.Add(new ItineraryItem(dayNumber, itemType, title, description, location, startTime, endTime, cost, normalizedCurrency));
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Confirm()
    {
        if (Status != ItineraryStatus.Draft)
            throw new DomainException("Only draft itineraries can be confirmed.");
        if (_items.Count == 0)
            throw new DomainException("Cannot confirm an itinerary with no scheduled items.");
        Status = ItineraryStatus.Confirmed;
        UpdatedAt = DateTimeOffset.UtcNow;
        AddDomainEvent(new ItineraryConfirmedEvent(Id, TenantId));
    }

    public void Start()
    {
        if (Status != ItineraryStatus.Confirmed)
            throw new DomainException("Only confirmed itineraries can be started.");
        Status = ItineraryStatus.InProgress;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Complete()
    {
        if (Status != ItineraryStatus.InProgress)
            throw new DomainException("Only in-progress itineraries can be completed.");
        Status = ItineraryStatus.Completed;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Cancel()
    {
        if (Status == ItineraryStatus.Completed)
            throw new DomainException("Cannot cancel a completed itinerary.");
        if (Status == ItineraryStatus.Cancelled)
            return;
        Status = ItineraryStatus.Cancelled;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Update(string title, string destination, DateTimeOffset startDate, DateTimeOffset endDate, int travellers, string currency)
    {
        if (Status != ItineraryStatus.Draft)
            throw new DomainException("Can only update draft itineraries.");

        ValidateDetails(CustomerName, title, destination, startDate, endDate, travellers, currency);
        var normalizedCurrency = NormalizeCurrency(currency);
        if (_items.Count > 0 && !string.Equals(normalizedCurrency, Currency, StringComparison.OrdinalIgnoreCase))
            throw new DomainException("Cannot change itinerary currency after items have been added.");

        Title = title.Trim();
        Destination = destination.Trim();
        StartDate = startDate;
        EndDate = endDate;
        Travellers = travellers;
        Currency = normalizedCurrency;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    private static void ValidateIdentity(Guid tenantId, Guid customerContactId)
    {
        if (tenantId == Guid.Empty)
            throw new DomainException("TenantId is required.");
        if (customerContactId == Guid.Empty)
            throw new DomainException("CustomerContactId is required.");
    }

    private static void ValidateDetails(string customerName, string title, string destination, DateTimeOffset startDate, DateTimeOffset endDate, int travellers, string currency)
    {
        if (string.IsNullOrWhiteSpace(customerName))
            throw new DomainException("Customer name is required.");
        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException("Itinerary title is required.");
        if (string.IsNullOrWhiteSpace(destination))
            throw new DomainException("Destination is required.");
        if (travellers <= 0)
            throw new DomainException("Travellers must be greater than zero.");
        if (endDate < startDate)
            throw new DomainException("End date must be on or after the start date.");
        _ = NormalizeCurrency(currency);
    }

    private static string NormalizeCurrency(string currency)
    {
        if (string.IsNullOrWhiteSpace(currency))
            throw new DomainException("Currency is required.");

        var normalized = currency.Trim().ToUpperInvariant();
        if (normalized.Length != 3)
            throw new DomainException("Currency must be a 3-letter ISO code.");

        return normalized;
    }
}
