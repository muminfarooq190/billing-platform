using MediatR;

namespace TravelService.Application.Queries.ListQuotationAttachments;

public sealed record ListQuotationAttachmentsQuery(Guid TenantId, Guid QuotationId, bool CustomerVisibleOnly = false) : IRequest<IReadOnlyList<QuotationAttachmentReadModel>>;

public sealed record QuotationAttachmentReadModel(
    Guid Id,
    Guid QuotationId,
    Guid? QuotationRevisionId,
    Guid TenantId,
    string OriginalFileName,
    string ContentType,
    long SizeBytes,
    string AttachmentType,
    string? Caption,
    bool IsCustomerVisible,
    int SortOrder,
    string ReadUrl,
    DateTimeOffset CreatedAt);
