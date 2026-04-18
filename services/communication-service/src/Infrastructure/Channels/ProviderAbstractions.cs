namespace CommunicationService.Infrastructure.Channels;

public interface IEmailDeliveryProvider
{
    string Name { get; }
    Task<ProviderDispatchResult> SendAsync(EmailMessage message, CancellationToken cancellationToken);
}

public interface ISmsDeliveryProvider
{
    string Name { get; }
    Task<ProviderDispatchResult> SendAsync(SmsMessage message, CancellationToken cancellationToken);
}

public interface IWhatsAppDeliveryProvider
{
    string Name { get; }
    Task<ProviderDispatchResult> SendAsync(WhatsAppMessage message, CancellationToken cancellationToken);
}

public sealed record EmailAttachmentReference(string Name, string? Url, string? ContentType, byte[]? Content = null);
public sealed record EmailMessage(string ToEmail, string FromEmail, string? FromName, string Subject, string Body, IReadOnlyList<EmailAttachmentReference>? Attachments = null);
public sealed record SmsMessage(string ToPhoneNumber, string FromPhoneNumber, string Body);
public sealed record WhatsAppMediaReference(string Name, string? Url, string? ContentType);
public sealed record WhatsAppMessage(string ToPhoneNumber, string FromPhoneNumber, string Body, IReadOnlyList<WhatsAppMediaReference>? Media = null);
public sealed record ProviderDispatchResult(bool Success, string? ProviderMessageId, string? ErrorMessage)
{
    public static ProviderDispatchResult Ok(string? providerMessageId) => new(true, providerMessageId, null);
    public static ProviderDispatchResult Fail(string errorMessage) => new(false, null, errorMessage);
}
