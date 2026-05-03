using MediatR;
using Microsoft.AspNetCore.Mvc;
using TravelService.Api.Auth;
using TravelService.Api.Contracts;
using TravelService.Application.Commands.DraftTripConcepts;
using TravelService.Application.Commands.TravelInquiries;
using TravelService.Application.Queries.DraftTripConcepts;
using TravelService.Application.Queries.TravelInquiries;

namespace TravelService.Api.Controllers;

[ApiController]
[Route("travel/inquiries")]
public sealed class InquiriesController(IMediator mediator, ITenantContext tenantContext) : ControllerBase
{
    [HttpGet]
    [RequirePermission(Permissions.Travel.InquiriesRead)]
    public async Task<IActionResult> List(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? status = null,
        [FromQuery] string? source = null,
        [FromQuery] Guid? assignedToUserId = null,
        [FromQuery] string? destination = null,
        [FromQuery(Name = "q")] string? query = null,
        CancellationToken cancellationToken = default)
    {
        var model = await mediator.Send(new ListTravelInquiriesQuery(tenantContext.TenantId, page, pageSize, status, source, assignedToUserId, destination, query), cancellationToken);
        return Ok(model);
    }

    [HttpGet("{id:guid}")]
    [RequirePermission(Permissions.Travel.InquiriesRead)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var model = await mediator.Send(new GetTravelInquiryByIdQuery(tenantContext.TenantId, id), cancellationToken);
        return model is null ? NotFound() : Ok(model);
    }

    [HttpGet("{id:guid}/history")]
    [RequirePermission(Permissions.Travel.InquiriesRead)]
    public async Task<IActionResult> GetHistory(Guid id, CancellationToken cancellationToken)
    {
        var model = await mediator.Send(new GetTravelInquiryHistoryQuery(tenantContext.TenantId, id), cancellationToken);
        return Ok(model);
    }

    [HttpPost("{id:guid}/assign")]
    [RequirePermission(Permissions.Travel.InquiriesWrite)]
    public async Task<IActionResult> Assign(Guid id, [FromBody] AssignInquiryRequest request, CancellationToken cancellationToken)
    {
        await mediator.Send(new AssignInquiryCommand(tenantContext.TenantId, id, request.AssignedToUserId), cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/qualify")]
    [RequirePermission(Permissions.Travel.InquiriesWrite)]
    public async Task<IActionResult> Qualify(Guid id, [FromBody] InquiryStatusReasonRequest request, CancellationToken cancellationToken)
    {
        await mediator.Send(new QualifyInquiryCommand(tenantContext.TenantId, id, request.Reason), cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/disqualify")]
    [RequirePermission(Permissions.Travel.InquiriesWrite)]
    public async Task<IActionResult> Disqualify(Guid id, [FromQuery] string status, [FromBody] InquiryStatusReasonRequest request, CancellationToken cancellationToken)
    {
        await mediator.Send(new DisqualifyInquiryCommand(tenantContext.TenantId, id, status, request.Reason), cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/mark-contacted")]
    [RequirePermission(Permissions.Travel.InquiriesWrite)]
    public async Task<IActionResult> MarkContacted(Guid id, [FromBody] InquiryStatusReasonRequest request, CancellationToken cancellationToken)
    {
        await mediator.Send(new MarkInquiryContactedCommand(tenantContext.TenantId, id, request.Reason), cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/archive")]
    [RequirePermission(Permissions.Travel.InquiriesWrite)]
    public async Task<IActionResult> Archive(Guid id, [FromBody] InquiryStatusReasonRequest request, CancellationToken cancellationToken)
    {
        await mediator.Send(new ArchiveInquiryCommand(tenantContext.TenantId, id, request.Reason), cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/convert-to-quotation")]
    [RequirePermission(Permissions.Travel.InquiriesWrite)]
    public async Task<IActionResult> ConvertToQuotation(Guid id, [FromBody] ConvertInquiryToQuotationRequest request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ConvertInquiryToQuotationCommand(
            tenantContext.TenantId,
            id,
            request.ContactId,
            request.QuotationTitle,
            request.Currency,
            request.Notes,
            request.AssignedToUserId,
            request.ConceptId,
            request.CreateContactIfMissing), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}/concepts")]
    [RequirePermission(Permissions.Travel.InquiriesRead)]
    public async Task<IActionResult> ListConcepts(Guid id, CancellationToken cancellationToken)
    {
        var concepts = await mediator.Send(new ListDraftTripConceptsByInquiryQuery(tenantContext.TenantId, id), cancellationToken);
        return Ok(concepts);
    }

    [HttpGet("{id:guid}/concepts/{conceptId:guid}")]
    [RequirePermission(Permissions.Travel.InquiriesRead)]
    public async Task<IActionResult> GetConcept(Guid id, Guid conceptId, CancellationToken cancellationToken)
    {
        var concept = await mediator.Send(new GetDraftTripConceptByIdQuery(tenantContext.TenantId, id, conceptId), cancellationToken);
        return concept is null ? NotFound() : Ok(concept);
    }

    [HttpPost("{id:guid}/concepts")]
    [RequirePermission(Permissions.Travel.InquiriesWrite)]
    public async Task<IActionResult> CreateConcept(Guid id, [FromBody] CreateDraftTripConceptRequest request, CancellationToken cancellationToken)
    {
        var days = request.Days?.Select(x => new CreateDraftTripConceptDayDto(x.DayNumber, x.Title, x.Description, x.Location, x.OvernightLocation)).ToList()
                   ?? [];
        var conceptId = await mediator.Send(new CreateDraftTripConceptCommand(
            tenantContext.TenantId,
            id,
            request.Title,
            request.Destination,
            request.Summary,
            request.StartDate,
            request.EndDate,
            request.Travellers,
            request.Currency,
            request.BudgetAmount,
            request.OptionLabel,
            request.Notes,
            tenantContext.UserId,
            days), cancellationToken);
        return Created($"/travel/inquiries/{id}/concepts/{conceptId}", new { conceptId });
    }

    [HttpPost("{id:guid}/concepts/{conceptId:guid}/mark-primary")]
    [RequirePermission(Permissions.Travel.InquiriesWrite)]
    public async Task<IActionResult> MarkPrimaryConcept(Guid id, Guid conceptId, CancellationToken cancellationToken)
    {
        await mediator.Send(new MarkPrimaryDraftTripConceptCommand(tenantContext.TenantId, id, conceptId), cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/concepts/{conceptId:guid}/archive")]
    [RequirePermission(Permissions.Travel.InquiriesWrite)]
    public async Task<IActionResult> ArchiveConcept(Guid id, Guid conceptId, CancellationToken cancellationToken)
    {
        await mediator.Send(new ArchiveDraftTripConceptCommand(tenantContext.TenantId, id, conceptId), cancellationToken);
        return NoContent();
    }
}
