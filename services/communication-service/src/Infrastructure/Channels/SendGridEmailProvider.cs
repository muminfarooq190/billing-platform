using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace CommunicationService.Infrastructure.Channels;

public sealed class SendGridEmailProvider(HttpClient httpClient, IOptions<EmailChannelOptions> options) : IEmailDeliveryProvider
{
    public string Name => "sendgrid";

    public async Task<ProviderDispatchResult> SendAsync(EmailMessage message, CancellationToken cancellationToken)
    {
        var settings = options.Value;
        using var request = new HttpRequestMessage(HttpMethod.Post, "v3/mail/send");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", settings.SendGridApiKey);
        request.Content = new StringContent(JsonSerializer.Serialize(new
        {
            from = new { email = message.FromEmail, name = message.FromName },
            personalizations = new[]
            {
                new
                {
                    to = new[] { new { email = message.ToEmail } },
                    subject = message.Subject
                }
            },
            content = new[]
            {
                new { type = "text/plain", value = message.Body }
            },
            attachments = (message.Attachments ?? [])
                .Where(x => x.Content is { Length: > 0 })
                .Select(x => new
                {
                    content = Convert.ToBase64String(x.Content!),
                    type = x.ContentType ?? "application/octet-stream",
                    filename = x.Name,
                    disposition = "attachment"
                })
                .ToArray()
        }), Encoding.UTF8, "application/json");

        using var response = await httpClient.SendAsync(request, cancellationToken);
        if (response.IsSuccessStatusCode)
        {
            var providerMessageId = response.Headers.TryGetValues("X-Message-Id", out var values)
                ? values.FirstOrDefault()
                : null;
            return ProviderDispatchResult.Ok(providerMessageId);
        }

        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        return ProviderDispatchResult.Fail($"SendGrid returned {(int)response.StatusCode}: {body}");
    }
}
