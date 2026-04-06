using MediatR;

namespace TravelService.Application.Queries.Contacts;

public sealed record ListContactsByTenantQuery(Guid TenantId, int Page = 1, int PageSize = 20)
    : IRequest<PagedResult<ContactReadModel>>;
