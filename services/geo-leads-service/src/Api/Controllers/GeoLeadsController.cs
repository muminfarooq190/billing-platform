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

        if (!request.Geometry.IsValidPolygon(out var geometryError))
            return BadRequest(new { error = geometryError });

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
                name = x.CanonicalName,
                leadType = x.LeadType,
                email = x.PrimaryEmail,
                phone = x.PrimaryPhone,
                website = x.Website,
                address = x.Address,
                city = x.City,
                region = x.Region,
                country = x.Country,
                location = new { lat = x.Latitude, lng = x.Longitude },
                sources = System.Text.Json.JsonSerializer.Deserialize<List<string>>(x.SourcesJson) ?? new List<string>(),
                reason = x.GetReasoning()
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
        lines.AddRange(query.Results.Select(x => string.Join(',', Escape(x.CanonicalName), Escape(x.LeadType), Escape(x.PrimaryEmail), Escape(x.PrimaryPhone), Escape(x.Website), Escape(x.Address), Escape(x.City), Escape(x.Region), Escape(x.Country), x.Score.ToString("0.####"))));
        return File(System.Text.Encoding.UTF8.GetBytes(string.Join(Environment.NewLine, lines)), "text/csv", $"geo-leads-{queryId:D}.csv");
    }

    private static string Escape(string? value) => $"\"{(value ?? string.Empty).Replace("\"", "\"\"")}\"";
}
