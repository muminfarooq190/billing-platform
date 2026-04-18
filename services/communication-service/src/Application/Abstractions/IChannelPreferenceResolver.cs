using CommunicationService.Domain.Aggregates;
using CommunicationService.Domain.Enums;

namespace CommunicationService.Application.Abstractions;

public interface IChannelPreferenceResolver
{
    ChannelType ResolvePreferredChannel(string? requestedChannel, RecipientPreferences? preferences, ChannelType defaultChannel = ChannelType.Email);
}
