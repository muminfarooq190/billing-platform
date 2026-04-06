using TravelService.Domain.Aggregates;

namespace TravelService.Domain.Repositories;

public interface IContactRepository
{
    Task AddAsync(Contact contact, CancellationToken cancellationToken);
    Task<Contact?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task UpdateAsync(Contact contact, CancellationToken cancellationToken);
}
