using MediatR;

namespace TravelService.Application.Commands.TravelInquiries;

public sealed record ConvertInquiryToQuotationCommand(
    Guid TenantId,
    Guid InquiryId,
    Guid? ContactId,
    string QuotationTitle,
    string Currency,
    string? Notes,
    Guid? AssignedToUserId,
    bool CreateContactIfMissing = true) : IRequest<ConvertInquiryToQuotationResult>;

public sealed record ConvertInquiryToQuotationResult(Guid InquiryId, Guid ContactId, Guid QuotationId);
