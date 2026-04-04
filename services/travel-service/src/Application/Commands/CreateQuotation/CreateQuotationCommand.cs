using MediatR;

namespace TravelService.Application.Commands.CreateQuotation;

public sealed record CreateQuotationCommand(
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
    List<LineItemDto> LineItems) : IRequest<Guid>;

public sealed record LineItemDto(string Description, decimal UnitPrice, int Quantity, string Currency);
