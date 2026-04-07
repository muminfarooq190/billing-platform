using MediatR;
using Microsoft.AspNetCore.Mvc;
using TravelService.Api.Contracts;
using TravelService.Application.Commands.AddTraveler;
using TravelService.Application.Commands.CreateBookingFromQuotation;
using TravelService.Application.Commands.DeleteTraveler;
using TravelService.Application.Commands.UpdateTraveler;
using TravelService.Application.Queries.GetBookingById;
using TravelService.Application.Queries.ListBookings;
using TravelService.Application.Queries.ListTravelersByBooking;

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

    [HttpPost("{id:guid}/travelers")]
    public async Task<IActionResult> AddTraveler(Guid id, [FromBody] AddTravelerRequest request, CancellationToken cancellationToken)
    {
        var travelerId = await mediator.Send(new AddTravelerCommand(
            tenantContext.TenantId,
            id,
            request.FirstName,
            request.LastName,
            request.DateOfBirth,
            request.Gender,
            request.Email,
            request.Phone,
            request.PassportNumber,
            request.PassportExpiry,
            request.Nationality,
            request.MealPreference,
            request.SpecialAssistanceNotes,
            request.EmergencyContactName,
            request.EmergencyContactPhone,
            request.LeadTraveler), cancellationToken);

        return Created($"/travel/bookings/{id}/travelers/{travelerId}", new { travelerId });
    }

    [HttpGet("{id:guid}/travelers")]
    public async Task<IActionResult> ListTravelers(Guid id, CancellationToken cancellationToken)
    {
        var travelers = await mediator.Send(new ListTravelersByBookingQuery(tenantContext.TenantId, id), cancellationToken);
        return Ok(travelers);
    }

    [HttpPut("{id:guid}/travelers/{travelerId:guid}")]
    public async Task<IActionResult> UpdateTraveler(Guid id, Guid travelerId, [FromBody] UpdateTravelerRequest request, CancellationToken cancellationToken)
    {
        await mediator.Send(new UpdateTravelerCommand(
            tenantContext.TenantId,
            id,
            travelerId,
            request.FirstName,
            request.LastName,
            request.DateOfBirth,
            request.Gender,
            request.Email,
            request.Phone,
            request.PassportNumber,
            request.PassportExpiry,
            request.Nationality,
            request.MealPreference,
            request.SpecialAssistanceNotes,
            request.EmergencyContactName,
            request.EmergencyContactPhone,
            request.LeadTraveler), cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:guid}/travelers/{travelerId:guid}")]
    public async Task<IActionResult> DeleteTraveler(Guid id, Guid travelerId, CancellationToken cancellationToken)
    {
        await mediator.Send(new DeleteTravelerCommand(tenantContext.TenantId, id, travelerId), cancellationToken);
        return NoContent();
    }
}
