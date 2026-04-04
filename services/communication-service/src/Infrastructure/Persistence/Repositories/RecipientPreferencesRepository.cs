using CommunicationService.Domain.Aggregates;
using CommunicationService.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace CommunicationService.Infrastructure.Persistence.Repositories;

public sealed class RecipientPreferencesRepository(CommunicationDbContext dbContext) : IRecipientPreferencesRepository
{
    public Task AddAsync(RecipientPreferences preferences, CancellationToken cancellationToken) => dbContext.RecipientPreferences.AddAsync(preferences, cancellationToken).AsTask();

    public async Task<RecipientPreferences?> GetByRecipientIdAsync(Guid recipientId, Guid tenantId, CancellationToken cancellationToken)
    {
        var preferences = await dbContext.RecipientPreferences.SingleOrDefaultAsync(x => x.RecipientId == recipientId && x.TenantId == tenantId, cancellationToken);
        preferences?.LoadChannelPreferencesFromJson();
        return preferences;
    }

    public Task UpdateAsync(RecipientPreferences preferences, CancellationToken cancellationToken)
    {
        dbContext.RecipientPreferences.Update(preferences);
        return Task.CompletedTask;
    }
}
