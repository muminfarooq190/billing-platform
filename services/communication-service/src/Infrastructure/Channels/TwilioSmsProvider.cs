using System.Net.Http.Headers;
using System.Text;
using Microsoft.Extensions.Options;

namespace CommunicationService.Infrastructure.Channels;

public sealed class TwilioSmsProvider(HttpClient httpClient, IOptions<SmsChannelOptions> options) : ISmsDeliveryProvider
{
    public string Name => "twilio";

    public async Task<ProviderDispatchResult> SendAsync(SmsMessage message, CancellationToken cancellationToken)
    {
        var settings = options.Value;
        using var request = new HttpRequestMessage(HttpMethod.Post, $"2010-04-01/Accounts/{settings.TwilioAccountSid}/Messages.json");
        var credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{settings.TwilioAccountSid}:{settings.TwilioAuthToken}"));
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", credentials);
        request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["To"] = PhoneNumberNormalizer.Normalize(message.ToPhoneNumber),
            ["From"] = PhoneNumberNormalizer.Normalize(message.FromPhoneNumber),
            ["Body"] = message.Body
        });

        using var response = await httpClient.SendAsync(request, cancellationToken);
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
            return ProviderDispatchResult.Fail($"Twilio returned {(int)response.StatusCode}: {responseBody}");

        var sid = ExtractJsonValue(responseBody, "sid");
        return ProviderDispatchResult.Ok(sid);
    }

    private static string? ExtractJsonValue(string json, string propertyName)
    {
        using var document = System.Text.Json.JsonDocument.Parse(json);
        return document.RootElement.TryGetProperty(propertyName, out var value) ? value.GetString() : null;
    }
}
