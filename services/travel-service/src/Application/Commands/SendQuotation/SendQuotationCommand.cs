using MediatR;

namespace TravelService.Application.Commands.SendQuotation;

public sealed record SendQuotationCommand(
    Guid TenantId,
    Guid QuotationId,
    Guid RevisionId,
    string? Channel,
    string? RecipientEmail,
    string? Message,
    DateTimeOffset? ExpiresAt) : IRequest<SendQuotationResult>;

public sealed record SendQuotationResult(
    Guid ShareLinkId,
    string Token,
    DateTimeOffset? ExpiresAt,
    string PublicUrl);
