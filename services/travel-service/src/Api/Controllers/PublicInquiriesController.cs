using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TravelService.Api.Contracts;
using TravelService.Application.Commands.TravelInquiries;
using TravelService.Domain.Exceptions;

namespace TravelService.Api.Controllers;

[ApiController]
[Route("travel/public/inquiries")]
public sealed class PublicInquiriesController(IMediator mediator, IPublicTenantResolver publicTenantResolver, IHttpContextAccessor httpContextAccessor) : ControllerBase
{
    [HttpPost]
    [Consumes("application/json")]
    [AllowAnonymous]
    public async Task<IActionResult> Create([FromBody] CreatePublicInquiryRequest request, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(request.Honeypot))
            return BadRequest(new { error = "Invalid inquiry payload." });

        if (string.IsNullOrWhiteSpace(request.FullName))
            throw new DomainException("Full name is required.");
        if (string.IsNullOrWhiteSpace(request.Destination))
            throw new DomainException("Destination is required.");
        if (string.IsNullOrWhiteSpace(request.Email) && string.IsNullOrWhiteSpace(request.Phone) && string.IsNullOrWhiteSpace(request.WhatsappNumber))
            throw new DomainException("At least one contact method is required.");

        var tenantId = publicTenantResolver.ResolveTenantId();
        var httpContext = httpContextAccessor.HttpContext;
        var ipAddress = httpContext?.Connection.RemoteIpAddress?.ToString();
        var userAgent = httpContext?.Request.Headers.UserAgent.FirstOrDefault();

        var inquiryId = await mediator.Send(new CreatePublicInquiryCommand(
            tenantId,
            request.FullName,
            request.Email,
            request.Phone,
            request.WhatsappNumber,
            request.DepartureCity,
            request.Destination,
            request.TravelDate,
            request.ReturnDate,
            request.IsDateFlexible,
            request.Travellers,
            request.BudgetAmount,
            request.BudgetCurrency,
            request.Message,
            string.IsNullOrWhiteSpace(request.Source) ? "Website" : request.Source!,
            ipAddress,
            userAgent), cancellationToken);

        return Created($"/travel/public/inquiries/{inquiryId}", new { inquiryId });
    }
}
