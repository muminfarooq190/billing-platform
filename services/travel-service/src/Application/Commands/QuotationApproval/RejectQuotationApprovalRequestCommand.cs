using MediatR;

namespace TravelService.Application.Commands.QuotationApproval;

public sealed record RejectQuotationApprovalRequestCommand(
    Guid TenantId,
    Guid QuotationId,
    Guid ApprovalRequestId,
    string? Reason) : IRequest;
