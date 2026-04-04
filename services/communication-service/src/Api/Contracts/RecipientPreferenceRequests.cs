namespace CommunicationService.Api.Contracts;

public sealed record UpdateRecipientPreferencesRequest(
    Guid TenantId,
    Guid RecipientId,
    string RecipientType,
    string Email,
    string? Phone,
    string? DeviceToken,
    string? Timezone,
    List<ChannelPreferenceRequest>? ChannelPreferences);

public sealed record ChannelPreferenceRequest(
    string Channel,
    bool Enabled,
    bool QuietHoursEnabled,
    string? QuietStart,
    string? QuietEnd);
