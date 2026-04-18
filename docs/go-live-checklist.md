# Go-Live Checklist - SaaS MVP

_Last updated: 2026-04-18_

This checklist is for the **current branch state** of `billing-platform` after the recent MVP hardening work across:
- billing-service
- communication-service
- travel-service
- webhook-service
- feature-based SaaS entitlements

This is not a generic checklist.
This is the practical go-live list for *this repo as it exists now*.

---

# 1. Launch decision summary

## Recommended launch position
- **Pilot / controlled MVP launch:** yes
- **Broader production launch:** only after finishing the highest-risk items below

## Strongest launch path today
- tenant/package/feature-based SaaS access
- quotation + itinerary workflows
- quote/itinerary PDF generation
- email-based customer communication with attachments
- Stripe checkout session flow + webhook reconciliation
- webhook event fanout foundation

## Weakest launch path today
- WhatsApp live delivery confidence
- communication delivery callback visibility
- billing finance hardening beyond MVP
- webhook auth/security maturity beyond tenant-header scoping

---

# 2. Core infrastructure checklist

## Environment & runtime
- [ ] production/staging environment variables are documented and actually set
- [ ] Postgres databases exist and migrations are applied for all services
- [ ] Redis is available where required
- [ ] RabbitMQ is available and reachable by billing-service, communication-service, and webhook-service
- [ ] public service URLs are correct and resolvable externally where required
- [ ] TLS/HTTPS is configured for externally reachable endpoints

## Base URLs / cross-service wiring
- [ ] `BILLING_SERVICE_URL` set correctly where consumed
- [ ] `COMMUNICATION_SERVICE_URL` set correctly where consumed
- [ ] `TRAVEL_SERVICE_URL` set correctly where consumed
- [ ] `TRAVEL_PUBLIC_BASE_URL` set correctly for document links/PDFs
- [ ] `APP_PUBLIC_BASE_URL` set where Stripe fallback URLs depend on it

---

# 3. Billing-service go-live checklist

## Payments
- [ ] `PAYMENT_GATEWAY=Stripe` set where intended
- [ ] `STRIPE_SECRET_KEY` configured
- [ ] `STRIPE_WEBHOOK_SECRET` configured
- [ ] `STRIPE_CHECKOUT_SUCCESS_URL` configured or valid `APP_PUBLIC_BASE_URL` fallback exists
- [ ] `STRIPE_CHECKOUT_CANCEL_URL` configured or valid `APP_PUBLIC_BASE_URL` fallback exists
- [ ] Stripe webhook endpoint is exposed and reachable:
  - `POST /billing/webhooks/stripe`
- [ ] checkout session metadata includes invoice correlation as expected in live environment

## Billing data / pricing
- [ ] commercial package metadata contains valid pricing objects for active packages
- [ ] legacy tenants are backfilled into compatibility packages where needed
- [ ] billing migrations applied, including invoice period uniqueness support
- [ ] at least one real end-to-end invoice payment test completed in staging

## Billing operations
- [ ] scheduler jobs are deployed intentionally (single node or safe concurrency strategy)
- [ ] duplicate invoice generation has been tested for the same billing period
- [ ] internal tenant invoice read API works for downstream consumers

---

# 4. Communication-service go-live checklist

## Email
- [ ] `EMAIL_PROVIDER=sendgrid` or equivalent configured intentionally
- [ ] `SENDGRID_API_KEY` / mapped configuration present
- [ ] default sender email/name configured
- [ ] at least one real quote PDF email attachment tested end-to-end
- [ ] attachment-fetch URLs are reachable from communication-service runtime

## WhatsApp
- [ ] `WHATSAPP_PROVIDER=twilio` configured if WhatsApp is part of MVP
- [ ] `TWILIO_ACCOUNT_SID` configured
- [ ] `TWILIO_AUTH_TOKEN` configured
- [ ] `WHATSAPP_DEFAULT_FROM_NUMBER` configured
- [ ] Twilio sender/sandbox is approved for the destination test path
- [ ] at least one real WhatsApp media message tested successfully with a quote/itinerary PDF URL
- [ ] PDF/media URLs are reachable externally by Twilio

## Recipient routing
- [ ] travel contact resolution works for real customer contacts
- [ ] recipient preferences are populated for at least one realistic user/customer scenario
- [ ] callers know that preferred channel routing works best when channel is omitted

