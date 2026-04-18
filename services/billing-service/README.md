# Billing Service MVP notes

This service now has a pragmatic MVP pass focused on not faking money flows.

## Payment behavior

- `StripePaymentGateway` no longer returns unconditional success.
- If Stripe is configured, invoice payment requests now return an **action-required** style result with a checkout URL shape instead of pretending payment already completed.
- If Stripe is selected but not configured, the payment attempt fails explicitly.
- Mock gateway remains available for local/dev flows and returns a deterministic mock success.

## Invoice generation

Invoice generation is now package/subscription-aware:
- pricing resolves from the tenant's active package metadata when available
- otherwise falls back to sane plan defaults by billing cycle
- invoices now carry billing period start/end and pricing reference metadata
- duplicate invoice generation for the same subscription + billing period is prevented by lookup and DB uniqueness

## Internal finance reads

Added internal invoice read API expected by downstream services:
- `GET /billing/invoices/tenant/{tenantId}`

## Next non-MVP work

Still intentionally deferred:
- live Stripe SDK checkout session creation
- webhook-based reconciliation
- dedicated payment transaction table/entity
- refunds / partial refunds
- invoice PDF generation
