using BillingService.Application.Abstractions;
using BillingService.Domain.Enums;
using BillingService.Domain.Repositories;
using BillingService.Infrastructure.Payments;
using Microsoft.AspNetCore.Mvc;

namespace BillingService.Api.Controllers;

/// <summary>
/// Stripe-native subscription onboarding.
///
/// Flow:
///   1. POST /billing/stripe-subscriptions/checkout
///        { tenantId, subscriptionId, packageId, cycle: "Monthly"|"Annual" }
///   2. Server resolves Stripe Price id from package
///   3. Creates `mode=subscription` Checkout Session
///   4. Returns { checkoutUrl } — caller redirects browser
///   5. After payment, Stripe webhook `customer.subscription.created`
///      lands at /billing/webhooks/stripe → handler links our local
///      Subscription to the Stripe sub id and updates period boundaries
///   6. From there Stripe runs the schedule; our cron skips Stripe-managed
///      rows automatically
/// </summary>
[ApiController]
[Route("billing/stripe-subscriptions")]
public sealed class StripeSubscriptionsController(
    StripeSubscriptionGateway gateway,
    ISubscriptionRepository subscriptionRepository,
    ICommercialPackageRepository packageRepository,
    ILogger<StripeSubscriptionsController> logger) : ControllerBase
{
    public sealed record CheckoutRequest(Guid TenantId, Guid SubscriptionId, Guid PackageId, string Cycle);
    public sealed record CheckoutResponse(string CheckoutUrl, string SessionId, string StripeCustomerId);

    [HttpPost("checkout")]
    public async Task<IActionResult> Checkout([FromBody] CheckoutRequest request, CancellationToken cancellationToken)
    {
        if (request.TenantId == Guid.Empty) return BadRequest(new { error = "TenantId is required." });
        if (request.SubscriptionId == Guid.Empty) return BadRequest(new { error = "SubscriptionId is required." });
        if (request.PackageId == Guid.Empty) return BadRequest(new { error = "PackageId is required." });

        if (!Enum.TryParse<BillingCycle>(request.Cycle, ignoreCase: true, out var cycle))
            return BadRequest(new { error = "Cycle must be 'Monthly' or 'Annual'." });

        var subscription = await subscriptionRepository.GetByIdAsync(request.SubscriptionId, cancellationToken);
        if (subscription is null) return NotFound(new { error = "Subscription not found." });
        if (subscription.TenantId != request.TenantId)
            return BadRequest(new { error = "Subscription does not belong to tenant." });
        if (subscription.IsManagedByStripe)
            return Conflict(new { error = "Subscription is already linked to a Stripe subscription. Use the Stripe customer portal to change plans." });

        var package = await packageRepository.GetByIdAsync(request.PackageId, cancellationToken);
        if (package is null) return NotFound(new { error = "Package not found." });

        var priceId = package.GetStripePriceFor(cycle);
        if (string.IsNullOrWhiteSpace(priceId))
        {
            return BadRequest(new
            {
                error = $"Package '{package.Code}' has no Stripe Price id wired for {cycle}. " +
                        "Set StripePriceIdMonthly/StripePriceIdAnnual on the package before using Stripe-native billing.",
            });
        }

        var result = await gateway.CreateCheckoutAsync(request.TenantId, request.SubscriptionId, priceId, cancellationToken);
        if (result is null)
        {
            logger.LogError("StripeSubscriptionGateway returned null — see preceding log for failure reason.");
            return StatusCode(502, new { error = "Stripe checkout creation failed." });
        }

        return Ok(new CheckoutResponse(result.CheckoutUrl, result.SessionId, result.StripeCustomerId));
    }
}
