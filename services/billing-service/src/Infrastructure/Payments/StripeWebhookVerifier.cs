using System.Security.Cryptography;
using System.Text;

namespace BillingService.Infrastructure.Payments;

public interface IStripeWebhookVerifier
{
    bool IsValid(string payload, string? signatureHeader, string secret, out string? failureReason);
}

public sealed class StripeWebhookVerifier : IStripeWebhookVerifier
{
    public bool IsValid(string payload, string? signatureHeader, string secret, out string? failureReason)
    {
        failureReason = null;

        if (string.IsNullOrWhiteSpace(signatureHeader))
        {
            failureReason = "Missing Stripe-Signature header.";
            return false;
        }

        var parts = signatureHeader.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var timestampPart = parts.FirstOrDefault(x => x.StartsWith("t=", StringComparison.OrdinalIgnoreCase));
        var signaturePart = parts.FirstOrDefault(x => x.StartsWith("v1=", StringComparison.OrdinalIgnoreCase));

        if (timestampPart is null || signaturePart is null)
        {
            failureReason = "Stripe-Signature header is missing required t= or v1= components.";
            return false;
        }

        var timestamp = timestampPart[2..];
        var providedSignature = signaturePart[3..];
        var signedPayload = $"{timestamp}.{payload}";

        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        var computed = Convert.ToHexString(hmac.ComputeHash(Encoding.UTF8.GetBytes(signedPayload))).ToLowerInvariant();

        if (!CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(computed), Encoding.UTF8.GetBytes(providedSignature.ToLowerInvariant())))
        {
            failureReason = "Stripe webhook signature verification failed.";
            return false;
        }

        return true;
    }
}
