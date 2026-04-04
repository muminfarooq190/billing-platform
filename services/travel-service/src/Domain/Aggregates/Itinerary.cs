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

    private Itinerary(Guid tenantId, Guid customerContactId, string customerName, string title, string destination, DateTimeOffset startDate, DateTimeOffset endDate, int travellers, string currency, Guid? quotationId)
    {
        Id = Guid.NewGuid();
        TenantId = tenantId;
        CustomerContactId = customerContactId;
        CustomerName = customerName;
        Title = title;
        Destination = destination;
        StartDate = startDate;
        EndDate = endDate;
        Travellers = travellers;
        Currency = currency;
        QuotationId = quotationId;
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
    public ItineraryStatus Status { get; private set; }
    public IReadOnlyList<ItineraryItem> Items => _items.AsReadOnly();
    public decimal TotalCost => _items.Sum(x => x.Cost);
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public DateTimeOffset? DeletedAt { get; private set; }

    public static Itinerary Create(Guid tenantId, Guid customerContactId, string customerName, string title, string destination, DateTimeOffset startDate, DateTimeOffset endDate, int travellers, string currency, Guid? quotationId)
        => new(tenantId, customerContactId, customerName, title, destination, startDate, endDate, travellers, currency, quotationId);

    public void AddItem(int dayNumber, ItineraryItemType itemType, string title, string description, string location, DateTimeOffset? startTime, DateTimeOffset? endTime, decimal cost, string currency)
    {
        if (Status == ItineraryStatus.Completed || Status == ItineraryStatus.Cancelled)
            throw new DomainException("Cannot modify a completed or cancelled itinerary.");
        _items.Add(new ItineraryItem(dayNumber, itemType, title, description, location, startTime, endTime, cost, currency));
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Confirm()
    {
        if (Status != ItineraryStatus.Draft)
            throw new DomainException("Only draft itineraries can be confirmed.");
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
        Status = ItineraryStatus.Cancelled;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Update(string title, string destination, DateTimeOffset startDate, DateTimeOffset endDate, int travellers, string currency)
    {
        if (Status != ItineraryStatus.Draft)
            throw new DomainException("Can only update draft itineraries.");
        Title = title;
        Destination = destination;
        StartDate = startDate;
        EndDate = endDate;
        Travellers = travellers;
        Currency = currency;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
