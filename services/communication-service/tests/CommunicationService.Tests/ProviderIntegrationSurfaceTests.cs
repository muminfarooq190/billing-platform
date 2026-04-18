using CommunicationService.Infrastructure.Channels;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace CommunicationService.Tests;

public sealed class ProviderIntegrationSurfaceTests
{
    [Fact]
    public void EmailChannelOptionsValidator_ShouldRequireSendGridConfig_WhenSendGridEnabled()
    {
        var validator = new EmailChannelOptionsValidator();

        var result = validator.Validate(null, new EmailChannelOptions
        {
            Provider = "sendgrid"
        });

        result.Failed.Should().BeTrue();
        result.Failures.Should().Contain(failure => failure.Contains("SendGridApiKey"));
        result.Failures.Should().Contain(failure => failure.Contains("DefaultFromEmail"));
    }

    [Fact]
    public void SmsChannelOptionsValidator_ShouldRequireTwilioConfig_WhenTwilioEnabled()
    {
        var validator = new SmsChannelOptionsValidator();

        var result = validator.Validate(null, new SmsChannelOptions
        {
            Provider = "twilio"
        });

        result.Failed.Should().BeTrue();
        result.Failures.Should().Contain(failure => failure.Contains("TwilioAccountSid"));
        result.Failures.Should().Contain(failure => failure.Contains("DefaultFromNumber"));
    }

    [Fact]
    public async Task EmailDispatcher_ShouldRejectInvalidEmailRecipients()
    {
        var dispatcher = new EmailDispatcher(
            new EmailProviderResolver([new LogEmailProvider(NullLogger<LogEmailProvider>.Instance)], Options.Create(new EmailChannelOptions())),
            new DummyHttpClientFactory(),
            Options.Create(new EmailChannelOptions()),
            NullLogger<EmailDispatcher>.Instance);

        var result = await dispatcher.SendAsync("not-an-email", "hello", "body", CancellationToken.None);

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("valid email address");
    }

    [Fact]
    public async Task SmsDispatcher_ShouldRejectInvalidPhoneRecipients()
    {
        var dispatcher = new SmsDispatcher(
            new SmsProviderResolver([new LogSmsProvider(NullLogger<LogSmsProvider>.Instance)], Options.Create(new SmsChannelOptions())),
            Options.Create(new SmsChannelOptions()),
            NullLogger<SmsDispatcher>.Instance);

        var result = await dispatcher.SendAsync("555-1212", string.Empty, "body", CancellationToken.None);

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("valid E.164 phone number");
    }

    [Fact]
    public async Task EmailDispatcher_ShouldUseConfiguredLogProvider_WhenRecipientIsValid()
    {
        var dispatcher = new EmailDispatcher(
            new EmailProviderResolver([new LogEmailProvider(NullLogger<LogEmailProvider>.Instance)], Options.Create(new EmailChannelOptions { Provider = "log", DefaultFromEmail = "noreply@example.com" })),
            new DummyHttpClientFactory(),
            Options.Create(new EmailChannelOptions { Provider = "log", DefaultFromEmail = "noreply@example.com" }),
            NullLogger<EmailDispatcher>.Instance);

        var result = await dispatcher.SendAsync("customer@example.com", "subject", "body", CancellationToken.None);

        result.Success.Should().BeTrue();
        result.ProviderMessageId.Should().StartWith("log-");
    }

    private sealed class DummyHttpClientFactory : IHttpClientFactory
    {
        public HttpClient CreateClient(string name) => new();
    }
}
