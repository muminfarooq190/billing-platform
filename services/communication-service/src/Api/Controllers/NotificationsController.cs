using CommunicationService.Api.Contracts;
using CommunicationService.Application.Commands.SendNotification;
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
    public async Task<IActionResult> ListByRecipient(Guid recipientId, CancellationToken cancellationToken)
    {
        var models = await mediator.Send(new ListNotificationsByRecipientQuery(recipientId), cancellationToken);
        return Ok(models);
    }
}
