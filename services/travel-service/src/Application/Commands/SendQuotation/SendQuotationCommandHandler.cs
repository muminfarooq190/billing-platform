using System.Security.Cryptography;
using MediatR;
using TravelService.Api.Documents;
using TravelService.Application.Abstractions;
using TravelService.Application.Queries.GetQuotationRevisionById;
using TravelService.Application.Queries.QuotationRevisions;
using TravelService.Domain.Aggregates;
using TravelService.Domain.Exceptions;
using TravelService.Domain.Repositories;

namespace TravelService.Application.Commands.SendQuotation;

public sealed class SendQuotationCommandHandler(
    IQuotationRepository quotationRepository,
    IQuotationRevisionRepository quotationRevisionRepository,
    IQuotationShareLinkRepository quotationShareLinkRepository,
    IQuotationStatusHistoryRepository quotationStatusHistoryRepository,
    IQuotationApprovalRequestRepository approvalRequestRepository,
    IQuotationAttachmentRepository quotationAttachmentRepository,
    IFileStorage fileStorage,
    IPdfDocumentRenderer pdfDocumentRenderer,
    ICommunicationWorkflowClient communicationWorkflowClient,
    IFeatureGate featureGate,
    IActivityWriter activityWriter,
    IActorContext actorContext,
    IUnitOfWork unitOfWork,
    Api.ITenantContext tenantContext) : IRequestHandler<SendQuotationCommand, SendQuotationResult>
{
    public async Task<SendQuotationResult> Handle(SendQuotationCommand request, CancellationToken cancellationToken)
    {
        await featureGate.EnsureEnabledAsync(FeatureKeys.TravelQuotationSend, request.TenantId, tenantContext.UserId, cancellationToken);

        var quotation = await quotationRepository.GetByIdAsync(request.QuotationId, cancellationToken)
            ?? throw new DomainException($"Quotation {request.QuotationId} not found.");

        if (quotation.TenantId != request.TenantId)
            throw new DomainException("Quotation does not belong to the active tenant.");

        var revision = await quotationRevisionRepository.GetByIdAsync(request.QuotationId, request.RevisionId, cancellationToken)
            ?? throw new DomainException("Quotation revision not found.");

        if (revision.TenantId != request.TenantId)
            throw new DomainException("Quotation revision does not belong to the active tenant.");

        if (request.ExpiresAt.HasValue && request.ExpiresAt.Value <= DateTimeOffset.UtcNow)
            throw new DomainException("Share link expiry must be in the future.");

        var approvals = await approvalRequestRepository.ListByQuotationIdAsync(quotation.Id, cancellationToken);
        var latestApproval = approvals.OrderByDescending(x => x.RequestedAt).FirstOrDefault();
        if (latestApproval is not null)
        {
            if (latestApproval.Status == TravelService.Domain.Enums.QuotationApprovalStatus.Pending)
                throw new DomainException("Quotation cannot be sent while approval is pending.");
            if (latestApproval.Status == TravelService.Domain.Enums.QuotationApprovalStatus.Rejected)
                throw new DomainException("Quotation cannot be sent because the latest approval request was rejected.");
        }

        var previousStatus = quotation.Status.ToString();
        var token = GenerateToken();
        var shareLink = QuotationShareLink.Create(quotation.Id, revision.Id, quotation.TenantId, token, request.ExpiresAt);

        quotation.SetShareToken(token, request.ExpiresAt);
        quotation.Send();

        var revisionReadModel = MapRevisionReadModel(quotation, revision);
        var pdfBytes = pdfDocumentRenderer.RenderQuotationRevisionPdf(revisionReadModel);
        var attachmentFileName = $"quotation-{revision.RevisionNumber}.pdf";
        await using var pdfStream = new MemoryStream(pdfBytes);
        var storageKey = await fileStorage.UploadAsync(pdfStream, $"quotations/{quotation.Id:D}/revisions/{revision.Id:D}/{attachmentFileName}", "application/pdf", cancellationToken);
        var attachment = QuotationAttachment.Create(
            quotation.Id,
            revision.Id,
            quotation.TenantId,
            storageKey,
            attachmentFileName,
            "application/pdf",
            pdfBytes.LongLength,
            "Pdf",
            $"Sent quotation PDF for revision {revision.RevisionNumber}",
            true,
            0,
            actorContext.UserId == Guid.Empty ? null : actorContext.UserId);

        await quotationShareLinkRepository.AddAsync(shareLink, cancellationToken);
        await quotationAttachmentRepository.AddAsync(attachment, cancellationToken);
        await quotationStatusHistoryRepository.AddAsync(
            QuotationStatusHistory.Create(quotation.Id, quotation.TenantId, previousStatus, quotation.Status.ToString(), request.Message),
            cancellationToken);
        await quotationRepository.UpdateAsync(quotation, cancellationToken);
        await activityWriter.WriteAsync(
            ActivityEntry.Create(
                quotation.TenantId,
                "Quotation",
                quotation.Id,
                "Sent",
                $"Quote sent to {request.RecipientEmail}",
                new { request.Channel, request.RecipientEmail, RevisionId = revision.Id, revision.RevisionNumber, shareLink.ExpiresAt },
                actorContext.UserId),
            cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var publicPath = $"/travel/quotations/public/{token}";
        var publicBaseUrl = Environment.GetEnvironmentVariable("TRAVEL_PUBLIC_BASE_URL")?.TrimEnd('/') ?? "http://localhost:5060";
        var publicUrl = $"{publicBaseUrl}{publicPath}";
        var quotationPdfUrl = $"{publicBaseUrl}/travel/documents/quotations/{quotation.Id:D}/revisions/{revision.Id:D}/pdf";

        await communicationWorkflowClient.SendQuotationAsync(request.TenantId, new QuotationCommunicationRequest(
            quotation.CustomerContactId,
            string.IsNullOrWhiteSpace(request.Channel) ? "Email" : request.Channel!,
            $"Your quotation is ready - {quotation.Title}",
            string.IsNullOrWhiteSpace(request.Message)
                ? $"Your quotation for {quotation.Destination} is ready. You can review it using the attached/shared document link."
                : request.Message!,
            quotation.Id.ToString("D"),
            revision.Id.ToString("D"),
            $"quotation-sent:{quotation.Id:D}:{revision.Id:D}",
            [
                new CommunicationDocumentReference(
                    $"quotation-{revision.RevisionNumber}.pdf",
                    revision.Id.ToString("D"),
                    quotationPdfUrl,
                    "application/pdf",
                    null,
                    new Dictionary<string, string> { ["quotationId"] = quotation.Id.ToString("D"), ["revisionId"] = revision.Id.ToString("D") })
            ]), cancellationToken);

        return new SendQuotationResult(shareLink.Id, token, shareLink.ExpiresAt, publicPath);
    }

    private static QuotationRevisionReadModel MapRevisionReadModel(Quotation quotation, QuotationRevision revision)
        => new()
        {
            Id = revision.Id,
            QuotationId = quotation.Id,
            TenantId = quotation.TenantId,
            RevisionNumber = revision.RevisionNumber,
            Status = revision.Status.ToString(),
            CustomerContactId = quotation.CustomerContactId,
            CustomerName = quotation.CustomerName,
            Title = revision.Title,
            Destination = revision.Destination,
            TravelDate = revision.TravelDate,
            ReturnDate = revision.ReturnDate,
            Travellers = revision.Travellers,
            Currency = revision.Currency,
            Notes = quotation.Notes,
            VisibleNotes = revision.VisibleNotes,
            InternalNotes = revision.InternalNotes,
            ValidUntil = revision.ValidUntil,
            SubtotalAmount = revision.SubtotalAmount,
            TaxAmount = revision.TaxAmount,
            TotalAmount = revision.TotalAmount,
            CreatedByUserId = revision.CreatedByUserId,
            CreatedAt = revision.CreatedAt,
            InclusionsJson = revision.InclusionsJson,
            ExclusionsJson = revision.ExclusionsJson,
            PaymentTerms = revision.PaymentTerms,
            CancellationPolicy = revision.CancellationPolicy,
            LineItems = revision.LineItems
                .Select(x => new QuotationRevisionLineItemReadModel(x.Id, x.Description, x.Quantity, x.UnitPriceAmount, x.Currency, x.SortOrder, x.LineTotal))
                .ToList(),
            Attachments = [],
        };

    private static string GenerateToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(24);
        return Convert.ToBase64String(bytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .TrimEnd('=');
    }
}
