using CommunicationService.Application.Abstractions;
using CommunicationService.Domain.Enums;
using Microsoft.Extensions.Options;

namespace CommunicationService.Infrastructure.Channels;

public sealed class SmsDispatcher(
    SmsProviderResolver providerResolver,
    IOptions<SmsChannelOptions> options,
    ILogger<SmsDispatcher> logger) : IChannelDispatcher
{
    public ChannelType Channel => ChannelType.Sms;

    public async Task<ChannelDispatchResult> SendAsync(string recipient, string subject, string body, CancellationToken cancellationToken)
    {
        if (!ChannelValidators.IsKnownPhoneNumber(recipient))
            return new ChannelDispatchResult(false, null, $"Recipient '{recipient}' is not a valid E.164 phone number.");

        var settings = options.Value;
        var provider = providerResolver.Resolve();
        var result = await provider.SendAsync(new SmsMessage(recipient, settings.DefaultFromNumber ?? string.Empty, body), cancellationToken);
        if (!result.Success)
            logger.LogWarning("Sms dispatch failed via provider {Provider}: {Error}", provider.Name, result.ErrorMessage);

        return new ChannelDispatchResult(result.Success, result.ProviderMessageId, result.ErrorMessage);
    }
}
