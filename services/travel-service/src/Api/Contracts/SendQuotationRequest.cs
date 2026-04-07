namespace TravelService.Api.Contracts;

public sealed record SendQuotationRequest(
    Guid RevisionId,
    string? Channel,
    string? RecipientEmail,
    string? Message,
    DateTimeOffset? ExpiresAt);
