using TravelService.Api.Contracts;
using TravelService.Application.Commands.AcceptQuotation;
using TravelService.Application.Commands.ConvertQuotationToItinerary;
using TravelService.Application.Commands.CreateQuotation;
using TravelService.Application.Commands.CreateQuotationRevision;
using TravelService.Application.Commands.ExpireQuotation;
using TravelService.Application.Commands.RejectQuotation;
using TravelService.Application.Commands.UpdateQuotation;
using TravelService.Application.Queries.GetQuotationById;
using TravelService.Application.Queries.GetQuotationHistory;
using TravelService.Application.Queries.GetQuotationRevisionById;
using TravelService.Application.Queries.ListQuotationRevisions;
using TravelService.Application.Queries.ListQuotationsByTenant;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace TravelService.Api.Controllers;

[ApiController]
[Route("travel/quotations")]
public sealed class QuotationsController(IMediator mediator, ITenantContext tenantContext) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateQuotationRequest request, CancellationToken cancellationToken)
    {
        var lineItems = request.LineItems.Select(x => new LineItemDto(x.Description, x.UnitPrice, x.Quantity, x.Currency)).ToList();
        var id = await mediator.Send(new CreateQuotationCommand(
            tenantContext.TenantId, request.CustomerContactId, request.CustomerName,
            request.Title, request.Destination, request.TravelDate, request.ReturnDate,
            request.Travellers, request.Currency, request.Notes, lineItems), cancellationToken);
        return Created($"/travel/quotations/{id}", new { id });
    }

    [HttpPost("{id:guid}/revisions")]
    public async Task<IActionResult> CreateRevision(Guid id, [FromBody] CreateQuotationRevisionRequest request, CancellationToken cancellationToken)
    {
        var lineItems = request.LineItems.Select(x => new QuotationRevisionLineItemDto(x.Description, x.UnitPrice, x.Quantity, x.Currency)).ToList();
        var result = await mediator.Send(new CreateQuotationRevisionCommand(
            tenantContext.TenantId,
            id,
            request.Title,
            request.Destination,
            request.TravelDate,
            request.ReturnDate,
            request.Travellers,
            request.Currency,
            request.VisibleNotes,
            request.InternalNotes,
            request.ValidUntil,
            lineItems), cancellationToken);

        return Created($"/travel/quotations/{id}/revisions/{result.RevisionId}", result);
    }

    [HttpPost("{id:guid}/accept")]
    public async Task<IActionResult> Accept(Guid id, [FromBody] AcceptQuotationRequest request, CancellationToken cancellationToken)
    {
        await mediator.Send(new AcceptQuotationCommand(tenantContext.TenantId, id, request.RevisionId, request.Reason), cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/reject")]
    public async Task<IActionResult> Reject(Guid id, [FromBody] RejectQuotationRequest request, CancellationToken cancellationToken)
    {
        await mediator.Send(new RejectQuotationCommand(tenantContext.TenantId, id, request.Reason), cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/expire")]
    public async Task<IActionResult> Expire(Guid id, [FromBody] ExpireQuotationRequest request, CancellationToken cancellationToken)
    {
        await mediator.Send(new ExpireQuotationCommand(tenantContext.TenantId, id, request.Reason), cancellationToken);
        return NoContent();
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var model = await mediator.Send(new GetQuotationByIdQuery(id), cancellationToken);
        return model is null ? NotFound() : Ok(model);
    }

    [HttpGet("{id:guid}/history")]
    public async Task<IActionResult> GetHistory(Guid id, CancellationToken cancellationToken)
    {
        var model = await mediator.Send(new GetQuotationHistoryQuery(tenantContext.TenantId, id), cancellationToken);
        return Ok(model);
    }

    [HttpGet("{id:guid}/revisions")]
    public async Task<IActionResult> ListRevisions(Guid id, CancellationToken cancellationToken)
    {
        var models = await mediator.Send(new ListQuotationRevisionsQuery(tenantContext.TenantId, id), cancellationToken);
        return Ok(models);
    }

    [HttpGet("{id:guid}/revisions/{revisionId:guid}")]
    public async Task<IActionResult> GetRevisionById(Guid id, Guid revisionId, CancellationToken cancellationToken)
    {
        var model = await mediator.Send(new GetQuotationRevisionByIdQuery(tenantContext.TenantId, id, revisionId), cancellationToken);
        return model is null ? NotFound() : Ok(model);
    }

    [HttpGet]
    public async Task<IActionResult> ListByTenant(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? status = null,
        [FromQuery] string? customerName = null,
        [FromQuery] DateTimeOffset? travelDateFrom = null,
        [FromQuery] DateTimeOffset? travelDateTo = null,
        CancellationToken cancellationToken = default)
    {
        var models = await mediator.Send(new ListQuotationsByTenantQuery(tenantContext.TenantId, page, pageSize, status, customerName, travelDateFrom, travelDateTo), cancellationToken);
        return Ok(models);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateQuotationRequest request, CancellationToken cancellationToken)
    {
        await mediator.Send(new UpdateQuotationCommand(id, request.Title, request.Destination, request.TravelDate, request.ReturnDate, request.Travellers, request.Currency, request.Notes, request.ValidUntil, request.Action), cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/convert")]
    public async Task<IActionResult> ConvertToItinerary(Guid id, CancellationToken cancellationToken)
    {
        var itineraryId = await mediator.Send(new ConvertQuotationToItineraryCommand(id), cancellationToken);
        return Created($"/travel/itineraries/{itineraryId}", new { itineraryId });
    }
}
