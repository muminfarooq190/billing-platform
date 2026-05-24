using System.Text.Json;
using CommunicationService.Application.Abstractions;
using CommunicationService.Domain.Enums;
using CommunicationService.Domain.Repositories;
using CommunicationService.Infrastructure.Channels;
using CommunicationService.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace CommunicationService.Api.Controllers;

/// <summary>
/// Twilio inbound message webhook.
///
/// Twilio POSTs here when a customer texts our number. We do two things:
///
///   1. **STOP / UNSUBSCRIBE handling** (required by TCPA + GDPR).
///      Body matching `STOP / UNSUBSCRIBE / CANCEL / QUIT / END /
///      OPTOUT` flips the RecipientPreferences row for the matching
///      channel to Enabled=false. Without this we keep blasting messages
///      and accrue regulatory + carrier penalties.
///
///   2. **Acknowledge & defer threading.** Inbound messages that aren't
///      opt-outs are logged + 204'd. A future iteration will thread them
///      back into the originating inquiry/booking via a cross-service
///      InboundMessageReceivedEvent. Today we don't lose the message —
///      Twilio retains it; we just don't surface it in the portal yet.
///
/// Signature: reuses the SMS/WhatsApp Twilio auth token via the same
/// validator as the status webhook (HMAC-SHA1 over fullUrl + sorted
/// form params + auth token).
/// </summary>
[ApiController]
[Route("communication/webhooks/twilio")]
public sealed class TwilioInboundWebhookController(
    IRecipientPreferencesRepository preferencesRepository,
    IUnitOfWork unitOfWork,
    ITwilioRequestValidator validator,
    IOptions<SmsChannelOptions> smsOptions,
    IOptions<WhatsAppChannelOptions> waOptions,
    IConfiguration configuration,
    IWebHostEnvironment env,
    CommunicationDbContext dbContext,
    ILogger<TwilioInboundWebhookController> logger) : ControllerBase
{
    private static readonly HashSet<string> OptOutKeywords = new(StringComparer.OrdinalIgnoreCase)
    {
        "STOP", "STOPALL", "UNSUBSCRIBE", "END", "QUIT", "CANCEL", "OPTOUT", "OPT-OUT",
    };

    [HttpPost("inbound")]
    [Consumes("application/x-www-form-urlencoded")]
    public async Task<IActionResult> Inbound([FromForm] IFormCollection form, CancellationToken cancellationToken)
    {
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
                logger.LogError("Twilio inbound webhook hit but TWILIO_AUTH_TOKEN not configured in {Env}.", env.EnvironmentName);
                return StatusCode(503, new { error = "Twilio auth token not configured." });
            }
            logger.LogWarning("TWILIO_AUTH_TOKEN missing — accepting webhook unverified (Development only).");
        }
        else
        {
            var fullUrl = ReconstructPublicUrl();
            var parameters = form.ToDictionary(k => k.Key, v => v.Value.ToString());
            var signature = Request.Headers["X-Twilio-Signature"].ToString();
            if (!authTokens.Any(t => validator.IsValid(t!, fullUrl, parameters, signature)))
            {
                logger.LogWarning("Twilio signature mismatch on inbound webhook. Url={Url}", fullUrl);
                return Unauthorized(new { error = "Invalid Twilio signature." });
            }
        }

        var from = form["From"].ToString();
        var body = form["Body"].ToString().Trim();
        var channel = form["To"].ToString().StartsWith("whatsapp:", StringComparison.OrdinalIgnoreCase)
            ? ChannelType.WhatsApp
            : ChannelType.Sms;

        // Twilio prefixes WhatsApp numbers with `whatsapp:` — strip so we
        // match on the underlying E.164 number stored in RecipientPreferences.
        var normalizedFrom = from.StartsWith("whatsapp:", StringComparison.OrdinalIgnoreCase) ? from[9..] : from;

        if (string.IsNullOrWhiteSpace(normalizedFrom) || string.IsNullOrWhiteSpace(body))
        {
            return Ok(new { acknowledged = true, ignored = true });
        }

        // -- STOP keyword detection ------------------------------------------
        // Twilio actually blocks messages on their side once STOP is seen,
        // but we still need to sync our preferences so the dispatcher
        // doesn't keep queueing sends that will 21610 out.
        var firstToken = body.Split(new[] { ' ', '\n', '\t' }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? string.Empty;
        if (OptOutKeywords.Contains(firstToken))
        {
            var rows = await preferencesRepository.ListByPhoneAsync(normalizedFrom, cancellationToken);
            var changed = 0;
            foreach (var row in rows)
            {
                if (row.OptOutChannel(channel))
                {
                    await preferencesRepository.UpdateAsync(row, cancellationToken);
                    changed++;
                }
            }
            if (changed > 0)
            {
                await unitOfWork.SaveChangesAsync(cancellationToken);
            }
            logger.LogInformation("Twilio STOP keyword from {From} on {Channel}: opted out {Count} preference row(s).", normalizedFrom, channel, changed);
            return Ok(new { acknowledged = true, optedOut = true, channelsAffected = changed });
        }

        // -- Emit InboundMessageReceived via outbox for downstream threading --
        // Travel-service consumes this event, matches the From number against
        // contacts.phone, and appends a note to the most recent active
        // inquiry/booking. Comm-service stays agnostic of that thread logic.
        var providerMessageId = form["MessageSid"].ToString();
        var to = form["To"].ToString();
        var payload = JsonSerializer.Serialize(new
        {
            from = normalizedFrom,
            to,
            body,
            channel = channel.ToString(),
            providerMessageId,
            occurredAt = DateTimeOffset.UtcNow,
        });
        dbContext.DomainEvents.Add(new OutboxMessage
        {
            Id = Guid.NewGuid(),
            AggregateType = "inbound",
            AggregateId = Guid.NewGuid(),
            EventType = "received",
            Payload = payload,
            CreatedAt = DateTimeOffset.UtcNow,
        });
        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Inbound {Channel} from {From} → queued for threading: {Body}", channel, normalizedFrom, body);
        return Ok(new { acknowledged = true, threaded = true });
    }

    private string ReconstructPublicUrl()
    {
        var proto = Request.Headers["X-Forwarded-Proto"].ToString();
        var host = Request.Headers["X-Forwarded-Host"].ToString();
        if (!string.IsNullOrWhiteSpace(proto) && !string.IsNullOrWhiteSpace(host))
        {
            return $"{proto}://{host}{Request.PathBase}{Request.Path}{Request.QueryString}";
        }
        return $"{Request.Scheme}://{Request.Host}{Request.PathBase}{Request.Path}{Request.QueryString}";
    }
}
