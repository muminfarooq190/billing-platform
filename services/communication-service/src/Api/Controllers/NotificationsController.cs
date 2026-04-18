using System.Text.Json;
using CommunicationService.Api.Contracts;
using CommunicationService.Application.Commands.MarkNotificationRead;
using CommunicationService.Application.Commands.ReplayNotification;
using CommunicationService.Application.Commands.SendNotification;
using CommunicationService.Application.Commands.SendWorkflowNotification;
using CommunicationService.Application.Queries.GetNotificationDetail;
using CommunicationService.Application.Queries.GetUnreadNotificationCount;
using CommunicationService.Application.Queries.ListNotifications;
using CommunicationService.Application.Queries.ListNotificationsByRecipient;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CommunicationService.Api.Controllers;

[ApiController]
[Route("communication/notifications")]
public sealed class NotificationsController(IMediator mediator, ITenantContext tenantContext) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Send([FromBody] SendNotificationRequest request, CancellationToken cancellationToken)
    {
        var id = await mediator.Send(new SendNotificationCommand(
            tenantContext.TenantId,
            request.RecipientId,
            request.RecipientType,
            request.Channel,
            request.TemplateName,
            request.Subject,
            request.Body,
            request.Priority,
            request.ReferenceId,
            request.CorrelationId,
            request.IdempotencyKey,
            null,
            SerializeDocuments(request.Documents),
            SerializeMetadata(request.Metadata),
            request.Placeholders), cancellationToken);

        return Accepted($"/communication/notifications/{id}", new { id });
    }

    [HttpPost("workflows/{workflowType}")]
    public async Task<IActionResult> SendWorkflow(string workflowType, [FromBody] SendWorkflowNotificationRequest request, CancellationToken cancellationToken)
    {
        var id = await mediator.Send(new SendWorkflowNotificationCommand(
            tenantContext.TenantId,
            workflowType,
            request.RecipientId,
            request.RecipientType,
            request.Channel,
            request.TemplateName,
            request.Subject,
            request.Body,
            request.Priority,
            request.ReferenceId,
            request.CorrelationId,
            request.IdempotencyKey,
            SerializeDocuments(request.Documents),
            SerializeMetadata(request.Metadata),
            request.Placeholders), cancellationToken);

        return Accepted($"/communication/notifications/{id}", new { id, workflowType });
    }

    [HttpGet]
    public async Task<IActionResult> List([FromQuery] string? status, [FromQuery] string? channel, [FromQuery] string? referenceId, [FromQuery] string? correlationId, [FromQuery] string? workflowType, [FromQuery] Guid? recipientId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var models = await mediator.Send(new ListNotificationsQuery(tenantContext.TenantId, status, channel, referenceId, correlationId, workflowType, recipientId, page, pageSize), cancellationToken);
        return Ok(models);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var model = await mediator.Send(new GetNotificationDetailQuery(tenantContext.TenantId, id), cancellationToken);
        return model is null ? NotFound() : Ok(model);
    }

    [HttpGet("recipient/{recipientId:guid}")]
    public async Task<IActionResult> ListByRecipient(Guid recipientId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var models = await mediator.Send(new ListNotificationsByRecipientQuery(tenantContext.TenantId, recipientId, page, pageSize), cancellationToken);
        return Ok(models);
    }

    [HttpGet("recipient/{recipientId:guid}/unread-count")]
    public async Task<IActionResult> GetUnreadCount(Guid recipientId, CancellationToken cancellationToken)
    {
        var count = await mediator.Send(new GetUnreadNotificationCountQuery(tenantContext.TenantId, recipientId), cancellationToken);
        return Ok(new { count });
    }

    [HttpPatch("{id:guid}/read")]
    public async Task<IActionResult> MarkAsRead(Guid id, CancellationToken cancellationToken)
    {
        await mediator.Send(new MarkNotificationReadCommand(id), cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/replay")]
    public async Task<IActionResult> Replay(Guid id, [FromBody] ReplayNotificationRequest? request, CancellationToken cancellationToken)
    {
        await mediator.Send(new ReplayNotificationCommand(tenantContext.TenantId, id, request?.Reason), cancellationToken);
        return Accepted($"/communication/notifications/{id}", new { id, action = "replayed" });
    }

    private static string SerializeDocuments(List<DocumentReferenceRequest>? documents)
        => JsonSerializer.Serialize(documents ?? []);

    private static string SerializeMetadata(Dictionary<string, string>? metadata)
        => JsonSerializer.Serialize(metadata ?? new Dictionary<string, string>());
}
