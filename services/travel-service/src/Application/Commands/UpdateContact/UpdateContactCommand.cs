using MediatR;

namespace TravelService.Application.Commands.UpdateContact;

public sealed record UpdateContactCommand(
    Guid Id,
    string FirstName,
    string LastName,
    string? Email,
    string? Phone,
    string? Company,
    string? Notes,
    IReadOnlyCollection<string>? Tags) : IRequest;
