using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TravelService.Application.Abstractions;
using TravelService.Application.Queries.GetPublicQuotationByToken;

namespace TravelService.Api.Controllers;

[ApiController]
[AllowAnonymous]
[Route("travel/public/branding")]
public sealed class PublicBrandingController(IMediator mediator, IPublicBrandingClient brandingClient) : ControllerBase
{
    [HttpGet("{token}")]
    public async Task<IActionResult> Get(string token, CancellationToken cancellationToken)
    {
        var quotation = await mediator.Send(new GetPublicQuotationByTokenQuery(token), cancellationToken);
        if (quotation is null)
        {
            return NotFound();
        }

        var branding = await brandingClient.GetByTenantIdAsync(quotation.TenantId, cancellationToken);
        return branding is null ? NotFound() : Ok(branding);
    }
}
