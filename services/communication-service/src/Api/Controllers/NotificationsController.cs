using CommunicationService.Api.Contracts;
using CommunicationService.Application.Commands.MarkNotificationRead;
using CommunicationService.Application.Commands.SendNotification;
using CommunicationService.Application.Queries.GetUnreadNotificationCount;
using CommunicationService.Application.Queries.ListNotificationsByRecipient;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CommunicationService.Api.Controllers;

[ApiController]
[Route("communication/notifications")]
public sealed class NotificationsController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Send([FromBody] SendNotificationRequest request, CancellationToken cancellationToken)
    {
        var id = await mediator.Send(new SendNotificationCommand(
            request.TenantId,
            request.RecipientId,
            request.RecipientType,
            request.Channel,
            request.TemplateName,
            request.Subject,
            request.Body,
            request.Priority,
            request.ReferenceId,
            request.Placeholders), cancellationToken);

        return Accepted($"/communication/notifications/{id}", new { id });
    }

    [HttpGet("recipient/{recipientId:guid}")]
    public async Task<IActionResult> ListByRecipient(Guid recipientId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var models = await mediator.Send(new ListNotificationsByRecipientQuery(recipientId, page, pageSize), cancellationToken);
        return Ok(models);
    }

    [HttpGet("recipient/{recipientId:guid}/unread-count")]
    public async Task<IActionResult> GetUnreadCount(Guid recipientId, CancellationToken cancellationToken)
    {
        var count = await mediator.Send(new GetUnreadNotificationCountQuery(recipientId), cancellationToken);
        return Ok(new { count });
    }

    [HttpPatch("{id:guid}/read")]
    public async Task<IActionResult> MarkAsRead(Guid id, CancellationToken cancellationToken)
    {
        await mediator.Send(new MarkNotificationReadCommand(id), cancellationToken);
        return NoContent();
    }
}
