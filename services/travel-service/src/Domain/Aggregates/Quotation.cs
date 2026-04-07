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
        ValidateIdentity(tenantId, customerContactId);
        ValidateDetails(customerName, title, destination, travelDate, returnDate, travellers, currency);

        Id = Guid.NewGuid();
        TenantId = tenantId;
        CustomerContactId = customerContactId;
        CustomerName = customerName.Trim();
        Title = title.Trim();
        Destination = destination.Trim();
        TravelDate = travelDate;
        ReturnDate = returnDate;
        Travellers = travellers;
        Currency = NormalizeCurrency(currency);
        Notes = NormalizeNotes(notes);
        Status = QuotationStatus.Draft;
        ValidUntil = DetermineInitialValidUntil(travelDate);
        CurrentRevisionNumber = 0;
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
    public int CurrentRevisionNumber { get; private set; }
    public Guid? AcceptedRevisionId { get; private set; }
    public DateTimeOffset? LastSentAt { get; private set; }
    public DateTimeOffset? LastViewedAt { get; private set; }
    public DateTimeOffset? ExpiredAt { get; private set; }
    public DateTimeOffset? RejectedAt { get; private set; }
    public string? ShareToken { get; private set; }
    public DateTimeOffset? ShareTokenExpiresAt { get; private set; }
    public IReadOnlyList<QuotationLineItem> LineItems => _lineItems.AsReadOnly();
    public decimal TotalAmount => _lineItems.Sum(x => x.Total);
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public DateTimeOffset? DeletedAt { get; private set; }

    public static Quotation Create(Guid tenantId, Guid customerContactId, string customerName, string title, string destination, DateTimeOffset travelDate, DateTimeOffset returnDate, int travellers, string currency, string notes)
        => new(tenantId, customerContactId, customerName, title, destination, travelDate, returnDate, travellers, currency, notes);

    public void AddLineItem(string description, decimal unitPrice, int quantity, string currency)
    {
        EnsureDraftMutationAllowed();

        var normalizedCurrency = NormalizeCurrency(currency);
        EnsureSameCurrency(normalizedCurrency, "line item");
        _lineItems.Add(new QuotationLineItem(description, unitPrice, quantity, normalizedCurrency));
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void ReplaceDraftDetails(string title, string destination, DateTimeOffset travelDate, DateTimeOffset returnDate, int travellers, string currency, string notes, DateTimeOffset validUntil)
    {
        EnsureDraftMutationAllowed();

        ValidateDetails(CustomerName, title, destination, travelDate, returnDate, travellers, currency);
        EnsureValidUntil(validUntil, travelDate);
        var normalizedCurrency = NormalizeCurrency(currency);
        EnsureNoCurrencyMismatch(normalizedCurrency);

        Title = title.Trim();
        Destination = destination.Trim();
        TravelDate = travelDate;
        ReturnDate = returnDate;
        Travellers = travellers;
        Currency = normalizedCurrency;
        Notes = NormalizeNotes(notes);
        ValidUntil = validUntil;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void ReplaceLineItems(IEnumerable<(string Description, decimal UnitPrice, int Quantity, string Currency)> lineItems)
    {
        EnsureDraftMutationAllowed();

        var normalizedItems = lineItems
            .Select(x => new QuotationLineItem(x.Description, x.UnitPrice, x.Quantity, NormalizeCurrency(x.Currency)))
            .ToList();

        if (normalizedItems.Count == 0)
            throw new DomainException("Quotation revision must include at least one line item.");

        if (normalizedItems.Any(x => !string.Equals(x.Currency, Currency, StringComparison.OrdinalIgnoreCase)))
            throw new DomainException("Quotation line item currency must match quotation currency.");

        _lineItems.Clear();
        _lineItems.AddRange(normalizedItems);
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public QuotationRevision CreateRevision(string visibleNotes, string internalNotes, Guid? createdByUserId = null)
    {
        if (_lineItems.Count == 0)
            throw new DomainException("Cannot create a quotation revision with no line items.");

        var revision = QuotationRevision.Create(
            Id,
            TenantId,
            CurrentRevisionNumber + 1,
            Status.ToString(),
            CustomerContactId,
            CustomerName,
            Title,
            Destination,
            TravelDate,
            ReturnDate,
            Travellers,
            Currency,
            Notes,
            visibleNotes,
            internalNotes,
            ValidUntil,
            createdByUserId,
            _lineItems.Select((item, index) => QuotationRevisionLineItem.Create(item.Description, item.Quantity, item.UnitPrice, item.Currency, index + 1)).ToList());

        CurrentRevisionNumber = revision.RevisionNumber;
        UpdatedAt = DateTimeOffset.UtcNow;
        return revision;
    }

    public void Send()
    {
        if (Status != QuotationStatus.Draft)
            throw new DomainException("Only draft quotations can be sent.");
        if (_lineItems.Count == 0)
            throw new DomainException("Cannot send a quotation with no line items.");
        if (ValidUntil < DateTimeOffset.UtcNow)
            throw new DomainException("Cannot send an expired quotation.");

        Status = QuotationStatus.Sent;
        LastSentAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
        AddDomainEvent(new QuotationSentEvent(Id, TenantId));
    }

    public void Accept(Guid revisionId)
    {
        if (revisionId == Guid.Empty)
            throw new DomainException("Accepted revision id is required.");
        if (CurrentRevisionNumber <= 0)
            throw new DomainException("Cannot accept a quotation with no revisions.");
        if (Status != QuotationStatus.Sent)
            throw new DomainException("Only sent quotations can be accepted.");
        if (ValidUntil < DateTimeOffset.UtcNow)
        {
            Expire();
            throw new DomainException("Cannot accept an expired quotation.");
        }

        Status = QuotationStatus.Accepted;
        AcceptedRevisionId = revisionId;
        RejectedAt = null;
        ExpiredAt = null;
        UpdatedAt = DateTimeOffset.UtcNow;
        AddDomainEvent(new QuotationAcceptedEvent(Id, TenantId));
    }

    public void Reject()
    {
        if (Status != QuotationStatus.Sent)
            throw new DomainException("Only sent quotations can be rejected.");

        Status = QuotationStatus.Rejected;
        RejectedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void MarkViewed(DateTimeOffset? viewedAt = null)
    {
        LastViewedAt = viewedAt ?? DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void SetShareToken(string? shareToken, DateTimeOffset? shareTokenExpiresAt)
    {
        ShareToken = string.IsNullOrWhiteSpace(shareToken) ? null : shareToken.Trim();
        ShareTokenExpiresAt = shareTokenExpiresAt;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Expire(DateTimeOffset? expiredAt = null)
    {
        if (Status is QuotationStatus.Accepted or QuotationStatus.ConvertedToItinerary)
            throw new DomainException("Accepted quotations cannot be expired.");
        if (Status == QuotationStatus.Rejected)
            throw new DomainException("Rejected quotations cannot be expired.");
        if (Status == QuotationStatus.Expired)
            throw new DomainException("Quotation is already expired.");

        Status = QuotationStatus.Expired;
        ExpiredAt = expiredAt ?? DateTimeOffset.UtcNow;
        RejectedAt = null;
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
        => ReplaceDraftDetails(title, destination, travelDate, returnDate, travellers, currency, notes, validUntil);

    private void EnsureDraftMutationAllowed()
    {
        if (Status != QuotationStatus.Draft)
            throw new DomainException("Can only modify draft quotations.");
    }

    private static void ValidateIdentity(Guid tenantId, Guid customerContactId)
    {
        if (tenantId == Guid.Empty)
            throw new DomainException("TenantId is required.");
        if (customerContactId == Guid.Empty)
            throw new DomainException("CustomerContactId is required.");
    }

    private static void ValidateDetails(string customerName, string title, string destination, DateTimeOffset travelDate, DateTimeOffset returnDate, int travellers, string currency)
    {
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
        _ = NormalizeCurrency(currency);
    }

    private static DateTimeOffset DetermineInitialValidUntil(DateTimeOffset travelDate)
    {
        var proposed = DateTimeOffset.UtcNow.AddDays(14);
        return proposed > travelDate ? travelDate : proposed;
    }

    private static void EnsureValidUntil(DateTimeOffset validUntil, DateTimeOffset travelDate)
    {
        if (validUntil < DateTimeOffset.UtcNow)
            throw new DomainException("Quotation validity cannot already be in the past.");
        if (validUntil > travelDate)
            throw new DomainException("Quotation validity cannot extend past the travel date.");
    }

    private void EnsureSameCurrency(string currency, string context)
    {
        if (_lineItems.Count > 0 && !string.Equals(currency, Currency, StringComparison.OrdinalIgnoreCase))
            throw new DomainException($"Quotation {context} currency must match quotation currency.");
    }

    private void EnsureNoCurrencyMismatch(string currency)
    {
        if (_lineItems.Count > 0 && !string.Equals(currency, Currency, StringComparison.OrdinalIgnoreCase))
            throw new DomainException("Cannot change quotation currency after line items have been added.");
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

    private static string NormalizeNotes(string notes) => notes.Trim();
}
