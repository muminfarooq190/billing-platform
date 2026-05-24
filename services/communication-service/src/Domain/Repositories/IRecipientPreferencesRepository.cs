using CommunicationService.Domain.Aggregates;

namespace CommunicationService.Domain.Repositories;

public interface IRecipientPreferencesRepository
{
    Task AddAsync(RecipientPreferences preferences, CancellationToken cancellationToken);
    Task<RecipientPreferences?> GetByRecipientIdAsync(Guid recipientId, Guid tenantId, CancellationToken cancellationToken);

    /// <summary>
    /// Lookup preferences rows by phone number. Used by the Twilio inbound
    /// STOP handler — the recipient identifies themselves by the phone
    /// number they texted from, not by id. Returns ALL matching rows
    /// because the same number may belong to multiple recipients across
    /// tenants.
    /// </summary>
    Task<IReadOnlyList<RecipientPreferences>> ListByPhoneAsync(string phone, CancellationToken cancellationToken);

    Task UpdateAsync(RecipientPreferences preferences, CancellationToken cancellationToken);
}
