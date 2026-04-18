using System.Text.Json;
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

        var (renderedBody, attachments) = TryExtractDocumentAwareBody(body);
        var result = await provider.SendAsync(new EmailMessage(recipient, settings.DefaultFromEmail ?? string.Empty, settings.DefaultFromName, subject, renderedBody, attachments), cancellationToken);
        if (!result.Success)
            logger.LogWarning("Email dispatch failed via provider {Provider}: {Error}", provider.Name, result.ErrorMessage);

        return new ChannelDispatchResult(result.Success, result.ProviderMessageId, result.ErrorMessage);
    }

    private static (string Body, IReadOnlyList<EmailAttachmentReference> Attachments) TryExtractDocumentAwareBody(string body)
    {
        const string marker = "\n\n[DocumentReferencesJson]";
        var markerIndex = body.IndexOf(marker, StringComparison.Ordinal);
        if (markerIndex < 0)
            return (body, []);

        var displayBody = body[..markerIndex].TrimEnd();
        var json = body[(markerIndex + marker.Length)..].Trim();
        try
        {
            var docs = JsonSerializer.Deserialize<List<EmailDocumentPayload>>(json) ?? [];
            if (docs.Count == 0)
                return (displayBody, []);

            var attachmentRefs = docs
                .Select(x => new EmailAttachmentReference(x.Name, x.Url, x.ContentType))
                .ToList();
            var lines = docs.Select((doc, index) => $"{index + 1}. {doc.Name}: {doc.Url ?? doc.DocumentId ?? "document reference"}");
            var enhancedBody = $"{displayBody}\n\nDocuments:\n{string.Join("\n", lines)}";
            return (enhancedBody, attachmentRefs);
        }
        catch (JsonException)
        {
            return (body, []);
        }
    }

    private sealed record EmailDocumentPayload(string Name, string? DocumentId, string? Url, string? ContentType, long? SizeBytes, Dictionary<string, string>? Metadata);
}
