namespace TravelService.Api.Contracts;

public sealed record CreateQuotationApprovalRequestRequest(
    Guid RevisionId,
    string Reason,
    decimal? MarginPercent,
    decimal? DiscountPercent);

public sealed record DecideQuotationApprovalRequest(
    string? Reason);
