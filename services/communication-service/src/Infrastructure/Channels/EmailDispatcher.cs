using CommunicationService.Application.Abstractions;
using CommunicationService.Domain.Enums;
using Microsoft.Extensions.Options;

namespace CommunicationService.Infrastructure.Channels;

public sealed class EmailDispatcher(
    EmailProviderResolver providerResolver,
    IOptions<EmailChannelOptions> options,
    ILogger<EmailDispatcher> logger) : IChannelDispatcher
{
    public ChannelType Channel => ChannelType.Email;

    public async Task<ChannelDispatchResult> SendAsync(string recipient, string subject, string body, CancellationToken cancellationToken)
    {
        if (!ChannelValidators.IsKnownEmail(recipient))
            return new ChannelDispatchResult(false, null, $"Recipient '{recipient}' is not a valid email address.");

        var settings = options.Value;
        var provider = providerResolver.Resolve();
        if (provider.Name.Equals("sendgrid", StringComparison.OrdinalIgnoreCase) && string.IsNullOrWhiteSpace(subject))
            return new ChannelDispatchResult(false, null, "Email subject is required for SendGrid delivery.");

        var result = await provider.SendAsync(new EmailMessage(recipient, settings.DefaultFromEmail ?? string.Empty, settings.DefaultFromName, subject, body), cancellationToken);
        if (!result.Success)
            logger.LogWarning("Email dispatch failed via provider {Provider}: {Error}", provider.Name, result.ErrorMessage);

        return new ChannelDispatchResult(result.Success, result.ProviderMessageId, result.ErrorMessage);
    }
}
