using MediatR;
using Microsoft.AspNetCore.Mvc;
using TravelService.Api.Auth;
using TravelService.Api.Contracts;
using TravelService.Application.Commands.TravelTemplates;
using TravelService.Application.Queries.TravelTemplates;

namespace TravelService.Api.Controllers;

[ApiController]
[Route("travel/templates")]
public sealed class TravelTemplatesController(IMediator mediator, ITenantContext tenantContext) : ControllerBase
{
    [HttpGet]
    [RequirePermission(Permissions.Travel.TemplatesRead)]
    public async Task<IActionResult> List([FromQuery] string? context, CancellationToken cancellationToken)
    {
        var templates = await mediator.Send(new ListTravelTemplatesQuery(tenantContext.TenantId, context), cancellationToken);
        return Ok(templates);
    }

    [HttpGet("{id:guid}")]
    [RequirePermission(Permissions.Travel.TemplatesRead)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var template = await mediator.Send(new GetTravelTemplateByIdQuery(tenantContext.TenantId, id), cancellationToken);
        return template is null ? NotFound() : Ok(template);
    }

    [HttpPost]
    [RequirePermission(Permissions.Travel.TemplatesWrite)]
    public async Task<IActionResult> Create([FromBody] CreateTravelTemplateRequest request, CancellationToken cancellationToken)
    {
        var templateId = await mediator.Send(new CreateTravelTemplateCommand(
            tenantContext.TenantId,
            request.Context,
            request.Name,
            request.Description,
            request.Category,
            request.Banner,
            request.AccentColor,
            request.Tagline,
            request.Sections.Select(x => new TravelTemplateSectionCommandModel(x.Id, x.Label, x.Hint)).ToList(),
            new TravelTemplateSeedCommandModel(
                request.Seed.ConceptSeed.Select(x => new TravelTemplateConceptSeedCommandModel(x.Type, x.Content)).ToList(),
                request.Seed.QuoteSeed.Select(x => new TravelTemplateQuoteSeedCommandModel(x.Type, x.Title, x.Description, x.Amount)).ToList(),
                request.Seed.ItineraryDays.Select(x => new TravelTemplateItineraryDaySeedCommandModel(
                    x.Title,
                    x.Items.Select(i => new TravelTemplateItineraryItemSeedCommandModel(i.Type, i.Title, i.Time, i.Notes)).ToList())).ToList()),
            tenantContext.UserId == Guid.Empty ? null : tenantContext.UserId), cancellationToken);

        return Created($"/travel/templates/{templateId}", new { templateId });
    }

    [HttpPut("{id:guid}")]
    [RequirePermission(Permissions.Travel.TemplatesWrite)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTravelTemplateRequest request, CancellationToken cancellationToken)
    {
        await mediator.Send(new UpdateTravelTemplateCommand(
            tenantContext.TenantId,
            id,
            request.Name,
            request.Description,
            request.Category,
            request.Banner,
            request.AccentColor,
            request.Tagline,
            request.Sections.Select(x => new TravelTemplateSectionCommandModel(x.Id, x.Label, x.Hint)).ToList(),
            new TravelTemplateSeedCommandModel(
                request.Seed.ConceptSeed.Select(x => new TravelTemplateConceptSeedCommandModel(x.Type, x.Content)).ToList(),
                request.Seed.QuoteSeed.Select(x => new TravelTemplateQuoteSeedCommandModel(x.Type, x.Title, x.Description, x.Amount)).ToList(),
                request.Seed.ItineraryDays.Select(x => new TravelTemplateItineraryDaySeedCommandModel(
                    x.Title,
                    x.Items.Select(i => new TravelTemplateItineraryItemSeedCommandModel(i.Type, i.Title, i.Time, i.Notes)).ToList())).ToList())), cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [RequirePermission(Permissions.Travel.TemplatesWrite)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await mediator.Send(new DeleteTravelTemplateCommand(tenantContext.TenantId, id), cancellationToken);
        return NoContent();
    }

    [HttpGet("active")]
    [RequirePermission(Permissions.Travel.TemplatesRead)]
    public async Task<IActionResult> GetActive([FromQuery] string context, CancellationToken cancellationToken)
    {
        var active = await mediator.Send(new GetActiveTravelTemplateQuery(tenantContext.TenantId, context), cancellationToken);
        return Ok(active);
    }

    [HttpPut("active")]
    [RequirePermission(Permissions.Travel.TemplatesWrite)]
    public async Task<IActionResult> SetActive([FromBody] SetActiveTravelTemplateRequest request, CancellationToken cancellationToken)
    {
        await mediator.Send(new SetActiveTravelTemplateCommand(tenantContext.TenantId, request.Context, request.TemplateId), cancellationToken);
        return NoContent();
    }
}
