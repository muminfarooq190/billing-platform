using CommunicationService.Domain.Aggregates;
using CommunicationService.Domain.Enums;
using CommunicationService.Domain.ValueObjects;
using CommunicationService.Infrastructure.Channels;
using FluentAssertions;

namespace CommunicationService.Tests.Infrastructure;

public sealed class ChannelPreferenceResolverTests
{
    [Fact]
    public void ChannelPreferenceResolver_ShouldChooseEnabledPreferredChannel_WhenChannelNotRequested()
    {
        var preferences = RecipientPreferences.Create(Guid.NewGuid(), Guid.NewGuid(), RecipientType.EndUser, "customer@example.com", "+917006501588", null);
        preferences.SetChannelPreferences(
        [
            new ChannelPreference(ChannelType.Email, true, false, null, null),
            new ChannelPreference(ChannelType.WhatsApp, true, false, null, null),
            new ChannelPreference(ChannelType.Sms, false, false, null, null)
        ]);
        var resolver = new ChannelPreferenceResolver();

        var result = resolver.ResolvePreferredChannel(null, preferences, ChannelType.Email);

        result.Should().Be(ChannelType.WhatsApp);
    }
}
