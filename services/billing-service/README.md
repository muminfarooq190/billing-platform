# Billing Service MVP notes

This service now has a pragmatic MVP pass focused on not faking money flows.

## Payment behavior

- `StripePaymentGateway` no longer returns unconditional success.
- If Stripe is configured, invoice payment requests now return an **action-required** style result with a checkout URL shape instead of pretending payment already completed.
- If Stripe is selected but not configured, the payment attempt fails explicitly.
- Mock gateway remains available for local/dev flows and returns a deterministic mock success.

## Invoice generation

Invoice generation is now package/subscription-aware:
- pricing resolves from the tenant's active commercial package metadata
- package metadata is now the primary source of truth for monthly/annual price and tax rate
- legacy compatibility packages are seeded with explicit pricing metadata so migrated tenants still work
- invoices now carry billing period start/end and pricing reference metadata
- duplicate invoice generation for the same subscription + billing period is prevented by lookup and DB uniqueness

If no active package assignment exists, or if a package does not contain valid pricing metadata, invoice generation now fails loudly instead of silently falling back to deprecated plan pricing.

## Internal finance reads

Added internal invoice read API expected by downstream services:
- `GET /billing/invoices/tenant/{tenantId}`

## Next non-MVP work

Still intentionally deferred:
- live Stripe SDK checkout session creation
- dedicated payment transaction table/entity
- refunds / partial refunds
- invoice PDF generation

## Stripe webhook / reconciliation

Added pragmatic webhook endpoint:
- `POST /billing/webhooks/stripe`

Current MVP behavior:
- validates `Stripe-Signature` using HMAC SHA-256 when `STRIPE_WEBHOOK_SECRET` is configured
- parses a Stripe-style raw event envelope from the request body
- resolves invoice id from `data.object.metadata.invoiceId` (or nested invoice metadata)
- marks invoice paid on `payment_intent.succeeded` / `checkout.session.completed`
- marks invoice failed/overdue on `payment_intent.payment_failed`

Expected webhook shape is now closer to real Stripe events, not the old flat DTO. The relevant invoice id should be present in metadata.

## Billing to communication wiring

Communication service now listens to billing invoice events and relays them into workflow sends for:
- `invoice-issued`
- `payment-receipt`

This is intentionally lightweight MVP glue built on the existing RabbitMQ billing event exchange.
