using CommunicationService.Domain.Aggregates;

namespace CommunicationService.Domain.Repositories;

public interface IRecipientPreferencesRepository
{
    Task AddAsync(RecipientPreferences preferences, CancellationToken cancellationToken);
    Task<RecipientPreferences?> GetByRecipientIdAsync(Guid recipientId, Guid tenantId, CancellationToken cancellationToken);
    Task UpdateAsync(RecipientPreferences preferences, CancellationToken cancellationToken);
}
