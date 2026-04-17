namespace TravelService.Api.Contracts;

public sealed record AssignInquiryRequest(Guid? AssignedToUserId);
public sealed record InquiryStatusReasonRequest(string? Reason);
