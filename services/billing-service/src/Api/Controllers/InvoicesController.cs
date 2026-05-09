using BillingService.Api.Contracts;
using BillingService.Api.Documents;
using BillingService.Application.Commands.GenerateInvoice;
using BillingService.Application.Commands.ProcessPayment;
using BillingService.Application.Queries.GetInvoiceById;
using BillingService.Application.Queries.ListInvoicesByTenant;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BillingService.Api.Controllers;

[ApiController]
[Route("billing/invoices")]
public sealed class InvoicesController(
    IMediator mediator,
    ITenantContext tenantContext,
    IInvoicePdfRenderer pdfRenderer) : ControllerBase
{
    [HttpPost("generate")]
    public async Task<IActionResult> Generate([FromBody] GenerateInvoiceRequest request, CancellationToken cancellationToken)
    {
        var id = await mediator.Send(new GenerateInvoiceCommand(request.SubscriptionId), cancellationToken);
        return Ok(new { invoiceId = id });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken)
    {
        var invoice = await mediator.Send(new GetInvoiceByIdQuery(id), cancellationToken);
        return invoice is null ? NotFound() : Ok(invoice);
    }

    [HttpGet]
    public async Task<IActionResult> List([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? status = null, CancellationToken cancellationToken = default)
    {
        var items = await mediator.Send(new ListInvoicesByTenantQuery(tenantContext.TenantId, page, pageSize, status), cancellationToken);
        return Ok(items);
    }

    [HttpPost("{id:guid}/pay")]
    public async Task<IActionResult> Pay(Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ProcessPaymentCommand(id), cancellationToken);
        return Ok(new { result });
    }

    [HttpGet("{id:guid}/pdf")]
    public async Task<IActionResult> DownloadPdf(Guid id, CancellationToken cancellationToken)
    {
        var invoice = await mediator.Send(new GetInvoiceByIdQuery(id), cancellationToken);
        if (invoice is null) return NotFound();
        if (invoice.TenantId != tenantContext.TenantId) return Forbid();
        var bytes = pdfRenderer.RenderInvoicePdf(invoice);
        var filename = string.IsNullOrWhiteSpace(invoice.InvoiceNumber)
            ? $"invoice-{invoice.Id:D}.pdf"
            : $"invoice-{invoice.InvoiceNumber}.pdf";
        return File(bytes, "application/pdf", filename);
    }

    [HttpPost("{id:guid}/send")]
    public async Task<IActionResult> SendInvoice(Guid id, CancellationToken cancellationToken)
    {
        var invoice = await mediator.Send(new GetInvoiceByIdQuery(id), cancellationToken);
        if (invoice is null) return NotFound();
        if (invoice.TenantId != tenantContext.TenantId) return Forbid();
        // Communication-service integration deferred — caller can hand off the PDF via existing notification flow.
        // Endpoint reserved + returns 202 with invoice metadata so frontend can wire to UI.
        return Accepted(new
        {
            invoiceId = invoice.Id,
            tenantId = invoice.TenantId,
            status = "queued-for-send",
            message = "Send pipeline not yet implemented. Use download + manual email until communication-service flow lands.",
        });
    }
}
