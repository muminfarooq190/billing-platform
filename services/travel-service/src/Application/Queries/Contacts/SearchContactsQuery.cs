using MediatR;

namespace TravelService.Application.Queries.Contacts;

public sealed record SearchContactsQuery(Guid TenantId, string? SearchTerm, int Page = 1, int PageSize = 20)
    : IRequest<PagedResult<ContactReadModel>>;
