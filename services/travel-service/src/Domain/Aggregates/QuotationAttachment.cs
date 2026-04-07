using TravelService.Domain.Exceptions;

namespace TravelService.Domain.Aggregates;

public sealed class QuotationAttachment
{
    private static readonly HashSet<string> AllowedAttachmentTypes =
    [
        "Image",
        "Pdf",
        "Brochure",
        "Terms",
        "Document",
        "Other"
    ];

    private QuotationAttachment() { }

    private QuotationAttachment(
        Guid quotationId,
        Guid? quotationRevisionId,
        Guid tenantId,
        string storageKey,
        string originalFileName,
        string contentType,
        long sizeBytes,
        string attachmentType,
        string? caption,
        bool isCustomerVisible,
        int sortOrder,
        Guid? uploadedByUserId)
    {
        if (quotationId == Guid.Empty)
            throw new DomainException("QuotationId is required.");
        if (tenantId == Guid.Empty)
            throw new DomainException("TenantId is required.");
        if (string.IsNullOrWhiteSpace(storageKey))
            throw new DomainException("Storage key is required.");
        if (string.IsNullOrWhiteSpace(originalFileName))
            throw new DomainException("Original file name is required.");
        if (string.IsNullOrWhiteSpace(contentType))
            throw new DomainException("Content type is required.");
        if (sizeBytes <= 0)
            throw new DomainException("Attachment size must be greater than zero.");
        if (sortOrder < 0)
            throw new DomainException("Sort order cannot be negative.");

        Id = Guid.NewGuid();
        QuotationId = quotationId;
        QuotationRevisionId = quotationRevisionId;
        TenantId = tenantId;
        StorageKey = storageKey.Trim();
        OriginalFileName = originalFileName.Trim();
        ContentType = contentType.Trim().ToLowerInvariant();
        SizeBytes = sizeBytes;
        AttachmentType = NormalizeAttachmentType(attachmentType);
        Caption = NormalizeCaption(caption);
        IsCustomerVisible = isCustomerVisible;
        SortOrder = sortOrder;
        UploadedByUserId = uploadedByUserId;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }
    public Guid QuotationId { get; private set; }
    public Guid? QuotationRevisionId { get; private set; }
    public Guid TenantId { get; private set; }
    public string StorageKey { get; private set; } = string.Empty;
    public string OriginalFileName { get; private set; } = string.Empty;
    public string ContentType { get; private set; } = string.Empty;
    public long SizeBytes { get; private set; }
    public string AttachmentType { get; private set; } = string.Empty;
    public string? Caption { get; private set; }
    public bool IsCustomerVisible { get; private set; }
    public int SortOrder { get; private set; }
    public Guid? UploadedByUserId { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? DeletedAt { get; private set; }

    public static QuotationAttachment Create(
        Guid quotationId,
        Guid? quotationRevisionId,
        Guid tenantId,
        string storageKey,
        string originalFileName,
        string contentType,
        long sizeBytes,
        string attachmentType,
        string? caption,
        bool isCustomerVisible,
        int sortOrder,
        Guid? uploadedByUserId = null)
        => new(quotationId, quotationRevisionId, tenantId, storageKey, originalFileName, contentType, sizeBytes, attachmentType, caption, isCustomerVisible, sortOrder, uploadedByUserId);

    public void Delete(DateTimeOffset? deletedAt = null)
    {
        if (DeletedAt is not null)
            throw new DomainException("Quotation attachment has already been deleted.");

        DeletedAt = deletedAt ?? DateTimeOffset.UtcNow;
    }

    private static string NormalizeAttachmentType(string attachmentType)
    {
        if (string.IsNullOrWhiteSpace(attachmentType))
            throw new DomainException("Attachment type is required.");

        var normalized = attachmentType.Trim();
        if (!AllowedAttachmentTypes.Contains(normalized))
            throw new DomainException($"Attachment type '{normalized}' is not supported.");

        return normalized;
    }

    private static string? NormalizeCaption(string? caption)
        => string.IsNullOrWhiteSpace(caption) ? null : caption.Trim();
}