## Communication operations
- [ ] notification replay endpoints are reachable only to intended operators/tools
- [ ] failed notification list/detail is usable in staging/ops
- [ ] attachment fetch failure behavior is understood and accepted before launch

---

# 5. Travel-service go-live checklist

## Product flows
- [ ] quotation create/send flow tested end-to-end
- [ ] booking creation tested end-to-end
- [ ] itinerary creation tested end-to-end
- [ ] communication workflow trigger after quotation send verified
- [ ] communication workflow trigger after itinerary creation verified

## Documents
- [ ] QuestPDF document rendering works in deployment environment
- [ ] quotation PDF endpoint works:
  - `GET /travel/documents/quotations/{quotationId}/revisions/{revisionId}/pdf`
- [ ] itinerary PDF endpoint works:
  - `GET /travel/documents/bookings/{bookingId}/itinerary/pdf`
- [ ] generated PDFs are downloadable and readable from the actual environment

## Access control
- [ ] travel feature gates are enabled for intended tenant packages
- [ ] public/share quotation URL behavior has been tested intentionally
- [ ] document endpoint exposure model is reviewed for tenant/security expectations

---

# 6. Feature/entitlement checklist

## Billing-driven SaaS access
- [ ] feature catalog seeded in target environment
- [ ] commercial packages seeded in target environment
- [ ] tenant package assignments exist for launch tenants
- [ ] feature gates verified in travel-service
- [ ] feature gates verified in communication-service
- [ ] feature gates verified in identity-service

## Package correctness
- [ ] package metadata matches actual commercial offer
- [ ] communication feature included where notification send is expected
- [ ] travel quotation / booking / notes / timeline features align with target package
- [ ] branding/admin features align with target package

---

# 7. Webhook-service go-live checklist

## Service readiness
- [ ] webhook-service migrations applied
- [ ] webhook-service can connect to RabbitMQ
- [ ] webhook-service can connect to Postgres
- [ ] outbound webhook targets can be reached from deployment network

## Operational behavior
- [ ] subscription creation validated in staging
- [ ] tenant-scoped delivery listing validated
- [ ] replay behavior validated for intended tenant only
- [ ] retry behavior validated for 4xx / 5xx / 429 cases
- [ ] event dedupe behavior validated in staging

## Security
- [ ] `x-tenant-id` trust boundary is acceptable for current deployment model
- [ ] webhook targets verify outbound signatures
- [ ] signing secret management process is documented

---

# 8. Must-pass live tests before launch

## Billing
- [ ] create invoice -> create Stripe checkout session -> pay -> webhook marks paid

## Communication
- [ ] send quote PDF by email to real address
- [ ] send itinerary PDF by email to real address
- [ ] if WhatsApp is in MVP: send quote/itinerary media message successfully

## Travel
- [ ] send quotation -> customer communication arrives
- [ ] create itinerary -> customer communication arrives

## Webhooks
- [ ] billing event triggers webhook delivery to test endpoint
- [ ] travel event triggers webhook delivery to test endpoint
- [ ] duplicate inbound event does not create duplicate outbound webhook delivery

---

# 9. High-risk launch caveats

These items are not necessarily launch blockers, but they are the biggest sources of pain if ignored.

- [ ] communication delivery callbacks are still limited; ops must not expect perfect delivered/opened truth yet
- [ ] WhatsApp path is more fragile than email and must be proven live before promising it loudly
- [ ] billing is MVP-ready, not finance-platform-complete; do not oversell refund/collections maturity
- [ ] webhook auth model is improved but still basic; deploy with trusted network boundaries if needed

---

# 10. Final go-live decision gate

## Launch if ALL are true
- [ ] package/feature entitlements verified for target tenants
- [ ] Stripe live/staging payment flow verified
- [ ] quote and itinerary PDFs render successfully in deployed environment
- [ ] email quote/itinerary delivery verified end-to-end
- [ ] recipient resolution verified for real customer contacts
- [ ] RabbitMQ/Redis/Postgres connectivity stable
- [ ] webhook-service tenant-scoped delivery tested

## Do not launch loudly if any of these are false
- [ ] recipient resolution remains uncertain
- [ ] quote/itinerary PDFs fail in runtime
- [ ] SendGrid/Twilio config missing
- [ ] WhatsApp is promised but not live-tested
- [ ] Stripe webhook reconciliation not verified in deployed environment
