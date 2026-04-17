namespace TravelService.Api.Contracts;

public sealed record ConvertInquiryToQuotationRequest(
    Guid? ContactId,
    string QuotationTitle,
    string Currency,
    string? Notes,
    Guid? AssignedToUserId,
    bool CreateContactIfMissing = true);
