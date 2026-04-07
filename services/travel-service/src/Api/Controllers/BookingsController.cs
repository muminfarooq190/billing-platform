using MediatR;
using Microsoft.AspNetCore.Mvc;
using TravelService.Api.Contracts;
using TravelService.Application.Commands.CreateBookingFromQuotation;
using TravelService.Application.Queries.GetBookingById;
using TravelService.Application.Queries.ListBookings;

namespace TravelService.Api.Controllers;

[ApiController]
[Route("travel/bookings")]
public sealed class BookingsController(IMediator mediator, ITenantContext tenantContext) : ControllerBase
{
    [HttpPost("from-quotation/{quotationId:guid}")]
    public async Task<IActionResult> CreateFromQuotation(Guid quotationId, [FromBody] CreateBookingFromQuotationRequest request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new CreateBookingFromQuotationCommand(tenantContext.TenantId, quotationId, request.AssignedToUserId, request.InternalNotes), cancellationToken);
        return Created($"/travel/bookings/{result.BookingId}", result);
    }

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        var bookings = await mediator.Send(new ListBookingsQuery(tenantContext.TenantId), cancellationToken);
        return Ok(bookings);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var booking = await mediator.Send(new GetBookingByIdQuery(tenantContext.TenantId, id), cancellationToken);
        return booking is null ? NotFound() : Ok(booking);
    }
}
