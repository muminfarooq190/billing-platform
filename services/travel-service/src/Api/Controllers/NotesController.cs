using MediatR;
using Microsoft.AspNetCore.Mvc;
using TravelService.Api.Auth;
using TravelService.Api.Contracts;
using TravelService.Application.Commands.EntityNotes;
using TravelService.Application.Queries.EntityNotes;

namespace TravelService.Api.Controllers;

[ApiController]
[Route("travel")]
public sealed class NotesController(IMediator mediator, ITenantContext tenantContext) : ControllerBase
{
    [HttpPost("{entityType}/{entityId:guid}/notes")]
    [RequirePermission(Permissions.Travel.NotesWrite)]
    public async Task<IActionResult> Create(string entityType, Guid entityId, [FromBody] CreateEntityNoteRequest request, CancellationToken cancellationToken)
    {
        var noteId = await mediator.Send(new CreateEntityNoteCommand(tenantContext.TenantId, entityType, entityId, request.Visibility, request.Content), cancellationToken);
        return Created($"/travel/notes/{noteId}", new { noteId });
    }

    [HttpGet("{entityType}/{entityId:guid}/notes")]
    [RequirePermission(Permissions.Travel.NotesRead)]
    public async Task<IActionResult> List(string entityType, Guid entityId, CancellationToken cancellationToken)
    {
        var notes = await mediator.Send(new ListEntityNotesQuery(tenantContext.TenantId, entityType, entityId), cancellationToken);
        return Ok(notes);
    }

    [HttpPut("notes/{noteId:guid}")]
    [RequirePermission(Permissions.Travel.NotesWrite)]
    public async Task<IActionResult> Update(Guid noteId, [FromBody] UpdateEntityNoteRequest request, CancellationToken cancellationToken)
    {
        await mediator.Send(new UpdateEntityNoteCommand(tenantContext.TenantId, noteId, request.Visibility, request.Content), cancellationToken);
        return NoContent();
    }

    [HttpDelete("notes/{noteId:guid}")]
    [RequirePermission(Permissions.Travel.NotesWrite)]
    public async Task<IActionResult> Delete(Guid noteId, CancellationToken cancellationToken)
    {
        await mediator.Send(new DeleteEntityNoteCommand(tenantContext.TenantId, noteId), cancellationToken);
        return NoContent();
    }
}
