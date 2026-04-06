using MediatR;

namespace TravelService.Application.Commands.CreateContact;

public sealed record CreateContactCommand(
    Guid TenantId,
    string FirstName,
    string LastName,
    string? Email,
    string? Phone,
    string? Company,
    string? Notes,
    IReadOnlyCollection<string>? Tags) : IRequest<Guid>;
