using System.Security.Cryptography;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TravelService.Api.Auth;
using TravelService.Domain.Aggregates;
using TravelService.Infrastructure.Persistence;

namespace TravelService.Api.Controllers;

[ApiController]
[Route("travel/quotations/{quotationId:guid}/share-links")]
public sealed class QuotationShareLinksController(TravelDbContext dbContext, ITenantContext tenantContext) : ControllerBase
{
    [HttpGet]
    [RequirePermission(Permissions.Travel.QuotationRead)]
    public async Task<IActionResult> List(Guid quotationId, CancellationToken cancellationToken)
    {
        var tenantId = tenantContext.TenantId;
        await EnsureQuotationOwnedAsync(quotationId, tenantId, cancellationToken);

        var links = await dbContext.QuotationShareLinks.AsNoTracking()
            .Where(x => x.QuotationId == quotationId && x.TenantId == tenantId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        return Ok(links.Select(MapResponse));
    }

    [HttpPost]
    [RequirePermission(Permissions.Travel.QuotationWrite)]
    public async Task<IActionResult> Create(
        Guid quotationId,
        [FromBody] CreateShareLinkRequest? request,
        CancellationToken cancellationToken)
    {
        var tenantId = tenantContext.TenantId;
        await EnsureQuotationOwnedAsync(quotationId, tenantId, cancellationToken);

        Guid revisionId;
        if (request?.RevisionId is { } providedRevisionId)
        {
            var exists = await dbContext.QuotationRevisions.AsNoTracking()
                .AnyAsync(r => r.Id == providedRevisionId && r.QuotationId == quotationId && r.TenantId == tenantId, cancellationToken);
            if (!exists) return BadRequest(new { message = "Revision does not belong to this quotation." });
            revisionId = providedRevisionId;
        }
        else
        {
            var latest = await dbContext.QuotationRevisions.AsNoTracking()
                .Where(r => r.QuotationId == quotationId && r.TenantId == tenantId)
                .OrderByDescending(r => r.RevisionNumber)
                .Select(r => (Guid?)r.Id)
                .FirstOrDefaultAsync(cancellationToken);
            if (latest is null) return BadRequest(new { message = "Quotation has no revisions to share." });
            revisionId = latest.Value;
        }

        var token = GenerateToken();
        var expiresAt = request?.ExpiresAt;
        var link = QuotationShareLink.Create(quotationId, revisionId, tenantId, token, expiresAt);
        dbContext.QuotationShareLinks.Add(link);
        await dbContext.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(nameof(List), new { quotationId }, MapResponse(link));
    }

    [HttpPut("{tokenId:guid}")]
    [RequirePermission(Permissions.Travel.QuotationWrite)]
    public async Task<IActionResult> Update(
        Guid quotationId,
        Guid tokenId,
        [FromBody] UpdateShareLinkRequest? request,
        CancellationToken cancellationToken)
    {
        var tenantId = tenantContext.TenantId;
        var link = await dbContext.QuotationShareLinks
            .FirstOrDefaultAsync(x => x.Id == tokenId && x.QuotationId == quotationId && x.TenantId == tenantId, cancellationToken);
        if (link is null) return NotFound();

        if (request?.Revoke == true && link.RevokedAt is null)
        {
            link.Revoke();
        }

        // Note: ExpiresAt update would need a domain method; skipping here to keep aggregate immutable for now.

        await dbContext.SaveChangesAsync(cancellationToken);
        return Ok(MapResponse(link));
    }

    [HttpDelete("{tokenId:guid}")]
    [RequirePermission(Permissions.Travel.QuotationWrite)]
    public async Task<IActionResult> Revoke(Guid quotationId, Guid tokenId, CancellationToken cancellationToken)
    {
        var tenantId = tenantContext.TenantId;
        var link = await dbContext.QuotationShareLinks
            .FirstOrDefaultAsync(x => x.Id == tokenId && x.QuotationId == quotationId && x.TenantId == tenantId, cancellationToken);
        if (link is null) return NotFound();

        if (link.RevokedAt is null) link.Revoke();
        await dbContext.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    private async Task EnsureQuotationOwnedAsync(Guid quotationId, Guid tenantId, CancellationToken cancellationToken)
    {
        var exists = await dbContext.Quotations.AsNoTracking()
            .AnyAsync(q => q.Id == quotationId && q.TenantId == tenantId, cancellationToken);
        if (!exists) throw new KeyNotFoundException($"Quotation {quotationId} not found for tenant.");
    }

    private static string GenerateToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToBase64String(bytes).Replace('+', '-').Replace('/', '_').TrimEnd('=');
    }

    private static object MapResponse(QuotationShareLink link) => new
    {
        id = link.Id,
        quotationId = link.QuotationId,
        revisionId = link.QuotationRevisionId,
        token = link.Token,
        url = $"/quote/{link.Token}",
        expiresAt = link.ExpiresAt,
        revokedAt = link.RevokedAt,
        lastViewedAt = link.LastViewedAt,
        createdAt = link.CreatedAt,
        active = link.RevokedAt is null && (!link.ExpiresAt.HasValue || link.ExpiresAt.Value >= DateTimeOffset.UtcNow),
    };

    public sealed record CreateShareLinkRequest(Guid? RevisionId, DateTimeOffset? ExpiresAt);
    public sealed record UpdateShareLinkRequest(bool? Revoke);
}
