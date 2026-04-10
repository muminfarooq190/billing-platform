using MediatR;

namespace TravelService.Application.Commands.QuotationApproval;

public sealed record CreateQuotationApprovalRequestCommand(
    Guid TenantId,
    Guid QuotationId,
    Guid RevisionId,
    string Reason,
    decimal? MarginPercent,
    decimal? DiscountPercent) : IRequest<Guid>;
