using MediatR;

namespace CommunicationService.Application.Commands.UpdateRecipientPreferences;

public sealed record UpdateRecipientPreferencesCommand(
    Guid TenantId,
    Guid RecipientId,
    string RecipientType,
    string Email,
    string? Phone,
    string? DeviceToken,
    string? Timezone,
    List<ChannelPreferenceDto>? ChannelPreferences) : IRequest<Guid>;

public sealed record ChannelPreferenceDto(string Channel, bool Enabled, bool QuietHoursEnabled, string? QuietStart, string? QuietEnd);
