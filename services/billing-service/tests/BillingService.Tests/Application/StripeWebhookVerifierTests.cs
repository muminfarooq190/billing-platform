using System.Security.Cryptography;
using System.Text;
using BillingService.Infrastructure.Payments;
using FluentAssertions;

namespace BillingService.Tests.Application;

public sealed class StripeWebhookVerifierTests
{
    [Fact]
    public void StripeWebhookVerifier_ShouldAcceptValidSignature()
    {
        var secret = "whsec_test_secret";
        var payload = "{\"type\":\"payment_intent.succeeded\"}";
        var timestamp = "1713457800";
        var signature = ComputeSignature(secret, $"{timestamp}.{payload}");
        var header = $"t={timestamp},v1={signature}";
        var verifier = new StripeWebhookVerifier();

        var result = verifier.IsValid(payload, header, secret, out var failureReason);

        result.Should().BeTrue();
        failureReason.Should().BeNull();
    }

    [Fact]
    public void StripeWebhookVerifier_ShouldRejectInvalidSignature()
    {
        var verifier = new StripeWebhookVerifier();

        var result = verifier.IsValid("{}", "t=1713457800,v1=deadbeef", "whsec_test_secret", out var failureReason);

        result.Should().BeFalse();
        failureReason.Should().NotBeNull();
    }

    private static string ComputeSignature(string secret, string signedPayload)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        return Convert.ToHexString(hmac.ComputeHash(Encoding.UTF8.GetBytes(signedPayload))).ToLowerInvariant();
    }
}
