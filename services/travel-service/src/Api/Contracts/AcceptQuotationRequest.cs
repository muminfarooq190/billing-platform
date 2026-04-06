namespace TravelService.Api.Contracts;

public sealed record AcceptQuotationRequest(Guid RevisionId, string? Reason);
