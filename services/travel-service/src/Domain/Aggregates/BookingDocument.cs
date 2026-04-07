using TravelService.Domain.Exceptions;

namespace TravelService.Domain.Aggregates;

public sealed class BookingDocument
{
    private static readonly HashSet<string> AllowedDocumentTypes = ["Voucher", "Ticket", "Confirmation", "Invoice", "Receipt", "PassportCopy", "Visa", "Insurance", "Other"];

    private BookingDocument() { }

    private BookingDocument(Guid bookingId, Guid? travelerId, Guid tenantId, string storageKey, string originalFileName, string contentType, long sizeBytes, string documentType, bool isCustomerVisible, string? description, Guid? uploadedByUserId)
    {
        if (bookingId == Guid.Empty)
            throw new DomainException("BookingId is required.");
        if (tenantId == Guid.Empty)
            throw new DomainException("TenantId is required.");
        if (string.IsNullOrWhiteSpace(storageKey))
            throw new DomainException("Storage key is required.");
        if (string.IsNullOrWhiteSpace(originalFileName))
            throw new DomainException("Original file name is required.");
        if (string.IsNullOrWhiteSpace(contentType))
            throw new DomainException("Content type is required.");
        if (sizeBytes <= 0)
            throw new DomainException("Document size must be greater than zero.");

        Id = Guid.NewGuid();
        BookingId = bookingId;
        TravelerId = travelerId;
        TenantId = tenantId;
        StorageKey = storageKey.Trim();
        OriginalFileName = originalFileName.Trim();
        ContentType = contentType.Trim().ToLowerInvariant();
        SizeBytes = sizeBytes;
        DocumentType = NormalizeDocumentType(documentType);
        IsCustomerVisible = isCustomerVisible;
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        UploadedByUserId = uploadedByUserId;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }
    public Guid BookingId { get; private set; }
    public Guid? TravelerId { get; private set; }
    public Guid TenantId { get; private set; }
    public string StorageKey { get; private set; } = string.Empty;
    public string OriginalFileName { get; private set; } = string.Empty;
    public string ContentType { get; private set; } = string.Empty;
    public long SizeBytes { get; private set; }
    public string DocumentType { get; private set; } = string.Empty;
    public bool IsCustomerVisible { get; private set; }
    public string? Description { get; private set; }
    public Guid? UploadedByUserId { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? DeletedAt { get; private set; }

    public static BookingDocument Create(Guid bookingId, Guid? travelerId, Guid tenantId, string storageKey, string originalFileName, string contentType, long sizeBytes, string documentType, bool isCustomerVisible, string? description, Guid? uploadedByUserId = null)
        => new(bookingId, travelerId, tenantId, storageKey, originalFileName, contentType, sizeBytes, documentType, isCustomerVisible, description, uploadedByUserId);

    public void Delete()
    {
        if (DeletedAt is not null)
            throw new DomainException("Booking document is already deleted.");

        DeletedAt = DateTimeOffset.UtcNow;
    }

    private static string NormalizeDocumentType(string documentType)
    {
        if (string.IsNullOrWhiteSpace(documentType))
            throw new DomainException("Document type is required.");

        var normalized = documentType.Trim();
        if (!AllowedDocumentTypes.Contains(normalized))
            throw new DomainException($"Document type '{normalized}' is not supported.");

        return normalized;
    }
}
