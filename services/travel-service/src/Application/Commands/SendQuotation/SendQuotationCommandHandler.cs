using System.Security.Cryptography;
using MediatR;
using TravelService.Application.Abstractions;
using TravelService.Domain.Aggregates;
using TravelService.Domain.Exceptions;
using TravelService.Domain.Repositories;

namespace TravelService.Application.Commands.SendQuotation;

public sealed class SendQuotationCommandHandler(
    IQuotationRepository quotationRepository,
    IQuotationRevisionRepository quotationRevisionRepository,
    IQuotationShareLinkRepository quotationShareLinkRepository,
    IQuotationStatusHistoryRepository quotationStatusHistoryRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<SendQuotationCommand, SendQuotationResult>
{
    public async Task<SendQuotationResult> Handle(SendQuotationCommand request, CancellationToken cancellationToken)
    {
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

        var previousStatus = quotation.Status.ToString();
        var token = GenerateToken();
        var shareLink = QuotationShareLink.Create(quotation.Id, revision.Id, quotation.TenantId, token, request.ExpiresAt);

        quotation.SetShareToken(token, request.ExpiresAt);
        quotation.Send();

        await quotationShareLinkRepository.AddAsync(shareLink, cancellationToken);
        await quotationStatusHistoryRepository.AddAsync(
            QuotationStatusHistory.Create(quotation.Id, quotation.TenantId, previousStatus, quotation.Status.ToString(), request.Message),
            cancellationToken);
        await quotationRepository.UpdateAsync(quotation, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new SendQuotationResult(shareLink.Id, token, shareLink.ExpiresAt, $"/travel/quotations/public/{token}");
    }

    private static string GenerateToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(24);
        return Convert.ToBase64String(bytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .TrimEnd('=');
    }
}
