using System.Text;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using TravelService.Application.Queries.GetBookingItinerary;
using TravelService.Application.Queries.GetItineraryById;
using TravelService.Application.Queries.GetQuotationRevisionById;
using TravelService.Application.Queries.QuotationRevisions;

namespace TravelService.Api.Controllers;

[ApiController]
[Route("travel/documents")]
public sealed class PdfDocumentsController(IMediator mediator, ITenantContext tenantContext) : ControllerBase
{
    [HttpGet("quotations/{quotationId:guid}/revisions/{revisionId:guid}/pdf")]
    public async Task<IActionResult> GetQuotationRevisionPdf(Guid quotationId, Guid revisionId, CancellationToken cancellationToken)
    {
        var revision = await mediator.Send(new GetQuotationRevisionByIdQuery(tenantContext.TenantId, quotationId, revisionId), cancellationToken);
        if (revision is null)
            return NotFound();

        var content = BuildQuotationPdfPayload(revision);
        var bytes = Encoding.UTF8.GetBytes(content);
        return File(bytes, "application/pdf", $"quotation-{revision.RevisionNumber}.pdf");
    }

    [HttpGet("bookings/{bookingId:guid}/itinerary/pdf")]
    public async Task<IActionResult> GetBookingItineraryPdf(Guid bookingId, CancellationToken cancellationToken)
    {
        var itinerary = await mediator.Send(new GetBookingItineraryQuery(tenantContext.TenantId, bookingId), cancellationToken);
        if (itinerary is null)
            return NotFound();

        var content = BuildItineraryPdfPayload(itinerary);
        var bytes = Encoding.UTF8.GetBytes(content);
        return File(bytes, "application/pdf", $"itinerary-{itinerary.Id:D}.pdf");
    }

    private static string BuildQuotationPdfPayload(QuotationRevisionReadModel revision)
    {
        var sb = new StringBuilder();
        sb.AppendLine("VOYARA QUOTATION");
        sb.AppendLine($"Title: {revision.Title}");
        sb.AppendLine($"Customer: {revision.CustomerName}");
        sb.AppendLine($"Destination: {revision.Destination}");
        sb.AppendLine($"Travel Dates: {revision.TravelDate:yyyy-MM-dd} to {revision.ReturnDate:yyyy-MM-dd}");
        sb.AppendLine($"Travellers: {revision.Travellers}");
        sb.AppendLine($"Currency: {revision.Currency}");
        sb.AppendLine($"Valid Until: {revision.ValidUntil:yyyy-MM-dd}");
        sb.AppendLine();
        sb.AppendLine("Line Items:");
        foreach (var item in revision.LineItems)
            sb.AppendLine($"- {item.Description} x{item.Quantity}: {item.Currency} {item.LineTotal:0.##}");
        sb.AppendLine();
        sb.AppendLine($"Subtotal: {revision.Currency} {revision.SubtotalAmount:0.##}");
        sb.AppendLine($"Tax: {revision.Currency} {revision.TaxAmount:0.##}");
        sb.AppendLine($"Total: {revision.Currency} {revision.TotalAmount:0.##}");
        sb.AppendLine();
        if (!string.IsNullOrWhiteSpace(revision.VisibleNotes))
        {
            sb.AppendLine("Notes:");
            sb.AppendLine(revision.VisibleNotes);
        }
        return sb.ToString();
    }

    private static string BuildItineraryPdfPayload(ItineraryReadModel itinerary)
    {
        var sb = new StringBuilder();
        sb.AppendLine("VOYARA ITINERARY");
        sb.AppendLine($"Title: {itinerary.Title}");
        sb.AppendLine($"Customer: {itinerary.CustomerName}");
        sb.AppendLine($"Destination: {itinerary.Destination}");
        sb.AppendLine($"Travel Dates: {itinerary.StartDate:yyyy-MM-dd} to {itinerary.EndDate:yyyy-MM-dd}");
        sb.AppendLine($"Travellers: {itinerary.Travellers}");
        sb.AppendLine($"Currency: {itinerary.Currency}");
        sb.AppendLine($"Status: {itinerary.Status}");
        sb.AppendLine($"Estimated Total Cost: {itinerary.Currency} {itinerary.TotalCost:0.##}");
        return sb.ToString();
    }
}
