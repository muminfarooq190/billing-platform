using CommunicationService.Application.Abstractions;
using CommunicationService.Domain.Enums;

namespace CommunicationService.Infrastructure.Channels;

public sealed class EmailDispatcher(IConfiguration configuration, ILogger<EmailDispatcher> logger) : IChannelDispatcher
{
    public ChannelType Channel => ChannelType.Email;

    public async Task<ChannelDispatchResult> SendAsync(string recipient, string subject, string body, CancellationToken cancellationToken)
    {
        var provider = configuration["EMAIL_PROVIDER"] ?? "log";

        if (provider.Equals("log", StringComparison.OrdinalIgnoreCase))
        {
            logger.LogInformation("EMAIL to={Recipient} subject={Subject} body_length={BodyLength}", recipient, subject, body.Length);
            await Task.CompletedTask;
            return new ChannelDispatchResult(true, $"log-{Guid.NewGuid():N}", null);
        }

        // Future: SMTP, SendGrid, SES integration points
        logger.LogWarning("Unknown email provider: {Provider}, falling back to log", provider);
        return new ChannelDispatchResult(true, $"log-{Guid.NewGuid():N}", null);
    }
}
