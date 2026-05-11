using MediatR;
using Microsoft.AspNetCore.Mvc;
using TravelService.Api.Auth;
using TravelService.Api.Documents;
using TravelService.Api.Contracts;
using TravelService.Application.Queries.GetBookingItinerary;
using TravelService.Application.Queries.GetQuotationRevisionById;
using TravelService.Application.Queries.QuotationRevisions;

namespace TravelService.Api.Controllers;

[ApiController]
[Route("travel/documents")]
public sealed class PdfDocumentsController(IMediator mediator, ITenantContext tenantContext, IPdfDocumentRenderer pdfDocumentRenderer) : ControllerBase
{
    [HttpGet("quotations/{quotationId:guid}/revisions/{revisionId:guid}/pdf")]
    [RequirePermission(Permissions.Travel.DocumentsRead)]
    public async Task<IActionResult> GetQuotationRevisionPdf(Guid quotationId, Guid revisionId, CancellationToken cancellationToken)
    {
        var revision = await mediator.Send(new GetQuotationRevisionByIdQuery(tenantContext.TenantId, quotationId, revisionId), cancellationToken);
        if (revision is null)
            return NotFound();

        var bytes = pdfDocumentRenderer.RenderQuotationRevisionPdf(revision);
        return File(bytes, "application/pdf", $"quotation-{revision.RevisionNumber}.pdf");
    }

    [HttpPost("quotations/preview-pdf")]
    [RequirePermission(Permissions.Travel.DocumentsRead)]
    public IActionResult PreviewQuotationPdf([FromBody] PreviewQuotationRequest request, CancellationToken _)
    {
        if (request is null) return BadRequest("Body is required.");
        if (string.IsNullOrWhiteSpace(request.Title)) return BadRequest("Title is required.");

        var currency = string.IsNullOrWhiteSpace(request.Currency) ? "USD" : request.Currency.Trim().ToUpperInvariant();
        var lineItems = (request.LineItems ?? new List<PreviewQuotationLineItem>())
            .Select((it, idx) => new QuotationRevisionLineItemReadModel(
                Id: Guid.NewGuid(),
                Description: it.Description ?? string.Empty,
                Quantity: it.Quantity <= 0 ? 1 : it.Quantity,
                UnitPriceAmount: it.UnitPriceAmount,
                Currency: string.IsNullOrWhiteSpace(it.Currency) ? currency : it.Currency!,
                SortOrder: idx,
                LineTotal: it.UnitPriceAmount * (it.Quantity <= 0 ? 1 : it.Quantity)))
            .ToList();

        var subtotal = lineItems.Sum(li => li.LineTotal);
        var tax = request.TaxAmount ?? Math.Round(subtotal * 0.07m, 2);
        var total = request.TotalAmount ?? subtotal + tax;

        var revision = new QuotationRevisionReadModel
        {
            Id = Guid.NewGuid(),
            QuotationId = request.QuotationId ?? Guid.Empty,
            TenantId = tenantContext.TenantId,
            RevisionNumber = 0,
            Status = "Preview",
            CustomerContactId = Guid.Empty,
            CustomerName = request.CustomerName ?? "Sample customer",
            Title = request.Title,
            Destination = request.Destination ?? "Destination TBD",
            TravelDate = request.TravelDate ?? DateTimeOffset.UtcNow.AddDays(30),
            ReturnDate = request.ReturnDate ?? DateTimeOffset.UtcNow.AddDays(37),
            Travellers = request.Travellers <= 0 ? 1 : request.Travellers,
            Currency = currency,
            Notes = request.Notes ?? string.Empty,
            VisibleNotes = request.VisibleNotes ?? request.Notes ?? string.Empty,
            InternalNotes = request.InternalNotes ?? string.Empty,
            ValidUntil = request.ValidUntil ?? DateTimeOffset.UtcNow.AddDays(14),
            SubtotalAmount = subtotal,
            TaxAmount = tax,
            TotalAmount = total,
            CreatedByUserId = tenantContext.UserId,
            CreatedAt = DateTimeOffset.UtcNow,
            LineItems = lineItems,
            Attachments = Array.Empty<QuotationRevisionAttachmentReadModel>(),
        };

        var bytes = pdfDocumentRenderer.RenderQuotationRevisionPdf(revision);
        return File(bytes, "application/pdf", "quotation-preview.pdf");
    }

    [HttpGet("bookings/{bookingId:guid}/itinerary/pdf")]
    [RequirePermission(Permissions.Travel.DocumentsRead)]
    public async Task<IActionResult> GetBookingItineraryPdf(Guid bookingId, CancellationToken cancellationToken)
    {
        var itinerary = await mediator.Send(new GetBookingItineraryQuery(tenantContext.TenantId, bookingId), cancellationToken);
        if (itinerary is null)
            return NotFound();

        var bytes = pdfDocumentRenderer.RenderItineraryPdf(itinerary);
        return File(bytes, "application/pdf", $"itinerary-{itinerary.Id:D}.pdf");
    }
}
