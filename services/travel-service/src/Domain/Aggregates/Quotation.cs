using TravelService.Domain.Common;
using TravelService.Domain.Enums;
using TravelService.Domain.Events;
using TravelService.Domain.Exceptions;
using TravelService.Domain.ValueObjects;

namespace TravelService.Domain.Aggregates;

public sealed class Quotation : AggregateRoot
{
    private readonly List<QuotationLineItem> _lineItems = [];
    private Quotation() { }

    private Quotation(Guid tenantId, Guid customerContactId, string customerName, string title, string destination, DateTimeOffset travelDate, DateTimeOffset returnDate, int travellers, string currency, string notes)
    {
        Id = Guid.NewGuid();
        TenantId = tenantId;
        CustomerContactId = customerContactId;
        CustomerName = customerName;
        Title = title;
        Destination = destination;
        TravelDate = travelDate;
        ReturnDate = returnDate;
        Travellers = travellers;
        Currency = currency;
        Notes = notes;
        Status = QuotationStatus.Draft;
        ValidUntil = DateTimeOffset.UtcNow.AddDays(30);
        CreatedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
        AddDomainEvent(new QuotationCreatedEvent(Id, TenantId, CustomerContactId));
    }

    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public Guid CustomerContactId { get; private set; }
    public string CustomerName { get; private set; } = string.Empty;
    public string Title { get; private set; } = string.Empty;
    public string Destination { get; private set; } = string.Empty;
    public DateTimeOffset TravelDate { get; private set; }
    public DateTimeOffset ReturnDate { get; private set; }
    public int Travellers { get; private set; }
    public string Currency { get; private set; } = "USD";
    public string Notes { get; private set; } = string.Empty;
    public QuotationStatus Status { get; private set; }
    public DateTimeOffset ValidUntil { get; private set; }
    public IReadOnlyList<QuotationLineItem> LineItems => _lineItems.AsReadOnly();
    public decimal TotalAmount => _lineItems.Sum(x => x.Total);
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public DateTimeOffset? DeletedAt { get; private set; }

    public static Quotation Create(Guid tenantId, Guid customerContactId, string customerName, string title, string destination, DateTimeOffset travelDate, DateTimeOffset returnDate, int travellers, string currency, string notes)
        => new(tenantId, customerContactId, customerName, title, destination, travelDate, returnDate, travellers, currency, notes);

    public void AddLineItem(string description, decimal unitPrice, int quantity, string currency)
    {
        if (Status != QuotationStatus.Draft)
            throw new DomainException("Can only modify line items on draft quotations.");
        _lineItems.Add(new QuotationLineItem(description, unitPrice, quantity, currency));
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Send()
    {
        if (Status != QuotationStatus.Draft)
            throw new DomainException("Only draft quotations can be sent.");
        if (_lineItems.Count == 0)
            throw new DomainException("Cannot send a quotation with no line items.");
        Status = QuotationStatus.Sent;
        UpdatedAt = DateTimeOffset.UtcNow;
        AddDomainEvent(new QuotationSentEvent(Id, TenantId));
    }

    public void Accept()
    {
        if (Status != QuotationStatus.Sent)
            throw new DomainException("Only sent quotations can be accepted.");
        Status = QuotationStatus.Accepted;
        UpdatedAt = DateTimeOffset.UtcNow;
        AddDomainEvent(new QuotationAcceptedEvent(Id, TenantId));
    }

    public void Reject()
    {
        if (Status != QuotationStatus.Sent)
            throw new DomainException("Only sent quotations can be rejected.");
        Status = QuotationStatus.Rejected;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void MarkConverted()
    {
        if (Status != QuotationStatus.Accepted)
            throw new DomainException("Only accepted quotations can be converted to itineraries.");
        Status = QuotationStatus.ConvertedToItinerary;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Update(string title, string destination, DateTimeOffset travelDate, DateTimeOffset returnDate, int travellers, string currency, string notes, DateTimeOffset validUntil)
    {
        if (Status != QuotationStatus.Draft)
            throw new DomainException("Can only update draft quotations.");
        Title = title;
        Destination = destination;
        TravelDate = travelDate;
        ReturnDate = returnDate;
        Travellers = travellers;
        Currency = currency;
        Notes = notes;
        ValidUntil = validUntil;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
