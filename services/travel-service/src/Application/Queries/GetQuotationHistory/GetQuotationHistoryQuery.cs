using MediatR;

namespace TravelService.Application.Queries.GetQuotationHistory;

public sealed record GetQuotationHistoryQuery(Guid TenantId, Guid QuotationId) : IRequest<IReadOnlyList<QuotationHistoryReadModel>>;

public sealed record QuotationHistoryReadModel(
    Guid Id,
    Guid QuotationId,
    Guid TenantId,
    string? FromStatus,
    string ToStatus,
    string? Reason,
    Guid? ChangedByUserId,
    DateTimeOffset CreatedAt);
