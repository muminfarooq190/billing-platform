using System.Text.Json;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using TravelService.Api.Auth;
using TravelService.Api.Documents;
using TravelService.Api.Contracts;
using TravelService.Application.Queries.GetBookingItinerary;
using TravelService.Application.Queries.GetQuotationRevisionById;
using TravelService.Application.Queries.QuotationRevisions;
using TravelService.Application.Queries.TravelTemplates;

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

        var branding = await ResolveBrandingAsync("Quote", revision, cancellationToken);
        var bytes = pdfDocumentRenderer.RenderQuotationRevisionPdf(revision, branding);
        return File(bytes, "application/pdf", $"quotation-{revision.RevisionNumber}.pdf");
    }

    [HttpPost("quotations/preview-pdf")]
    [RequirePermission(Permissions.Travel.DocumentsRead)]
    public async Task<IActionResult> PreviewQuotationPdf([FromBody] PreviewQuotationRequest request, CancellationToken cancellationToken)
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
            InclusionsJson = request.InclusionsJson ?? "[]",
            ExclusionsJson = request.ExclusionsJson ?? "[]",
            PaymentTerms = request.PaymentTerms ?? string.Empty,
            CancellationPolicy = request.CancellationPolicy ?? string.Empty,
        };

        var branding = await ResolveBrandingAsync("Quote", revision, cancellationToken);
        var bytes = pdfDocumentRenderer.RenderQuotationRevisionPdf(revision, branding);
        return File(bytes, "application/pdf", "quotation-preview.pdf");
    }

    [HttpGet("bookings/{bookingId:guid}/itinerary/pdf")]
    [RequirePermission(Permissions.Travel.DocumentsRead)]
    public async Task<IActionResult> GetBookingItineraryPdf(Guid bookingId, CancellationToken cancellationToken)
    {
        var itinerary = await mediator.Send(new GetBookingItineraryQuery(tenantContext.TenantId, bookingId), cancellationToken);
        if (itinerary is null)
            return NotFound();

        var branding = await ResolveBrandingAsync("Itinerary", revision: null, cancellationToken);
        var bytes = pdfDocumentRenderer.RenderItineraryPdf(itinerary, branding);
        return File(bytes, "application/pdf", $"itinerary-{itinerary.Id:D}.pdf");
    }

    /// <summary>
    /// Resolve the active TravelTemplate for the given context and project it
    /// into a <see cref="PdfBranding"/>. Pulls policy/inclusions/exclusions
    /// from the quotation revision when present so the PDF mirrors what the
    /// canvas preview shows.
    /// </summary>
    private async Task<PdfBranding?> ResolveBrandingAsync(string context, QuotationRevisionReadModel? revision, CancellationToken cancellationToken)
    {
        try
        {
            var active = await mediator.Send(new GetActiveTravelTemplateQuery(tenantContext.TenantId, context), cancellationToken);
            TravelTemplateReadModel? template = null;
            if (active?.TemplateId is { } tid)
            {
                template = await mediator.Send(new GetTravelTemplateByIdQuery(tenantContext.TenantId, tid), cancellationToken);
            }

            var sections = (template?.Sections ?? Array.Empty<TravelTemplateSectionReadModel>())
                .Select(s => new PdfBrandingSection(s.Id, s.Label, string.IsNullOrWhiteSpace(s.Hint) ? null : s.Hint))
                .ToList();

            var inclusions = ParseStringList(revision?.InclusionsJson);
            var exclusions = ParseStringList(revision?.ExclusionsJson);

            return new PdfBranding(
                DisplayName: template?.Name ?? "Voyara",
                Tagline: template?.Tagline,
                BannerUrl: template?.Banner,
                AccentColor: template?.AccentColor ?? "#436653",
                PrimaryColor: "#041627",
                FontFamily: "Inter",
                Sections: sections,
                Inclusions: inclusions,
                Exclusions: exclusions,
                PaymentTerms: string.IsNullOrWhiteSpace(revision?.PaymentTerms) ? null : revision!.PaymentTerms,
                CancellationPolicy: string.IsNullOrWhiteSpace(revision?.CancellationPolicy) ? null : revision!.CancellationPolicy);
        }
        catch
        {
            // Branding is best-effort; if the template lookup fails the PDF
            // still renders with neutral defaults rather than 500.
            return null;
        }
    }

    private static IReadOnlyList<string> ParseStringList(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return Array.Empty<string>();
        try
        {
            var parsed = JsonSerializer.Deserialize<List<string>>(json);
            return parsed?.Where(x => !string.IsNullOrWhiteSpace(x)).ToList() ?? new List<string>();
        }
        catch
        {
            return Array.Empty<string>();
        }
    }
}
