using MediatR;

namespace TravelService.Application.Queries.Contacts;

public sealed record GetContactByIdQuery(Guid Id) : IRequest<ContactReadModel?>;
