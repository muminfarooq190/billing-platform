using System.Security.Cryptography;
using MediatR;
using TravelService.Application.Abstractions;
using TravelService.Domain.Aggregates;
using TravelService.Domain.Exceptions;
using TravelService.Domain.Repositories;

namespace TravelService.Application.Commands.UploadQuotationAttachment;

public sealed class UploadQuotationAttachmentCommandHandler(
    IQuotationRepository quotationRepository,
    IQuotationRevisionRepository quotationRevisionRepository,
    IQuotationAttachmentRepository quotationAttachmentRepository,
    IFileStorage fileStorage,
    IFeatureGate featureGate,
    IActivityWriter activityWriter,
    IActorContext actorContext,
    IUnitOfWork unitOfWork, Api.ITenantContext tenantContext) : IRequestHandler<UploadQuotationAttachmentCommand, UploadQuotationAttachmentResult>
{
    private const long MaxFileSizeBytes = 10 * 1024 * 1024;
    private static readonly HashSet<string> AllowedContentTypes =
    [
        "image/jpeg",
        "image/png",
        "image/webp",
        "application/pdf",
        "application/msword",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document"
    ];

    public async Task<UploadQuotationAttachmentResult> Handle(UploadQuotationAttachmentCommand request, CancellationToken cancellationToken)
    {
        await featureGate.EnsureEnabledAsync(FeatureKeys.TravelQuotationAttachmentsUpload, request.TenantId, tenantContext.UserId, cancellationToken);

        var quotation = await quotationRepository.GetByIdAsync(request.QuotationId, cancellationToken)
            ?? throw new DomainException($"Quotation {request.QuotationId} not found.");

        if (quotation.TenantId != request.TenantId)
            throw new DomainException("Quotation does not belong to the active tenant.");

        if (request.SizeBytes <= 0)
            throw new DomainException("Attachment file is required.");
        if (request.SizeBytes > MaxFileSizeBytes)
            throw new DomainException("Attachment exceeds the maximum allowed file size of 10 MB.");

        var normalizedContentType = NormalizeContentType(request.ContentType);
        if (!AllowedContentTypes.Contains(normalizedContentType))
            throw new DomainException($"Content type '{normalizedContentType}' is not allowed.");

        if (request.QuotationRevisionId.HasValue)
        {
            var revision = await quotationRevisionRepository.GetByIdAsync(request.QuotationId, request.QuotationRevisionId.Value, cancellationToken)
                ?? throw new DomainException("Quotation revision not found.");

            if (revision.TenantId != request.TenantId)
                throw new DomainException("Quotation revision does not belong to the active tenant.");
        }

        var storagePath = BuildStoragePath(request.TenantId, request.QuotationId, request.OriginalFileName);
        await using var stream = new MemoryStream(request.Content, writable: false);
        var storageKey = await fileStorage.UploadAsync(stream, storagePath, normalizedContentType, cancellationToken);

        var attachment = QuotationAttachment.Create(
            request.QuotationId,
            request.QuotationRevisionId,
            request.TenantId,
            storageKey,
            Path.GetFileName(request.OriginalFileName),
            normalizedContentType,
            request.SizeBytes,
            request.AttachmentType,
            request.Caption,
            request.IsCustomerVisible,
            request.SortOrder);

        await quotationAttachmentRepository.AddAsync(attachment, cancellationToken);
        await activityWriter.WriteAsync(
            ActivityEntry.Create(
                request.TenantId,
                "Quotation",
                quotation.Id,
                "DocumentUploaded",
                $"Quotation attachment uploaded: {attachment.OriginalFileName}",
                new { attachment.Id, attachment.AttachmentType, attachment.OriginalFileName, attachment.QuotationRevisionId, attachment.IsCustomerVisible },
                actorContext.UserId),
            cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new UploadQuotationAttachmentResult(attachment.Id, attachment.StorageKey);
    }

    private static string BuildStoragePath(Guid tenantId, Guid quotationId, string originalFileName)
    {
        var extension = Path.GetExtension(originalFileName)?.ToLowerInvariant() ?? string.Empty;
        if (!string.IsNullOrEmpty(extension) && extension.Any(ch => !char.IsLetterOrDigit(ch) && ch != '.'))
            throw new DomainException("Invalid file extension.");

        var randomName = Convert.ToHexString(RandomNumberGenerator.GetBytes(16)).ToLowerInvariant();
        return $"tenant/{tenantId:D}/quotations/{quotationId:D}/{randomName}{extension}";
    }

    private static string NormalizeContentType(string contentType)
    {
        if (string.IsNullOrWhiteSpace(contentType))
            throw new DomainException("Content type is required.");

        return contentType.Trim().ToLowerInvariant();
    }
}
