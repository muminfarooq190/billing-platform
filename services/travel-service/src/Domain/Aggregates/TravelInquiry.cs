using TravelService.Domain.Common;
using TravelService.Domain.Enums;
using TravelService.Domain.Exceptions;

namespace TravelService.Domain.Aggregates;

public sealed class TravelInquiry : AggregateRoot
{
    private TravelInquiry() { }

    private TravelInquiry(
        Guid tenantId,
        string source,
        string fullName,
        string? email,
        string? phone,
        string? whatsappNumber,
        string? departureCity,
        string destination,
        DateTimeOffset? travelDate,
        DateTimeOffset? returnDate,
        bool isDateFlexible,
        int? travellers,
        decimal? budgetAmount,
        string? budgetCurrency,
        string? customerMessage)
    {
        if (tenantId == Guid.Empty)
            throw new DomainException("TenantId is required.");
        if (string.IsNullOrWhiteSpace(source))
            throw new DomainException("Inquiry source is required.");
        if (string.IsNullOrWhiteSpace(fullName))
            throw new DomainException("Customer full name is required.");
        if (string.IsNullOrWhiteSpace(destination))
            throw new DomainException("Destination is required.");
        if (travelDate.HasValue && returnDate.HasValue && returnDate.Value < travelDate.Value)
            throw new DomainException("Return date must be on or after travel date.");
        if (travellers.HasValue && travellers.Value <= 0)
            throw new DomainException("Travellers must be greater than zero when provided.");
        if (budgetAmount.HasValue && budgetAmount.Value < 0)
            throw new DomainException("Budget amount cannot be negative.");
        if (budgetAmount.HasValue && string.IsNullOrWhiteSpace(budgetCurrency))
            throw new DomainException("Budget currency is required when budget amount is provided.");

        Id = Guid.NewGuid();
        TenantId = tenantId;
        Source = source.Trim();
        Status = TravelInquiryStatus.New;
        FullName = fullName.Trim();
        Email = NormalizeOptional(email);
        Phone = NormalizeOptional(phone);
        WhatsappNumber = NormalizeOptional(whatsappNumber);
        DepartureCity = NormalizeOptional(departureCity);
        Destination = destination.Trim();
        TravelDate = travelDate;
        ReturnDate = returnDate;
        IsDateFlexible = isDateFlexible;
        Travellers = travellers;
        BudgetAmount = budgetAmount;
        BudgetCurrency = NormalizeCurrencyOptional(budgetCurrency);
        CustomerMessage = NormalizeOptional(customerMessage);
        CreatedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public string Source { get; private set; } = string.Empty;
    public TravelInquiryStatus Status { get; private set; }
    public string FullName { get; private set; } = string.Empty;
    public string? Email { get; private set; }
    public string? Phone { get; private set; }
    public string? WhatsappNumber { get; private set; }
    public string? DepartureCity { get; private set; }
    public string Destination { get; private set; } = string.Empty;
    public DateTimeOffset? TravelDate { get; private set; }
    public DateTimeOffset? ReturnDate { get; private set; }
    public bool IsDateFlexible { get; private set; }
    public int? Travellers { get; private set; }
    public decimal? BudgetAmount { get; private set; }
    public string? BudgetCurrency { get; private set; }
    public string? CustomerMessage { get; private set; }
    public Guid? AssignedToUserId { get; private set; }
    public DateTimeOffset? QualifiedAt { get; private set; }
    public DateTimeOffset? ContactedAt { get; private set; }
    public DateTimeOffset? DisqualifiedAt { get; private set; }
    public DateTimeOffset? ConvertedAt { get; private set; }
    public Guid? ConvertedContactId { get; private set; }
    public Guid? ConvertedQuotationId { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public DateTimeOffset? DeletedAt { get; private set; }

    public static TravelInquiry Create(
        Guid tenantId,
        string source,
        string fullName,
        string? email,
        string? phone,
        string? whatsappNumber,
        string? departureCity,
        string destination,
        DateTimeOffset? travelDate,
        DateTimeOffset? returnDate,
        bool isDateFlexible,
        int? travellers,
        decimal? budgetAmount,
        string? budgetCurrency,
        string? customerMessage)
        => new(tenantId, source, fullName, email, phone, whatsappNumber, departureCity, destination, travelDate, returnDate, isDateFlexible, travellers, budgetAmount, budgetCurrency, customerMessage);

    public void Assign(Guid? assignedToUserId)
    {
        EnsureNotClosed();
        AssignedToUserId = assignedToUserId;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Qualify()
    {
        EnsureNotClosed();
        if (Status == TravelInquiryStatus.Qualified)
            return;

        Status = TravelInquiryStatus.Qualified;
        QualifiedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void MarkContacted()
    {
        EnsureNotClosed();
        if (Status == TravelInquiryStatus.Contacted)
            return;

        if (Status == TravelInquiryStatus.New)
            QualifiedAt ??= DateTimeOffset.UtcNow;

        Status = TravelInquiryStatus.Contacted;
        ContactedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void MarkLost()
    {
        EnsureNotClosed();
        Status = TravelInquiryStatus.Lost;
        DisqualifiedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void MarkSpam()
    {
        EnsureNotClosed();
        Status = TravelInquiryStatus.Spam;
        DisqualifiedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Archive()
    {
        if (Status == TravelInquiryStatus.Archived)
            return;

        Status = TravelInquiryStatus.Archived;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void MarkQuoted(Guid contactId, Guid quotationId)
    {
        if (contactId == Guid.Empty)
            throw new DomainException("Converted contact id is required.");
        if (quotationId == Guid.Empty)
            throw new DomainException("Converted quotation id is required.");
        if (ConvertedAt.HasValue)
            throw new DomainException("Inquiry has already been converted.");

        Status = TravelInquiryStatus.Quoted;
        ConvertedAt = DateTimeOffset.UtcNow;
        ConvertedContactId = contactId;
        ConvertedQuotationId = quotationId;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    private void EnsureNotClosed()
    {
        if (Status is TravelInquiryStatus.Lost or TravelInquiryStatus.Spam or TravelInquiryStatus.Archived)
            throw new DomainException("Inquiry is closed and cannot be modified.");
        if (ConvertedAt.HasValue)
            throw new DomainException("Converted inquiry cannot be modified through this action.");
    }

    private static string? NormalizeOptional(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string? NormalizeCurrencyOptional(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var normalized = value.Trim().ToUpperInvariant();
        if (normalized.Length != 3)
            throw new DomainException("Budget currency must be a 3-letter ISO code.");

        return normalized;
    }
}
