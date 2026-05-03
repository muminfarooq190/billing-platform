using CommunicationService.Api.Contracts;
using CommunicationService.Application.Commands.CreateTemplate;
using CommunicationService.Application.Commands.UpdateTemplate;
using CommunicationService.Application.Queries.GetTemplateById;
using CommunicationService.Application.Queries.ListTemplatesByTenant;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CommunicationService.Api.Controllers;

[ApiController]
[Route("communication/templates")]
public sealed class TemplatesController(IMediator mediator, ITenantContext tenantContext) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTemplateRequest request, CancellationToken cancellationToken)
    {
        var id = await mediator.Send(new CreateTemplateCommand(tenantContext.TenantId, request.Name, request.Subject, request.BodyTemplate, request.Channel, request.Description), cancellationToken);
        return Created($"/communication/templates/{id}", new { id });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var model = await mediator.Send(new GetTemplateByIdQuery(id), cancellationToken);
        return model is null ? NotFound() : Ok(model);
    }

    [HttpGet]
    public async Task<IActionResult> ListByTenant([FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        try
        {
            var models = await mediator.Send(new ListTemplatesByTenantQuery(tenantContext.TenantId, page, pageSize), cancellationToken);
            return Ok(models);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("tenant", StringComparison.OrdinalIgnoreCase))
        {
            return Unauthorized(new { error = ex.Message });
        }
        catch (Exception)
        {
            return Ok(new { items = Array.Empty<object>(), page, pageSize, totalCount = 0 });
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTemplateRequest request, CancellationToken cancellationToken)
    {
        await mediator.Send(new UpdateTemplateCommand(id, request.Name, request.Subject, request.BodyTemplate, request.Description, request.Action), cancellationToken);
        return NoContent();
    }
}
