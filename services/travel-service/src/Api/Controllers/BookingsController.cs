using MediatR;
using Microsoft.AspNetCore.Mvc;
using TravelService.Api.Contracts;
using TravelService.Application.Commands.AddBookingItem;
using TravelService.Application.Commands.AddTraveler;
using TravelService.Application.Commands.BookingFulfillment;
using TravelService.Application.Commands.CreateBookingFromQuotation;
using TravelService.Application.Commands.CreateBookingItinerary;
using TravelService.Application.Commands.CreateItinerary;
using TravelService.Application.Commands.DeleteBookingDocument;
using TravelService.Application.Commands.DeleteBookingItem;
using TravelService.Application.Commands.DeleteTraveler;
using TravelService.Application.Commands.UpdateBookingItem;
using TravelService.Application.Commands.UpdateBookingItemStatus;
using TravelService.Application.Commands.UpdateTraveler;
using TravelService.Application.Commands.UploadBookingDocument;
using TravelService.Application.Queries.GetBookingById;
using TravelService.Application.Queries.GetBookingFinancialSummary;
using TravelService.Application.Queries.ListBookingDocuments;
using TravelService.Application.Queries.ListBookingItems;
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
    public async Task<IActionResult> List(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? status = null,
        [FromQuery] string? destination = null,
        [FromQuery] DateTimeOffset? startDateFrom = null,
        [FromQuery] DateTimeOffset? startDateTo = null,
        [FromQuery] Guid? assignedToUserId = null,
        [FromQuery] Guid? primaryContactId = null,
        CancellationToken cancellationToken = default)
    {
        var bookings = await mediator.Send(new ListBookingsQuery(tenantContext.TenantId, page, pageSize, status, destination, startDateFrom, startDateTo, assignedToUserId, primaryContactId), cancellationToken);
        return Ok(bookings);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var booking = await mediator.Send(new GetBookingByIdQuery(tenantContext.TenantId, id), cancellationToken);
        return booking is null ? NotFound() : Ok(booking);
    }

    [HttpGet("{id:guid}/financial-summary")]
    public async Task<IActionResult> GetFinancialSummary(Guid id, CancellationToken cancellationToken)
    {
        var summary = await mediator.Send(new GetBookingFinancialSummaryQuery(tenantContext.TenantId, id), cancellationToken);
        return summary is null ? NotFound() : Ok(summary);
    }

    [HttpPost("{id:guid}/itinerary")]
    public async Task<IActionResult> CreateItinerary(Guid id, [FromBody] CreateBookingItineraryRequest request, CancellationToken cancellationToken)
    {
        var items = request.Items.Select(x => new ItineraryItemDto(x.DayNumber, x.ItemType, x.Title, x.Description, x.Location, x.StartTime, x.EndTime, x.Cost, x.Currency)).ToList();
        var itineraryId = await mediator.Send(new CreateBookingItineraryCommand(
            tenantContext.TenantId,
            id,
            request.Title,
            request.Destination,
            request.StartDate,
            request.EndDate,
            request.Travellers,
            request.Currency,
            items), cancellationToken);

        return Created($"/travel/itineraries/{itineraryId}", new { itineraryId });
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

    [HttpPost("{id:guid}/items")]
    public async Task<IActionResult> AddItem(Guid id, [FromBody] AddBookingItemRequest request, CancellationToken cancellationToken)
    {
        var itemId = await mediator.Send(new AddBookingItemCommand(
            tenantContext.TenantId,
            id,
            request.Type,
            request.Title,
            request.Description,
            request.SupplierName,
            request.SupplierReference,
            request.Location,
            request.StartAt,
            request.EndAt,
            request.SellAmount,
            request.CostAmount,
            request.Currency,
            request.VoucherNumber,
            request.ConfirmationNumber,
            request.AssignedToUserId,
            request.Notes,
            request.SortOrder), cancellationToken);

        return Created($"/travel/bookings/{id}/items/{itemId}", new { itemId });
    }

    [HttpGet("{id:guid}/items")]
    public async Task<IActionResult> ListItems(Guid id, CancellationToken cancellationToken)
    {
        var items = await mediator.Send(new ListBookingItemsQuery(tenantContext.TenantId, id), cancellationToken);
        return Ok(items);
    }

    [HttpPut("{id:guid}/items/{itemId:guid}")]
    public async Task<IActionResult> UpdateItem(Guid id, Guid itemId, [FromBody] UpdateBookingItemRequest request, CancellationToken cancellationToken)
    {
        await mediator.Send(new UpdateBookingItemCommand(
            tenantContext.TenantId,
            id,
            itemId,
            request.Type,
            request.Title,
            request.Description,
            request.SupplierName,
            request.SupplierReference,
            request.Location,
            request.StartAt,
            request.EndAt,
            request.SellAmount,
            request.CostAmount,
            request.Currency,
            request.VoucherNumber,
            request.ConfirmationNumber,
            request.AssignedToUserId,
            request.Notes,
            request.SortOrder), cancellationToken);
        return NoContent();
    }

    [HttpPatch("{id:guid}/items/{itemId:guid}/status")]
    public async Task<IActionResult> UpdateItemStatus(Guid id, Guid itemId, [FromBody] UpdateBookingItemStatusRequest request, CancellationToken cancellationToken)
    {
        await mediator.Send(new UpdateBookingItemStatusCommand(tenantContext.TenantId, id, itemId, request.Status), cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/items/{itemId:guid}/request-confirmation")]
    public async Task<IActionResult> RequestItemConfirmation(Guid id, Guid itemId, [FromBody] RequestBookingItemConfirmationRequest request, CancellationToken cancellationToken)
    {
        await mediator.Send(new RequestBookingItemConfirmationCommand(tenantContext.TenantId, id, itemId, request.ConfirmationDeadline, request.Notes), cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/items/{itemId:guid}/confirm")]
    public async Task<IActionResult> ConfirmItem(Guid id, Guid itemId, [FromBody] ConfirmBookingItemRequest request, CancellationToken cancellationToken)
    {
        await mediator.Send(new ConfirmBookingItemCommand(tenantContext.TenantId, id, itemId, request.ConfirmationNumber, request.ConfirmedAt, request.Notes), cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/items/{itemId:guid}/issue")]
    public async Task<IActionResult> IssueItem(Guid id, Guid itemId, [FromBody] IssueBookingItemRequest request, CancellationToken cancellationToken)
    {
        await mediator.Send(new IssueBookingItemCommand(tenantContext.TenantId, id, itemId, request.VoucherNumber, request.IssuedAt, request.Notes), cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:guid}/items/{itemId:guid}")]
    public async Task<IActionResult> DeleteItem(Guid id, Guid itemId, CancellationToken cancellationToken)
    {
        await mediator.Send(new DeleteBookingItemCommand(tenantContext.TenantId, id, itemId), cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/documents")]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<IActionResult> UploadDocument(Guid id, [FromForm] UploadBookingDocumentRequest request, CancellationToken cancellationToken)
    {
        if (request.File is null || request.File.Length == 0)
            return BadRequest(new { error = "Document file is required." });

        await using var memoryStream = new MemoryStream();
        await request.File.CopyToAsync(memoryStream, cancellationToken);

        var result = await mediator.Send(new UploadBookingDocumentCommand(
            tenantContext.TenantId,
            id,
            request.TravelerId,
            request.File.FileName,
            request.File.ContentType,
            request.File.Length,
            request.DocumentType,
            request.IsCustomerVisible,
            request.Description,
            memoryStream.ToArray()), cancellationToken);

        return Created($"/travel/bookings/{id}/documents/{result.DocumentId}", result);
    }

    [HttpGet("{id:guid}/documents")]
    public async Task<IActionResult> ListDocuments(Guid id, CancellationToken cancellationToken)
    {
        var documents = await mediator.Send(new ListBookingDocumentsQuery(tenantContext.TenantId, id), cancellationToken);
        return Ok(documents);
    }

    [HttpDelete("{id:guid}/documents/{documentId:guid}")]
    public async Task<IActionResult> DeleteDocument(Guid id, Guid documentId, CancellationToken cancellationToken)
    {
        await mediator.Send(new DeleteBookingDocumentCommand(tenantContext.TenantId, id, documentId), cancellationToken);
        return NoContent();
    }
}
