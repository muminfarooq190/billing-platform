using MediatR;

namespace TravelService.Application.Commands.UpdateQuotation;

public sealed record UpdateQuotationCommand(
    Guid Id,
    string Title,
    string Destination,
    DateTimeOffset TravelDate,
    DateTimeOffset ReturnDate,
    int Travellers,
    string Currency,
    string Notes,
    DateTimeOffset ValidUntil,
    string? Action) : IRequest;
