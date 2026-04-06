using MediatR;

namespace TravelService.Application.Queries.GetQuotationById;

public sealed record GetQuotationByIdQuery(Guid Id) : IRequest<QuotationReadModel?>;

public sealed record QuotationReadModel(
    Guid Id,
    Guid TenantId,
    Guid CustomerContactId,
    string CustomerName,
    string Title,
    string Destination,
    DateTimeOffset TravelDate,
    DateTimeOffset ReturnDate,
    int Travellers,
    string Currency,
    string Notes,
    string Status,
    DateTimeOffset ValidUntil,
    int CurrentRevisionNumber,
    Guid? AcceptedRevisionId,
    decimal TotalAmount,
    DateTimeOffset? LastSentAt,
    DateTimeOffset? LastViewedAt,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
