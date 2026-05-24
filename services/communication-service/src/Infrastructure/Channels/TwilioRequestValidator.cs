using System.Security.Cryptography;
using System.Text;

namespace CommunicationService.Infrastructure.Channels;

/// <summary>
/// Validates Twilio's <c>X-Twilio-Signature</c> header per Twilio's spec:
/// HMAC-SHA1(authToken, requestUrl + concat(sortedFormParam.Key + value)).
/// Result is Base64-encoded and compared to the header value.
///
/// Reference: https://www.twilio.com/docs/usage/webhooks/webhooks-security
///
/// Validation is REQUIRED in production — without it, anyone can POST a
/// fake delivery status and corrupt our Notification ledger.
/// </summary>
public interface ITwilioRequestValidator
{
    bool IsValid(string authToken, string fullRequestUrl, IDictionary<string, string> formParameters, string? signatureHeader);
}

public sealed class TwilioRequestValidator : ITwilioRequestValidator
{
    public bool IsValid(string authToken, string fullRequestUrl, IDictionary<string, string> formParameters, string? signatureHeader)
    {
        if (string.IsNullOrWhiteSpace(authToken)) return false;
        if (string.IsNullOrWhiteSpace(signatureHeader)) return false;

        // Build the canonical string: full URL + concat(key+value) for each
        // form param, sorted alphabetically by key. Twilio uses the request
        // URL as the consumer saw it (including any query string).
        var canonical = new StringBuilder(fullRequestUrl);
        foreach (var pair in formParameters.OrderBy(p => p.Key, StringComparer.Ordinal))
        {
            canonical.Append(pair.Key).Append(pair.Value);
        }

        using var hmac = new HMACSHA1(Encoding.UTF8.GetBytes(authToken));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(canonical.ToString()));
        var expected = Convert.ToBase64String(hash);

        // Constant-time compare to avoid timing oracles.
        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(expected),
            Encoding.UTF8.GetBytes(signatureHeader));
    }
}
