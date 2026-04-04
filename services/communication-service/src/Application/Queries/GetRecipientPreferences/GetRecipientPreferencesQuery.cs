using MediatR;

namespace CommunicationService.Application.Queries.GetRecipientPreferences;

public sealed record GetRecipientPreferencesQuery(Guid RecipientId, Guid TenantId) : IRequest<RecipientPreferencesReadModel?>;

public sealed record RecipientPreferencesReadModel(
    Guid Id,
    Guid TenantId,
    Guid RecipientId,
    string RecipientType,
    string Email,
    string Phone,
    string DeviceToken,
    string Timezone,
    string ChannelPreferencesJson,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
