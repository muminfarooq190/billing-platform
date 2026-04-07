namespace TravelService.Application.Queries.QuotationRevisions;

public sealed record QuotationRevisionLineItemReadModel(
    Guid Id,
    string Description,
    int Quantity,
    decimal UnitPriceAmount,
    string Currency,
    int SortOrder,
    decimal LineTotal);

public sealed record QuotationRevisionSummaryReadModel(
    Guid Id,
    Guid QuotationId,
    Guid TenantId,
    int RevisionNumber,
    string Status,
    string Title,
    string Destination,
    DateTimeOffset TravelDate,
    DateTimeOffset ReturnDate,
    int Travellers,
    string Currency,
    DateTimeOffset ValidUntil,
    decimal SubtotalAmount,
    decimal TaxAmount,
    decimal TotalAmount,
    DateTimeOffset CreatedAt);

public sealed record QuotationRevisionAttachmentReadModel(
    Guid Id,
    string OriginalFileName,
    string ContentType,
    long SizeBytes,
    string AttachmentType,
    string? Caption,
    bool IsCustomerVisible,
    int SortOrder,
    string ReadUrl,
    DateTimeOffset CreatedAt);

public sealed record QuotationRevisionReadModel(
    Guid Id,
    Guid QuotationId,
    Guid TenantId,
    int RevisionNumber,
    string Status,
    Guid CustomerContactId,
    string CustomerName,
    string Title,
    string Destination,
    DateTimeOffset TravelDate,
    DateTimeOffset ReturnDate,
    int Travellers,
    string Currency,
    string Notes,
    string VisibleNotes,
    string InternalNotes,
    DateTimeOffset ValidUntil,
    decimal SubtotalAmount,
    decimal TaxAmount,
    decimal TotalAmount,
    Guid? CreatedByUserId,
    DateTimeOffset CreatedAt,
    IReadOnlyList<QuotationRevisionLineItemReadModel> LineItems,
    IReadOnlyList<QuotationRevisionAttachmentReadModel> Attachments);
