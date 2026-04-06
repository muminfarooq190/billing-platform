using Microsoft.EntityFrameworkCore;
using TravelService.Domain.Aggregates;
using TravelService.Domain.Repositories;

namespace TravelService.Infrastructure.Persistence.Repositories;

public sealed class ContactRepository(TravelDbContext dbContext) : IContactRepository
{
    public Task AddAsync(Contact contact, CancellationToken cancellationToken)
        => dbContext.Set<Contact>().AddAsync(contact, cancellationToken).AsTask();

    public async Task<Contact?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var contact = await dbContext.Set<Contact>().SingleOrDefaultAsync(x => x.Id == id && x.DeletedAt == null, cancellationToken);
        contact?.LoadTagsFromJson();
        return contact;
    }

    public Task UpdateAsync(Contact contact, CancellationToken cancellationToken)
    {
        dbContext.Set<Contact>().Update(contact);
        return Task.CompletedTask;
    }
}
