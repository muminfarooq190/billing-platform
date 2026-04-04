using Microsoft.Extensions.Options;

namespace CommunicationService.Infrastructure.Channels;

public sealed class LogEmailProvider(ILogger<LogEmailProvider> logger) : IEmailDeliveryProvider
{
    public string Name => "log";

    public Task<ProviderDispatchResult> SendAsync(EmailMessage message, CancellationToken cancellationToken)
    {
        logger.LogInformation("EMAIL provider=log to={Recipient} from={FromEmail} subject={Subject} body_length={BodyLength}", message.ToEmail, message.FromEmail, message.Subject, message.Body.Length);
        return Task.FromResult(ProviderDispatchResult.Ok($"log-{Guid.NewGuid():N}"));
    }
}

public sealed class LogSmsProvider(ILogger<LogSmsProvider> logger) : ISmsDeliveryProvider
{
    public string Name => "log";

    public Task<ProviderDispatchResult> SendAsync(SmsMessage message, CancellationToken cancellationToken)
    {
        logger.LogInformation("SMS provider=log to={Recipient} from={FromPhoneNumber} body_length={BodyLength}", message.ToPhoneNumber, message.FromPhoneNumber, message.Body.Length);
        return Task.FromResult(ProviderDispatchResult.Ok($"log-{Guid.NewGuid():N}"));
    }
}

public sealed class EmailProviderResolver(IEnumerable<IEmailDeliveryProvider> providers, IOptions<EmailChannelOptions> options)
{
    private readonly Dictionary<string, IEmailDeliveryProvider> _providers = providers.ToDictionary(provider => provider.Name, StringComparer.OrdinalIgnoreCase);

    public IEmailDeliveryProvider Resolve()
    {
        var providerName = options.Value.Provider ?? "log";
        return _providers.TryGetValue(providerName, out var provider)
            ? provider
            : _providers["log"];
    }
}

public sealed class SmsProviderResolver(IEnumerable<ISmsDeliveryProvider> providers, IOptions<SmsChannelOptions> options)
{
    private readonly Dictionary<string, ISmsDeliveryProvider> _providers = providers.ToDictionary(provider => provider.Name, StringComparer.OrdinalIgnoreCase);

    public ISmsDeliveryProvider Resolve()
    {
        var providerName = options.Value.Provider ?? "log";
        return _providers.TryGetValue(providerName, out var provider)
            ? provider
            : _providers["log"];
    }
}
