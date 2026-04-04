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
public sealed class TemplatesController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTemplateRequest request, CancellationToken cancellationToken)
    {
        var id = await mediator.Send(new CreateTemplateCommand(request.TenantId, request.Name, request.Subject, request.BodyTemplate, request.Channel, request.Description), cancellationToken);
        return Created($"/communication/templates/{id}", new { id });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var model = await mediator.Send(new GetTemplateByIdQuery(id), cancellationToken);
        return model is null ? NotFound() : Ok(model);
    }

    [HttpGet("tenant/{tenantId:guid}")]
    public async Task<IActionResult> ListByTenant(Guid tenantId, CancellationToken cancellationToken)
    {
        var models = await mediator.Send(new ListTemplatesByTenantQuery(tenantId), cancellationToken);
        return Ok(models);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTemplateRequest request, CancellationToken cancellationToken)
    {
        await mediator.Send(new UpdateTemplateCommand(id, request.Name, request.Subject, request.BodyTemplate, request.Description, request.Action), cancellationToken);
        return NoContent();
    }
}
