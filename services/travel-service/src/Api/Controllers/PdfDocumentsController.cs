using MediatR;
using Microsoft.AspNetCore.Mvc;
using TravelService.Api.Documents;
using TravelService.Application.Queries.GetBookingItinerary;
using TravelService.Application.Queries.GetQuotationRevisionById;

namespace TravelService.Api.Controllers;

[ApiController]
[Route("travel/documents")]
public sealed class PdfDocumentsController(IMediator mediator, ITenantContext tenantContext, IPdfDocumentRenderer pdfDocumentRenderer) : ControllerBase
{
    [HttpGet("quotations/{quotationId:guid}/revisions/{revisionId:guid}/pdf")]
    public async Task<IActionResult> GetQuotationRevisionPdf(Guid quotationId, Guid revisionId, CancellationToken cancellationToken)
    {
        var revision = await mediator.Send(new GetQuotationRevisionByIdQuery(tenantContext.TenantId, quotationId, revisionId), cancellationToken);
        if (revision is null)
            return NotFound();

        var bytes = pdfDocumentRenderer.RenderQuotationRevisionPdf(revision);
        return File(bytes, "application/pdf", $"quotation-{revision.RevisionNumber}.pdf");
    }

    [HttpGet("bookings/{bookingId:guid}/itinerary/pdf")]
    public async Task<IActionResult> GetBookingItineraryPdf(Guid bookingId, CancellationToken cancellationToken)
    {
        var itinerary = await mediator.Send(new GetBookingItineraryQuery(tenantContext.TenantId, bookingId), cancellationToken);
        if (itinerary is null)
            return NotFound();

        var bytes = pdfDocumentRenderer.RenderItineraryPdf(itinerary);
        return File(bytes, "application/pdf", $"itinerary-{itinerary.Id:D}.pdf");
    }
}
