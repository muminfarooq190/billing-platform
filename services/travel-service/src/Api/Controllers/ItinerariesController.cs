using TravelService.Api.Contracts;
using TravelService.Application.Commands.CreateItinerary;
using TravelService.Application.Commands.UpdateItinerary;
using TravelService.Application.Queries.GetItineraryById;
using TravelService.Application.Queries.ListItinerariesByTenant;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace TravelService.Api.Controllers;

[ApiController]
[Route("travel/itineraries")]
public sealed class ItinerariesController(IMediator mediator, ITenantContext tenantContext) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateItineraryRequest request, CancellationToken cancellationToken)
    {
        var items = request.Items.Select(x => new ItineraryItemDto(x.DayNumber, x.ItemType, x.Title, x.Description, x.Location, x.StartTime, x.EndTime, x.Cost, x.Currency)).ToList();
        var id = await mediator.Send(new CreateItineraryCommand(
            tenantContext.TenantId, request.CustomerContactId, request.CustomerName,
            request.Title, request.Destination, request.StartDate, request.EndDate,
            request.Travellers, request.Currency, request.QuotationId, items), cancellationToken);
        return Created($"/travel/itineraries/{id}", new { id });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var model = await mediator.Send(new GetItineraryByIdQuery(id), cancellationToken);
        return model is null ? NotFound() : Ok(model);
    }

    [HttpGet]
    public async Task<IActionResult> ListByTenant(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? status = null,
        [FromQuery] string? customerName = null,
        [FromQuery] DateTimeOffset? startDateFrom = null,
        [FromQuery] DateTimeOffset? startDateTo = null,
        CancellationToken cancellationToken = default)
    {
        var models = await mediator.Send(new ListItinerariesByTenantQuery(tenantContext.TenantId, page, pageSize, status, customerName, startDateFrom, startDateTo), cancellationToken);
        return Ok(models);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateItineraryRequest request, CancellationToken cancellationToken)
    {
        await mediator.Send(new UpdateItineraryCommand(id, request.Title, request.Destination, request.StartDate, request.EndDate, request.Travellers, request.Currency, request.Action), cancellationToken);
        return NoContent();
    }
}
