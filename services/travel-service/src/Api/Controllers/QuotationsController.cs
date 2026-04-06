using TravelService.Api.Contracts;
using TravelService.Application.Commands.CreateQuotation;
using TravelService.Application.Commands.UpdateQuotation;
using TravelService.Application.Commands.ConvertQuotationToItinerary;
using TravelService.Application.Queries.GetQuotationById;
using TravelService.Application.Queries.ListQuotationsByTenant;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace TravelService.Api.Controllers;

[ApiController]
[Route("travel/quotations")]
public sealed class QuotationsController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateQuotationRequest request, CancellationToken cancellationToken)
    {
        var lineItems = request.LineItems.Select(x => new LineItemDto(x.Description, x.UnitPrice, x.Quantity, x.Currency)).ToList();
        var id = await mediator.Send(new CreateQuotationCommand(
            request.TenantId, request.CustomerContactId, request.CustomerName,
            request.Title, request.Destination, request.TravelDate, request.ReturnDate,
            request.Travellers, request.Currency, request.Notes, lineItems), cancellationToken);
        return Created($"/travel/quotations/{id}", new { id });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var model = await mediator.Send(new GetQuotationByIdQuery(id), cancellationToken);
        return model is null ? NotFound() : Ok(model);
    }

    [HttpGet("tenant/{tenantId:guid}")]
    public async Task<IActionResult> ListByTenant(
        Guid tenantId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? status = null,
        [FromQuery] string? customerName = null,
        [FromQuery] DateTimeOffset? travelDateFrom = null,
        [FromQuery] DateTimeOffset? travelDateTo = null,
        CancellationToken cancellationToken = default)
    {
        var models = await mediator.Send(new ListQuotationsByTenantQuery(tenantId, page, pageSize, status, customerName, travelDateFrom, travelDateTo), cancellationToken);
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
