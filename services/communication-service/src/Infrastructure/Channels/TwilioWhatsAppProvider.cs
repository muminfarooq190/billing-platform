using System.Net.Http.Headers;
using System.Text;
using Microsoft.Extensions.Options;

namespace CommunicationService.Infrastructure.Channels;

public sealed class TwilioWhatsAppProvider(HttpClient httpClient, IOptions<WhatsAppChannelOptions> options) : IWhatsAppDeliveryProvider
{
    public string Name => "twilio";

    public async Task<ProviderDispatchResult> SendAsync(WhatsAppMessage message, CancellationToken cancellationToken)
    {
        var settings = options.Value;
        using var request = new HttpRequestMessage(HttpMethod.Post, $"2010-04-01/Accounts/{settings.TwilioAccountSid}/Messages.json");
        var credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{settings.TwilioAccountSid}:{settings.TwilioAuthToken}"));
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", credentials);

        var payload = new Dictionary<string, string>
        {
            ["To"] = $"whatsapp:{message.ToPhoneNumber}",
            ["From"] = $"whatsapp:{message.FromPhoneNumber}",
            ["Body"] = message.Body
        };

        if (message.Media is { Count: > 0 })
        {
            for (var i = 0; i < message.Media.Count; i++)
            {
                if (!string.IsNullOrWhiteSpace(message.Media[i].Url))
                    payload[$"MediaUrl{i}"] = message.Media[i].Url!;
            }
        }

        request.Content = new FormUrlEncodedContent(payload);

        using var response = await httpClient.SendAsync(request, cancellationToken);
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
            return ProviderDispatchResult.Fail($"Twilio WhatsApp returned {(int)response.StatusCode}: {responseBody}");

        using var document = System.Text.Json.JsonDocument.Parse(responseBody);
        var sid = document.RootElement.TryGetProperty("sid", out var value) ? value.GetString() : null;
        return ProviderDispatchResult.Ok(sid);
    }
}
