using CommunicationService.Infrastructure.Channels;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace CommunicationService.Tests.Infrastructure;

public sealed class WhatsAppDispatcherTests
{
    [Fact]
    public async Task WhatsAppDispatcher_ShouldRejectInvalidPhoneNumber()
    {
        var dispatcher = new WhatsAppDispatcher(
            new WhatsAppProviderResolver([new LogWhatsAppProvider(new NullLogger<LogWhatsAppProvider>())], Options.Create(new WhatsAppChannelOptions())),
            Options.Create(new WhatsAppChannelOptions { DefaultFromNumber = "+1234567890" }),
            new NullLogger<WhatsAppDispatcher>());

        var result = await dispatcher.SendAsync("not-a-phone", "Subject", "Body", CancellationToken.None);

        result.Success.Should().BeFalse();
    }
}
