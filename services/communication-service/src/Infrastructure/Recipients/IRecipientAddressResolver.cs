using CommunicationService.Domain.Aggregates;
using CommunicationService.Domain.Enums;

namespace CommunicationService.Infrastructure.Recipients;

public interface IRecipientAddressResolver
{
    Task<string?> ResolveAsync(Notification notification, RecipientPreferences? preferences, CancellationToken cancellationToken);
}
