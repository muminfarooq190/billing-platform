using Microsoft.EntityFrameworkCore;
using TravelService.Domain.Aggregates;
using TravelService.Domain.Enums;
using TravelService.Infrastructure.Persistence;

namespace TravelService.Infrastructure.FollowUps;

/// <summary>
/// Polls quotations every 5 min and generates follow-up rows when an actor should
/// chase a customer. Three triggers (v1):
///
///   1. <c>quote_sent_2h_no_reply</c> — Sent > 2h ago, no reply, no recent follow-up
///   2. <c>quote_expiring_today</c>   — Sent, validUntil between now and now+24h
///   3. <c>quote_viewed_no_reply</c>  — last_viewed_at > 1h ago, no recent follow-up
///
/// De-duplication: each follow-up's Subject is prefixed with
/// <c>AUTO[{quotationId}|{trigger}]</c>. Generator skips creation if a non-completed
/// row with that prefix exists within the last 24h.
/// </summary>
public sealed class FollowUpAutoGenerator(
    IServiceScopeFactory scopeFactory,
    ILogger<FollowUpAutoGenerator> logger) : BackgroundService
{
    private static readonly TimeSpan PollInterval = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan DedupWindow = TimeSpan.FromHours(24);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Stagger startup so it doesn't race with seeding.
        try { await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken); } catch (TaskCanceledException) { return; }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await RunOnceAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not TaskCanceledException)
            {
                logger.LogError(ex, "FollowUpAutoGenerator iteration failed");
            }

            try { await Task.Delay(PollInterval, stoppingToken); } catch (TaskCanceledException) { return; }
        }
    }

    private async Task RunOnceAsync(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TravelDbContext>();
        var now = DateTimeOffset.UtcNow;
        var dedupSince = now - DedupWindow;

        // Pre-fetch recent auto follow-ups for fast in-memory dedup.
        var recentAuto = await db.Set<FollowUp>().AsNoTracking()
            .Where(x => x.CreatedAt >= dedupSince && x.Subject.StartsWith("AUTO["))
            .Select(x => new { x.TenantId, x.Subject, x.Status })
            .ToListAsync(cancellationToken);
        bool AlreadyExists(Guid tenantId, string key)
            => recentAuto.Any(r => r.TenantId == tenantId
                && r.Subject.StartsWith($"AUTO[{key}]")
                && r.Status != FollowUpStatus.Completed
                && r.Status != FollowUpStatus.Cancelled);

        // ── Trigger 1: quote sent 2h ago, no follow-up yet ────────────────
        var sentCutoff = now.AddHours(-2);
        var sentQuotes = await db.Quotations.AsNoTracking()
            .Where(q => q.LastSentAt != null
                && q.LastSentAt <= sentCutoff
                && q.Status == QuotationStatus.Sent)
            .Select(q => new { q.Id, q.TenantId, q.CustomerContactId, q.CustomerName, q.Title, q.LastSentAt, q.ValidUntil })
            .Take(100)
            .ToListAsync(cancellationToken);

        var created = 0;
        foreach (var q in sentQuotes)
        {
            var key = $"{q.Id}|quote_sent_2h_no_reply";
            if (AlreadyExists(q.TenantId, key)) continue;
            var follow = FollowUp.Create(
                tenantId: q.TenantId,
                customerContactId: q.CustomerContactId,
                customerName: q.CustomerName,
                subject: $"AUTO[{key}] Nudge {q.CustomerName} on \"{q.Title}\"",
                notes: $"Quote was sent at {q.LastSentAt:u}. Auto-suggested follow-up — customer hasn't replied in 2h.",
                priority: FollowUpPriority.Medium,
                dueDate: now.AddMinutes(15),
                assignedToUserId: null);
            db.Set<FollowUp>().Add(follow);
            created++;
        }

        // ── Trigger 2: quote expiring within next 24h ─────────────────────
        var expiryCutoff = now.AddHours(24);
        var expiringQuotes = await db.Quotations.AsNoTracking()
            .Where(q => q.Status == QuotationStatus.Sent
                && q.ValidUntil >= now
                && q.ValidUntil <= expiryCutoff)
            .Select(q => new { q.Id, q.TenantId, q.CustomerContactId, q.CustomerName, q.Title, q.ValidUntil })
            .Take(100)
            .ToListAsync(cancellationToken);

        foreach (var q in expiringQuotes)
        {
            var key = $"{q.Id}|quote_expiring_today";
            if (AlreadyExists(q.TenantId, key)) continue;
            var follow = FollowUp.Create(
                tenantId: q.TenantId,
                customerContactId: q.CustomerContactId,
                customerName: q.CustomerName,
                subject: $"AUTO[{key}] Quote expiring — \"{q.Title}\"",
                notes: $"Quotation validity ends at {q.ValidUntil:u}. Push the customer to decide today.",
                priority: FollowUpPriority.High,
                dueDate: now.AddMinutes(15),
                assignedToUserId: null);
            db.Set<FollowUp>().Add(follow);
            created++;
        }

        // ── Trigger 3: quote viewed > 1h ago, no reply, no follow-up ──────
        var viewedCutoff = now.AddHours(-1);
        var viewedQuotes = await db.Quotations.AsNoTracking()
            .Where(q => q.LastViewedAt != null
                && q.LastViewedAt <= viewedCutoff
                && q.Status == QuotationStatus.Sent)
            .Select(q => new { q.Id, q.TenantId, q.CustomerContactId, q.CustomerName, q.Title, q.LastViewedAt })
            .Take(100)
            .ToListAsync(cancellationToken);

        foreach (var q in viewedQuotes)
        {
            var key = $"{q.Id}|quote_viewed_no_reply";
            if (AlreadyExists(q.TenantId, key)) continue;
            var follow = FollowUp.Create(
                tenantId: q.TenantId,
                customerContactId: q.CustomerContactId,
                customerName: q.CustomerName,
                subject: $"AUTO[{key}] {q.CustomerName} viewed quote — ask if questions",
                notes: $"Quote was viewed at {q.LastViewedAt:u}. Customer is engaged but silent — perfect time to call.",
                priority: FollowUpPriority.High,
                dueDate: now.AddMinutes(15),
                assignedToUserId: null);
            db.Set<FollowUp>().Add(follow);
            created++;
        }

        if (created > 0)
        {
            await db.SaveChangesAsync(cancellationToken);
            logger.LogInformation("FollowUpAutoGenerator created {Count} new follow-ups", created);
        }
    }
}
