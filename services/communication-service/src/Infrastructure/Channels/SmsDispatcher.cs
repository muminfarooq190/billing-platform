using CommunicationService.Application.Abstractions;
using CommunicationService.Domain.Enums;

namespace CommunicationService.Infrastructure.Channels;

public sealed class SmsDispatcher(IConfiguration configuration, ILogger<SmsDispatcher> logger) : IChannelDispatcher
{
    public ChannelType Channel => ChannelType.Sms;

    public async Task<ChannelDispatchResult> SendAsync(string recipient, string subject, string body, CancellationToken cancellationToken)
    {
        var provider = configuration["SMS_PROVIDER"] ?? "log";

        if (provider.Equals("log", StringComparison.OrdinalIgnoreCase))
        {
            logger.LogInformation("SMS to={Recipient} body_length={BodyLength}", recipient, body.Length);
            await Task.CompletedTask;
            return new ChannelDispatchResult(true, $"log-{Guid.NewGuid():N}", null);
        }

        // Future: Twilio, Vonage integration points
        logger.LogWarning("Unknown SMS provider: {Provider}, falling back to log", provider);
        return new ChannelDispatchResult(true, $"log-{Guid.NewGuid():N}", null);
    }
}
