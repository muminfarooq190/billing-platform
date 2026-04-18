using GeoLeadsService.Api.Contracts;
using GeoLeadsService.Api;
using GeoLeadsService.Application.Abstractions;
using GeoLeadsService.Application.Commands.SubmitGeoAreaQuery;
using GeoLeadsService.Application.Queries.GetGeoAreaQueryById;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace GeoLeadsService.Api.Controllers;

[ApiController]
[Route("geo-leads/queries")]
public sealed class GeoLeadsController(IMediator mediator, ITenantContext tenantContext) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Submit([FromBody] SubmitGeoAreaQueryRequest request, CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId == Guid.Empty)
            return Unauthorized(new { error = "x-tenant-id header is required." });

        if (!string.Equals(request.Geometry.Type, "Polygon", StringComparison.OrdinalIgnoreCase))
            return BadRequest(new { error = "Only Polygon geometry is supported in the initial MVP implementation." });

        if (request.Geometry.Coordinates is null || request.Geometry.Coordinates.Count < 3)
            return BadRequest(new { error = "Polygon must contain at least three coordinate pairs." });

        var polygon = new GeoPolygon(request.Geometry.Coordinates.Select(x => new GeoCoordinate(x[0], x[1])).ToList());
        var (queryId, count) = await mediator.Send(new SubmitGeoAreaQueryCommand(
            tenantContext.TenantId,
            polygon,
            request.LeadTypes?.ToList() ?? [],
            Math.Clamp(request.Limit ?? 50, 1, 500),
            request.RankingMode), cancellationToken);

        return Ok(new GeoAreaQueryResponse(queryId, "Completed", count));
    }

    [HttpGet("{queryId:guid}")]
    public async Task<IActionResult> Get(Guid queryId, CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId == Guid.Empty)
            return Unauthorized(new { error = "x-tenant-id header is required." });

        var query = await mediator.Send(new GetGeoAreaQueryByIdQuery(tenantContext.TenantId, queryId), cancellationToken);
        if (query is null)
            return NotFound();

        return Ok(new
        {
            queryId = query.Id,
            status = query.Status.ToString(),
            count = query.Results.Count,
            results = query.Results.Select(x => new
            {
                leadId = x.GeoLeadId,
                rank = x.Rank,
                score = x.Score,
                name = x.Lead.CanonicalName,
                leadType = x.Lead.LeadType,
                email = x.Lead.PrimaryEmail,
                phone = x.Lead.PrimaryPhone,
                website = x.Lead.Website,
                address = x.Lead.Address,
                city = x.Lead.City,
                region = x.Lead.Region,
                country = x.Lead.Country,
                location = new { lat = x.Lead.Latitude, lng = x.Lead.Longitude },
                sources = x.Lead.Sources,
                reason = x.Reasoning
            })
        });
    }

    [HttpGet("{queryId:guid}/export")]
    public async Task<IActionResult> Export(Guid queryId, [FromQuery] string format = "csv", CancellationToken cancellationToken = default)
    {
        if (!string.Equals(format, "csv", StringComparison.OrdinalIgnoreCase))
            return BadRequest(new { error = "Only CSV export is currently supported." });

        var query = await mediator.Send(new GetGeoAreaQueryByIdQuery(tenantContext.TenantId, queryId), cancellationToken);
        if (query is null)
            return NotFound();

        var lines = new List<string> { "Name,LeadType,Email,Phone,Website,Address,City,Region,Country,Score" };
        lines.AddRange(query.Results.Select(x => string.Join(',', Escape(x.Lead.CanonicalName), Escape(x.Lead.LeadType), Escape(x.Lead.PrimaryEmail), Escape(x.Lead.PrimaryPhone), Escape(x.Lead.Website), Escape(x.Lead.Address), Escape(x.Lead.City), Escape(x.Lead.Region), Escape(x.Lead.Country), x.Score.ToString("0.####"))));
        return File(System.Text.Encoding.UTF8.GetBytes(string.Join(Environment.NewLine, lines)), "text/csv", $"geo-leads-{queryId:D}.csv");
    }

    private static string Escape(string? value) => $"\"{(value ?? string.Empty).Replace("\"", "\"\"")}\"";
}
