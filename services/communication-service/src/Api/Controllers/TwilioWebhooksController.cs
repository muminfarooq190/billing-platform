using CommunicationService.Application.Abstractions;
using CommunicationService.Domain.Enums;
using CommunicationService.Domain.Repositories;
using CommunicationService.Infrastructure.Channels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace CommunicationService.Api.Controllers;

/// <summary>
/// Twilio inbound webhooks.
///
/// Currently supports the status callback path (POST .../status). Twilio
/// hits this endpoint each time a Message moves between states (queued →
/// sent → delivered, or → failed/undelivered). We correlate by `MessageSid`
/// → our `Notification.ProviderMessageId` and apply the appropriate
/// aggregate transition.
///
/// Security: validates `X-Twilio-Signature` per Twilio's HMAC-SHA1 spec.
/// In Development the validator is bypassed when the auth token isn't
/// configured (mirrors Stripe webhook posture). In any other environment
/// missing-config returns 503 so we never silently accept spoofed status
/// callbacks in production.
///
/// Inbound message handling (customer reply → reply land in inquiry thread)
/// is intentionally deferred — see audit doc.
/// </summary>
[ApiController]
[Route("communication/webhooks/twilio")]
public sealed class TwilioWebhooksController(
    INotificationRepository notificationRepository,
    IUnitOfWork unitOfWork,
    ITwilioRequestValidator validator,
    IOptions<SmsChannelOptions> smsOptions,
    IOptions<WhatsAppChannelOptions> waOptions,
    IConfiguration configuration,
    IWebHostEnvironment env,
    ILogger<TwilioWebhooksController> logger) : ControllerBase
{
    [HttpPost("status")]
    [Consumes("application/x-www-form-urlencoded")]
    public async Task<IActionResult> Status([FromForm] IFormCollection form, CancellationToken cancellationToken)
    {
        // Twilio always sends auth_token-tied SID. We accept either SMS or
        // WhatsApp tokens — whichever validates. Either matching token is
        // sufficient (a single Twilio sub-account often signs both channels).
        var authTokens = new[]
        {
            smsOptions.Value.TwilioAuthToken,
            waOptions.Value.TwilioAuthToken,
            configuration["TWILIO_AUTH_TOKEN"],
        }
        .Where(t => !string.IsNullOrWhiteSpace(t))
        .Distinct(StringComparer.Ordinal)
        .ToArray();

        if (authTokens.Length == 0)
        {
            if (!env.IsDevelopment())
            {
                logger.LogError("Twilio status callback received but TWILIO_AUTH_TOKEN not configured in {Env}.", env.EnvironmentName);
                return StatusCode(503, new { error = "Twilio auth token not configured." });
            }
            logger.LogWarning("TWILIO_AUTH_TOKEN missing — accepting webhook unverified (Development only).");
        }
        else
        {
            // Twilio signs against the public URL it called. We must reconstruct
            // it from the original request (X-Forwarded-Proto/Host when behind
            // gateway). Falls back to the actual request URL when unset.
            var fullUrl = ReconstructPublicUrl();
            var parameters = form.ToDictionary(k => k.Key, v => v.Value.ToString());
            var signature = Request.Headers["X-Twilio-Signature"].ToString();
            var anyValid = authTokens.Any(t => validator.IsValid(t!, fullUrl, parameters, signature));
            if (!anyValid)
            {
                logger.LogWarning("Twilio signature mismatch on status callback. Url={Url}", fullUrl);
                return Unauthorized(new { error = "Invalid Twilio signature." });
            }
        }

        var messageSid = form["MessageSid"].ToString();
        if (string.IsNullOrWhiteSpace(messageSid))
            return BadRequest(new { error = "Missing MessageSid." });

        var messageStatus = form["MessageStatus"].ToString();
        if (string.IsNullOrWhiteSpace(messageStatus))
            messageStatus = form["SmsStatus"].ToString(); // SMS callbacks use SmsStatus on legacy API

        var notification = await notificationRepository.GetByProviderMessageIdAsync(messageSid, cancellationToken);
        if (notification is null)
        {
            // Twilio retries 11h on non-2xx. Returning 200 here so we stop the
            // retry loop — the notification may have been purged or originated
            // outside our system (manual Twilio Console send).
            logger.LogInformation("Twilio status callback for unknown MessageSid {Sid} — ack to stop retries.", messageSid);
            return Ok(new { acknowledged = true, known = false });
        }

        try
        {
            ApplyStatusTransition(notification, messageStatus, form);
        }
        catch (Exception ex)
        {
            // Aggregate guards may reject an out-of-order transition (e.g.
            // delivered → sent). Log but return 200 so Twilio doesn't retry.
            logger.LogWarning(ex, "Could not apply Twilio status {Status} to notification {Id}.", messageStatus, notification.Id);
            return Ok(new { acknowledged = true, applied = false });
        }

        await notificationRepository.UpdateAsync(notification, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Ok(new { acknowledged = true, applied = true });
    }

    private static void ApplyStatusTransition(Domain.Aggregates.Notification notification, string messageStatus, IFormCollection form)
    {
        switch (messageStatus?.ToLowerInvariant())
        {
            case "delivered":
            case "read": // WhatsApp delivers a `read` status when recipient opens
                if (notification.Status == NotificationStatus.Sent)
                    notification.MarkDelivered();
                break;

            case "failed":
            case "undelivered":
            {
                var errorCode = form["ErrorCode"].ToString();
                var errorMessage = form["ErrorMessage"].ToString();
                var reason = !string.IsNullOrWhiteSpace(errorMessage)
                    ? $"Twilio {errorCode}: {errorMessage}"
                    : $"Twilio error code {errorCode}".Trim();
                notification.MarkFailed(string.IsNullOrWhiteSpace(reason) ? "Twilio reported delivery failure." : reason);
                break;
            }

            // "queued" / "sending" / "sent" — no state change beyond what we
            // already set on dispatch. Acknowledge silently.
        }
    }

    private string ReconstructPublicUrl()
    {
        // Behind api-gateway, X-Forwarded-* headers carry the original scheme/host.
        // When set, prefer them; otherwise fall back to Request.GetDisplayUrl().
        var proto = Request.Headers["X-Forwarded-Proto"].ToString();
        var host = Request.Headers["X-Forwarded-Host"].ToString();
        if (!string.IsNullOrWhiteSpace(proto) && !string.IsNullOrWhiteSpace(host))
        {
            return $"{proto}://{host}{Request.PathBase}{Request.Path}{Request.QueryString}";
        }
        return $"{Request.Scheme}://{Request.Host}{Request.PathBase}{Request.Path}{Request.QueryString}";
    }
}
