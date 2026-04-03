using BillingService.Api.Contracts;
using BillingService.Application.Commands.GenerateInvoice;
using BillingService.Application.Commands.ProcessPayment;
using BillingService.Application.Queries.GetInvoiceById;
using BillingService.Application.Queries.ListInvoicesByTenant;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BillingService.Api.Controllers;

[ApiController]
[Route("billing/invoices")]
public sealed class InvoicesController(IMediator mediator) : ControllerBase
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
    public async Task<IActionResult> List([FromQuery] Guid tenantId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? status = null, CancellationToken cancellationToken = default)
    {
        var items = await mediator.Send(new ListInvoicesByTenantQuery(tenantId, page, pageSize, status), cancellationToken);
        return Ok(items);
    }

    [HttpPost("{id:guid}/pay")]
    public async Task<IActionResult> Pay(Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ProcessPaymentCommand(id), cancellationToken);
        return Ok(new { result });
    }
}
