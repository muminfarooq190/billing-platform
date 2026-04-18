using CommunicationService.Application.Abstractions;
using CommunicationService.Domain.Aggregates;
using CommunicationService.Domain.Enums;

namespace CommunicationService.Infrastructure.Channels;

public sealed class ChannelPreferenceResolver : IChannelPreferenceResolver
{
    private static readonly ChannelType[] PreferredOrder =
    [
        ChannelType.WhatsApp,
        ChannelType.Email,
        ChannelType.Sms,
        ChannelType.InApp,
        ChannelType.PushNotification
    ];

    public ChannelType ResolvePreferredChannel(string? requestedChannel, RecipientPreferences? preferences, ChannelType defaultChannel = ChannelType.Email)
    {
        if (!string.IsNullOrWhiteSpace(requestedChannel))
            return Enum.Parse<ChannelType>(requestedChannel, true);

        if (preferences is null)
            return defaultChannel;

        foreach (var channel in PreferredOrder)
        {
            if (preferences.IsChannelEnabled(channel))
                return channel;
        }

        return defaultChannel;
    }
}
