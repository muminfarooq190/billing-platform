using MediatR;

namespace TravelService.Application.Commands.UploadQuotationAttachment;

public sealed record UploadQuotationAttachmentCommand(
    Guid TenantId,
    Guid QuotationId,
    Guid? QuotationRevisionId,
    string OriginalFileName,
    string ContentType,
    long SizeBytes,
    string AttachmentType,
    string? Caption,
    bool IsCustomerVisible,
    int SortOrder,
    byte[] Content) : IRequest<UploadQuotationAttachmentResult>;

public sealed record UploadQuotationAttachmentResult(Guid AttachmentId, string StorageKey);
