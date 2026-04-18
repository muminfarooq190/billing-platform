using CommunicationService.Application.Abstractions;
using CommunicationService.Domain.Enums;
using Microsoft.Extensions.Options;

namespace CommunicationService.Infrastructure.Channels;

public sealed class WhatsAppDispatcher(
    WhatsAppProviderResolver providerResolver,
    IOptions<WhatsAppChannelOptions> options,
    ILogger<WhatsAppDispatcher> logger) : IChannelDispatcher
{
    public ChannelType Channel => ChannelType.WhatsApp;

    public async Task<ChannelDispatchResult> SendAsync(string recipient, string subject, string body, CancellationToken cancellationToken)
    {
        if (!ChannelValidators.IsKnownPhoneNumber(recipient))
            return new ChannelDispatchResult(false, null, $"Recipient '{recipient}' is not a valid E.164 phone number.");

        var settings = options.Value;
        var provider = providerResolver.Resolve();
        var result = await provider.SendAsync(new WhatsAppMessage(recipient, settings.DefaultFromNumber ?? string.Empty, string.IsNullOrWhiteSpace(subject) ? body : $"{subject}\n\n{body}"), cancellationToken);
        if (!result.Success)
            logger.LogWarning("WhatsApp dispatch failed via provider {Provider}: {Error}", provider.Name, result.ErrorMessage);

        return new ChannelDispatchResult(result.Success, result.ProviderMessageId, result.ErrorMessage);
    }
}
