namespace BillingService.Domain.Enums;

/// <summary>
/// Lifecycle states for a tenant subscription.
///
///   Active    — normal billing, full workspace access.
///   Paused    — admin-paused (collection paused, access remains; rarely used).
///   PastDue   — invoice unpaid past grace window; portal flips read-only
///               at SubscriptionExpiryBanner / RouteGuard layer until invoice
///               clears or admin reactivates.
///   Cancelled — user-initiated cancel; access continues until CurrentPeriodEnd
///               then transitions to Cancelled+period-elapsed (effectively expired).
///
/// `Expired` is intentionally not modeled as a distinct state — once
/// `Cancelled` and `CurrentPeriodEnd` is in the past, the portal treats it as
/// expired (see voyara-portal `lib/billing/subscription-status.ts`).
/// </summary>
public enum SubscriptionStatus { Active, Paused, PastDue, Cancelled }
