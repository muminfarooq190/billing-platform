using TravelService.Domain.Exceptions;

namespace TravelService.Domain.Aggregates;

public sealed class BookingItem
{
    private static readonly HashSet<string> AllowedTypes = ["Flight", "Hotel", "Transfer", "Tour", "Train", "Visa", "Insurance", "Cruise", "Other"];
    private static readonly HashSet<string> AllowedStatuses = ["Pending", "Requested", "PendingSupplier", "Confirmed", "Ticketed", "Issued", "Cancelled", "Failed", "RefundPending"];

    private BookingItem() { }

    private BookingItem(Guid bookingId, Guid tenantId, string type, string status, string supplierName, string? supplierReference, string title, string? description, string? location, DateTimeOffset? startAt, DateTimeOffset? endAt, decimal? sellAmount, decimal? costAmount, string? currency, string? voucherNumber, string? confirmationNumber, Guid? assignedToUserId, string? notes, int sortOrder)
    {
        if (bookingId == Guid.Empty)
            throw new DomainException("BookingId is required.");
        if (tenantId == Guid.Empty)
            throw new DomainException("TenantId is required.");
        if (string.IsNullOrWhiteSpace(supplierName))
            throw new DomainException("Supplier name is required.");
        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException("Title is required.");
        if (sortOrder < 0)
            throw new DomainException("Sort order cannot be negative.");
        if (startAt.HasValue && endAt.HasValue && endAt.Value < startAt.Value)
            throw new DomainException("End time must be on or after start time.");

        Id = Guid.NewGuid();
        BookingId = bookingId;
        TenantId = tenantId;
        Type = NormalizeRequired(type, AllowedTypes, "Type");
        Status = NormalizeRequired(status, AllowedStatuses, "Status");
        SupplierName = supplierName.Trim();
        SupplierReference = NormalizeOptional(supplierReference);
        Title = title.Trim();
        Description = NormalizeOptional(description);
        Location = NormalizeOptional(location);
        StartAt = startAt;
        EndAt = endAt;
        SellAmount = sellAmount;
        CostAmount = costAmount;
        Currency = NormalizeCurrency(currency, sellAmount, costAmount);
        VoucherNumber = NormalizeOptional(voucherNumber);
        ConfirmationNumber = NormalizeOptional(confirmationNumber);
        AssignedToUserId = assignedToUserId;
        Notes = NormalizeOptional(notes);
        SortOrder = sortOrder;
        CreatedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }
    public Guid BookingId { get; private set; }
    public Guid TenantId { get; private set; }
    public string Type { get; private set; } = string.Empty;
    public string Status { get; private set; } = string.Empty;
    public string SupplierName { get; private set; } = string.Empty;
    public string? SupplierReference { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string? Location { get; private set; }
    public DateTimeOffset? StartAt { get; private set; }
    public DateTimeOffset? EndAt { get; private set; }
    public decimal? SellAmount { get; private set; }
    public decimal? CostAmount { get; private set; }
    public string? Currency { get; private set; }
    public string? VoucherNumber { get; private set; }
    public string? ConfirmationNumber { get; private set; }
    public DateTimeOffset? ConfirmationDeadline { get; private set; }
    public DateTimeOffset? ConfirmedAt { get; private set; }
    public DateTimeOffset? IssuedAt { get; private set; }
    public Guid? AssignedToUserId { get; private set; }
    public string? Notes { get; private set; }
    public int SortOrder { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public DateTimeOffset? DeletedAt { get; private set; }

    public static BookingItem Create(Guid bookingId, Guid tenantId, string type, string supplierName, string title, string? description, string? location, DateTimeOffset? startAt, DateTimeOffset? endAt, decimal? sellAmount, decimal? costAmount, string? currency, string? notes, int sortOrder)
        => new(bookingId, tenantId, type, "Pending", supplierName, null, title, description, location, startAt, endAt, sellAmount, costAmount, currency, null, null, null, notes, sortOrder);

    public void Update(string type, string supplierName, string? supplierReference, string title, string? description, string? location, DateTimeOffset? startAt, DateTimeOffset? endAt, decimal? sellAmount, decimal? costAmount, string? currency, string? voucherNumber, string? confirmationNumber, Guid? assignedToUserId, string? notes, int sortOrder)
    {
        if (string.IsNullOrWhiteSpace(supplierName))
            throw new DomainException("Supplier name is required.");
        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException("Title is required.");
        if (sortOrder < 0)
            throw new DomainException("Sort order cannot be negative.");
        if (startAt.HasValue && endAt.HasValue && endAt.Value < startAt.Value)
            throw new DomainException("End time must be on or after start time.");

        Type = NormalizeRequired(type, AllowedTypes, "Type");
        SupplierName = supplierName.Trim();
        SupplierReference = NormalizeOptional(supplierReference);
        Title = title.Trim();
        Description = NormalizeOptional(description);
        Location = NormalizeOptional(location);
        StartAt = startAt;
        EndAt = endAt;
        SellAmount = sellAmount;
        CostAmount = costAmount;
        Currency = NormalizeCurrency(currency, sellAmount, costAmount);
        VoucherNumber = NormalizeOptional(voucherNumber);
        ConfirmationNumber = NormalizeOptional(confirmationNumber);
        AssignedToUserId = assignedToUserId;
        Notes = NormalizeOptional(notes);
        SortOrder = sortOrder;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void UpdateStatus(string status)
    {
        Status = NormalizeRequired(status, AllowedStatuses, "Status");
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void RequestConfirmation(DateTimeOffset? confirmationDeadline, string? notes)
    {
        Status = "PendingSupplier";
        ConfirmationDeadline = confirmationDeadline;
        Notes = NormalizeOptional(notes) ?? Notes;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Confirm(string confirmationNumber, DateTimeOffset? confirmedAt, string? notes)
    {
        if (string.IsNullOrWhiteSpace(confirmationNumber))
            throw new DomainException("Confirmation number is required.");

        Status = "Confirmed";
        ConfirmationNumber = confirmationNumber.Trim();
        ConfirmedAt = confirmedAt ?? DateTimeOffset.UtcNow;
        Notes = NormalizeOptional(notes) ?? Notes;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Issue(string? voucherNumber, DateTimeOffset? issuedAt, string? notes)
    {
        Status = "Issued";
        VoucherNumber = NormalizeOptional(voucherNumber) ?? VoucherNumber;
        IssuedAt = issuedAt ?? DateTimeOffset.UtcNow;
        Notes = NormalizeOptional(notes) ?? Notes;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Delete()
    {
        if (DeletedAt is not null)
            throw new DomainException("Booking item is already deleted.");

        DeletedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    private static string NormalizeRequired(string value, HashSet<string> allowedValues, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException($"{fieldName} is required.");

        var normalized = value.Trim();
        if (!allowedValues.Contains(normalized))
            throw new DomainException($"{fieldName} '{normalized}' is not supported.");

        return normalized;
    }

    private static string? NormalizeCurrency(string? currency, decimal? sellAmount, decimal? costAmount)
    {
        if (sellAmount is null && costAmount is null)
            return string.IsNullOrWhiteSpace(currency) ? null : currency.Trim().ToUpperInvariant();

        if (string.IsNullOrWhiteSpace(currency))
            throw new DomainException("Currency is required when amounts are provided.");

        var normalized = currency.Trim().ToUpperInvariant();
        if (normalized.Length != 3)
            throw new DomainException("Currency must be a 3-letter ISO code.");

        return normalized;
    }

    private static string? NormalizeOptional(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
