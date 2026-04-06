using TravelService.Domain.Exceptions;

namespace TravelService.Domain.Aggregates;

public sealed class QuotationRevision
{
    private readonly List<QuotationRevisionLineItem> _lineItems = [];

    private QuotationRevision() { }

    private QuotationRevision(
        Guid quotationId,
        Guid tenantId,
        int revisionNumber,
        string status,
        Guid customerContactId,
        string customerName,
        string title,
        string destination,
        DateTimeOffset travelDate,
        DateTimeOffset returnDate,
        int travellers,
        string currency,
        string notes,
        string visibleNotes,
        string internalNotes,
        DateTimeOffset validUntil,
        Guid? createdByUserId,
        IReadOnlyCollection<QuotationRevisionLineItem> lineItems)
    {
        if (quotationId == Guid.Empty)
            throw new DomainException("QuotationId is required.");
        if (tenantId == Guid.Empty)
            throw new DomainException("TenantId is required.");
        if (revisionNumber <= 0)
            throw new DomainException("Revision number must be greater than zero.");
        if (customerContactId == Guid.Empty)
            throw new DomainException("CustomerContactId is required.");
        if (string.IsNullOrWhiteSpace(status))
            throw new DomainException("Revision status is required.");
        if (string.IsNullOrWhiteSpace(customerName))
            throw new DomainException("Customer name is required.");
        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException("Quotation title is required.");
        if (string.IsNullOrWhiteSpace(destination))
            throw new DomainException("Destination is required.");
        if (travellers <= 0)
            throw new DomainException("Travellers must be greater than zero.");
        if (returnDate < travelDate)
            throw new DomainException("Return date must be on or after the travel date.");
        if (validUntil > travelDate)
            throw new DomainException("Quotation validity cannot extend past the travel date.");
        if (lineItems.Count == 0)
            throw new DomainException("Quotation revision must include at least one line item.");

        Id = Guid.NewGuid();
        QuotationId = quotationId;
        TenantId = tenantId;
        RevisionNumber = revisionNumber;
        Status = status.Trim();
        CustomerContactId = customerContactId;
        CustomerName = customerName.Trim();
        Title = title.Trim();
        Destination = destination.Trim();
        TravelDate = travelDate;
        ReturnDate = returnDate;
        Travellers = travellers;
        Currency = NormalizeCurrency(currency);
        Notes = NormalizeNotes(notes);
        VisibleNotes = NormalizeNotes(visibleNotes);
        InternalNotes = NormalizeNotes(internalNotes);
        ValidUntil = validUntil;
        _lineItems.AddRange(lineItems.OrderBy(x => x.SortOrder));
        SubtotalAmount = _lineItems.Sum(x => x.LineTotal);
        TaxAmount = 0m;
        TotalAmount = SubtotalAmount + TaxAmount;
        CreatedByUserId = createdByUserId;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }
    public Guid QuotationId { get; private set; }
    public Guid TenantId { get; private set; }
    public int RevisionNumber { get; private set; }
    public string Status { get; private set; } = string.Empty;
    public Guid CustomerContactId { get; private set; }
    public string CustomerName { get; private set; } = string.Empty;
    public string Title { get; private set; } = string.Empty;
    public string Destination { get; private set; } = string.Empty;
    public DateTimeOffset TravelDate { get; private set; }
    public DateTimeOffset ReturnDate { get; private set; }
    public int Travellers { get; private set; }
    public string Currency { get; private set; } = "USD";
    public string Notes { get; private set; } = string.Empty;
    public string VisibleNotes { get; private set; } = string.Empty;
    public string InternalNotes { get; private set; } = string.Empty;
    public DateTimeOffset ValidUntil { get; private set; }
    public decimal SubtotalAmount { get; private set; }
    public decimal TaxAmount { get; private set; }
    public decimal TotalAmount { get; private set; }
    public Guid? CreatedByUserId { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public IReadOnlyList<QuotationRevisionLineItem> LineItems => _lineItems.AsReadOnly();

    public static QuotationRevision Create(
        Guid quotationId,
        Guid tenantId,
        int revisionNumber,
        string status,
        Guid customerContactId,
        string customerName,
        string title,
        string destination,
        DateTimeOffset travelDate,
        DateTimeOffset returnDate,
        int travellers,
        string currency,
        string notes,
        string visibleNotes,
        string internalNotes,
        DateTimeOffset validUntil,
        Guid? createdByUserId,
        IReadOnlyCollection<QuotationRevisionLineItem> lineItems)
        => new(quotationId, tenantId, revisionNumber, status, customerContactId, customerName, title, destination, travelDate, returnDate, travellers, currency, notes, visibleNotes, internalNotes, validUntil, createdByUserId, lineItems);

    private static string NormalizeCurrency(string currency)
    {
        if (string.IsNullOrWhiteSpace(currency))
            throw new DomainException("Currency is required.");

        var normalized = currency.Trim().ToUpperInvariant();
        if (normalized.Length != 3)
            throw new DomainException("Currency must be a 3-letter ISO code.");

        return normalized;
    }

    private static string NormalizeNotes(string notes) => notes.Trim();
}
