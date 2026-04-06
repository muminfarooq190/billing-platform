using MediatR;
using Microsoft.AspNetCore.Mvc;
using TravelService.Api.Contracts;
using TravelService.Application.Commands.CreateContact;
using TravelService.Application.Commands.DeleteContact;
using TravelService.Application.Commands.UpdateContact;
using TravelService.Application.Queries.Contacts;

namespace TravelService.Api.Controllers;

[ApiController]
[Route("travel/contacts")]
public sealed class ContactsController(IMediator mediator, ITenantContext tenantContext) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateContactRequest request, CancellationToken cancellationToken)
    {
        var id = await mediator.Send(new CreateContactCommand(
            tenantContext.TenantId,
            request.FirstName,
            request.LastName,
            request.Email,
            request.Phone,
            request.Company,
            request.Notes,
            request.Tags), cancellationToken);

        return Created($"/travel/contacts/{id}", new { id });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var model = await mediator.Send(new GetContactByIdQuery(id), cancellationToken);
        return model is null ? NotFound() : Ok(model);
    }

    [HttpGet]
    public async Task<IActionResult> ListByTenant([FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var models = await mediator.Send(new ListContactsByTenantQuery(tenantContext.TenantId, page, pageSize), cancellationToken);
        return Ok(models);
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string? q, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var models = await mediator.Send(new SearchContactsQuery(tenantContext.TenantId, q, page, pageSize), cancellationToken);
        return Ok(models);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateContactRequest request, CancellationToken cancellationToken)
    {
        await mediator.Send(new UpdateContactCommand(
            id,
            request.FirstName,
            request.LastName,
            request.Email,
            request.Phone,
            request.Company,
            request.Notes,
            request.Tags), cancellationToken);

        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await mediator.Send(new DeleteContactCommand(id), cancellationToken);
        return NoContent();
    }
}
