using BillingService.Application.Queries.ListInvoicesByTenant;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BillingService.Api.Controllers;

[ApiController]
[Route("billing/invoices/tenant/{tenantId:guid}")]
public sealed class TenantInvoiceReadController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> ListByTenant(Guid tenantId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? status = null, CancellationToken cancellationToken = default)
    {
        var items = await mediator.Send(new ListInvoicesByTenantQuery(tenantId, page, pageSize, status), cancellationToken);
        return Ok(items);
    }
}
