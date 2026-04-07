using MediatR;

namespace TravelService.Application.Commands.CreateQuotationRevision;

public sealed record CreateQuotationRevisionCommand(
    Guid TenantId,
    Guid QuotationId,
    string Title,
    string Destination,
    DateTimeOffset TravelDate,
    DateTimeOffset ReturnDate,
    int Travellers,
    string Currency,
    string VisibleNotes,
    string InternalNotes,
    DateTimeOffset ValidUntil,
    List<QuotationRevisionLineItemDto> LineItems) : IRequest<CreateQuotationRevisionResult>;

public sealed record QuotationRevisionLineItemDto(string Description, decimal UnitPrice, int Quantity, string Currency);

public sealed record CreateQuotationRevisionResult(Guid RevisionId, int RevisionNumber);
