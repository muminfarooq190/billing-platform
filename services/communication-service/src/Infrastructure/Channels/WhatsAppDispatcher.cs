using System.Text.Json;
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
        var (renderedBody, media) = TryExtractDocumentAwareBody(body);
        var result = await provider.SendAsync(new WhatsAppMessage(recipient, settings.DefaultFromNumber ?? string.Empty, string.IsNullOrWhiteSpace(subject) ? renderedBody : $"{subject}\n\n{renderedBody}", media), cancellationToken);
        if (!result.Success)
            logger.LogWarning("WhatsApp dispatch failed via provider {Provider}: {Error}", provider.Name, result.ErrorMessage);

        return new ChannelDispatchResult(result.Success, result.ProviderMessageId, result.ErrorMessage);
    }

    private static (string Body, IReadOnlyList<WhatsAppMediaReference> Media) TryExtractDocumentAwareBody(string body)
    {
        const string marker = "\n\n[DocumentReferencesJson]";
        var markerIndex = body.IndexOf(marker, StringComparison.Ordinal);
        if (markerIndex < 0)
            return (body, []);

        var displayBody = body[..markerIndex].TrimEnd();
        var json = body[(markerIndex + marker.Length)..].Trim();
        try
        {
            var docs = JsonSerializer.Deserialize<List<DocumentPayload>>(json) ?? [];
            var media = docs
                .Where(x => !string.IsNullOrWhiteSpace(x.Url))
                .Select(x => new WhatsAppMediaReference(x.Name, x.Url, x.ContentType))
                .ToList();
            return (displayBody, media);
        }
        catch (JsonException)
        {
            return (body, []);
        }
    }

    private sealed record DocumentPayload(string Name, string? DocumentId, string? Url, string? ContentType, long? SizeBytes, Dictionary<string, string>? Metadata);
}
