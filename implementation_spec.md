# Implementation Spec - Communication Service + Billing Service MVP Readiness

_Last updated: 2026-04-18_

This document is the implementation-ready audit spec for making the **communication-service** and **billing-service** MVP-ready in `billing-platform`.

The goal is not theoretical completeness.
The goal is to identify what is already good enough, what is still stubbed or dangerously thin, and what must be implemented next so Voyara can ship a credible MVP for:

- quotation send/share follow-up messaging
- booking confirmations and trip communications
- invoice generation and payment handling
- finance visibility for travel bookings
- tenant-safe operational use in a real environment

Bluntly: the repo already has strong structure and decent foundations, but both services still contain fake-it-till-demo parts that need to become real before anyone should call them production-ish.

---

# 1. Executive summary

## 1.1 Overall audit conclusion

### Communication service
**Status:** structurally close to MVP, but still incomplete for real multichannel customer communication.

What is already present:
- notification aggregate and persistence
- template CRUD
- recipient preferences
- background dispatcher loop
- provider abstraction for email + SMS
- SendGrid/Twilio transport adapters
- branding integration hook
- feature gating hook
- in-app and push placeholders

What is still missing or weak:
- no first-class delivery endpoint for quote PDFs / booking confirmations as business workflows
- no provider-backed WhatsApp channel
- push is a stub logger, not real delivery
- in-app is effectively a DB status trick, not a full inbox model
- no webhook/callback ingestion for delivery status, bounce, or read receipts
- request contracts trust caller-supplied tenant id in some write paths
- no idempotency, rate-control, dead-lettering, or operational admin visibility

### Billing service
**Status:** usable as a backend foundation for entitlements and simple invoices, but **not yet MVP-ready** as a real billing/collections service.

What is already present:
- subscriptions
- invoices
- invoice list/detail endpoints
- scheduled billing cycle generation
- overdue checker
- payment gateway abstraction
- Stripe gateway placeholder wiring
- dashboard/read models
- strong entitlement foundation

What is still missing or weak:
- invoice generation is hardcoded to a single fake base-plan line item
- Stripe gateway is a stub that always returns success
- no payment intent/session flow
- no webhook-driven reconciliation from Stripe
- no refunds / partial payments / payment failures model
- no tenant-safe finance API for travel-service integration shape currently expected
- scheduler persistence semantics need review for idempotent recurring billing
- no invoice PDF / customer-send integration with communication service

## 1.2 MVP recommendation

To make these two services MVP-ready fastest, do the work in this order:

1. **Communication path for quotation + booking customer messages**
2. **Real billing payment integration and reconciliation**
3. **Booking finance read APIs expected by travel-service**
4. **Operational hardening: tenant safety, retries, observability, idempotency**
5. **Invoice/receipt communication automation**

If you only fix one thing in billing-service first, fix the fake Stripe implementation.
If you only fix one thing in communication-service first, add a real business-facing outbound workflow for quotations/booking confirmations, not just generic notifications.

---

# 2. Audit scope and method

This audit was based on the current codebase under:

- `services/communication-service`
- `services/billing-service`
- relevant travel-service integration points
- existing implementation spec docs and README

The audit focused on:
- current feature coverage
- implementation realism vs placeholder behavior
- contract gaps between services
- tenant-safety and operational readiness
- what is required specifically for **communication service** and **billing service** to support Voyara MVP workflows

---

# 3. Product-level MVP outcomes required

For MVP, these user-visible outcomes must work end-to-end.

## 3.1 Communication MVP outcomes

The platform must support:
- send quotation notifications to customer through at least **email**
- optionally send short follow-up nudges through **SMS and/or WhatsApp**
- send booking confirmation messages after quote acceptance / booking creation
- send invoice/payment reminders
- respect recipient communication preferences where appropriate
- keep an auditable per-notification status trail
- allow support/admins to inspect whether a message was queued, sent, failed, or read

## 3.2 Billing MVP outcomes

The platform must support:
- create and manage tenant subscriptions
- generate invoice records from real pricing logic
- collect payment using a real payment provider
- reconcile payment success/failure asynchronously
- expose invoice data to other services safely
- support overdue marking and reminder workflows
- provide basic dashboard visibility for tenant finance state

---

# 4. Communication service - current state audit

## 4.1 What is already good

The service has a solid baseline architecture:
- API + MediatR + EF Core + Dapper-style read structure
- notification aggregate with statuses and retry count
- templates
- recipient preferences
- background dispatch loop
- provider abstraction for email and SMS
- config validation for SendGrid/Twilio
- branding enrichment hook via identity-service
- entitlement enforcement hook via billing-service

That means the skeleton is not the problem.
The problem is last-mile productization.

## 4.2 What is clearly implemented

### Domain and persistence
Implemented entities include:
- `Notification`
- `NotificationTemplate`
- `RecipientPreferences`

Notification lifecycle supports:
- pending
- queued
- sent
- delivered
- read
- failed
- bounced

### Delivery providers
Implemented provider adapters:
- `SendGridEmailProvider`
- `TwilioSmsProvider`
- log providers for local/dev fallback

### Dispatchers
Implemented dispatchers:
- `EmailDispatcher`
- `SmsDispatcher`
- `PushNotificationDispatcher`
- `InAppDispatcher`

### Background processing
`NotificationDispatcherService`:
- polls pending notifications
- maps notification channel to dispatcher
- sends messages
- marks sent/failed
- resets retryable failed notifications

## 4.3 What is not MVP-ready yet

### Gap A - generic notification model, but no business workflow contract
Right now the service exposes generic notification/template endpoints, but MVP needs concrete communication workflows for:
- quotation sent
- quotation accepted / rejected follow-up
- booking confirmed
- payment reminder
- invoice issued
- invoice paid receipt

Today those workflows would need to be hand-assembled by other services or the frontend. That is brittle and will rot fast.

### Gap B - tenant trust issue in API contracts
`NotificationsController` and `TemplatesController` accept tenant identifiers from request body or route instead of deriving all write-scoped tenancy from context.

That is not a tiny style nit. It is a multi-tenant correctness problem.

### Gap C - no WhatsApp / channel parity for real travel ops
Previous repo memory already pointed to the exact concern: quote PDFs / booking confirmations need all channels.

Current channel support in practice:
- Email: real-ish
- SMS: real-ish
- Push: stub logger only
- In-app: pseudo-success only
- WhatsApp: not implemented

For a travel CRM MVP, WhatsApp is often not optional. Especially if the product pitch includes customer-facing quote/booking comms.

### Gap D - no delivery status callback ingestion
There is no visible inbound webhook or provider callback surface for:
- email delivered/opened/bounced
- SMS delivered/undelivered
- WhatsApp delivered/read

So notification status is mostly optimistic. That is demo-ready, not ops-ready.

### Gap E - no attachment-aware outbound workflow
Travel communications often require:
- quote PDF
- itinerary PDF
- invoice PDF
- voucher / booking confirmation attachments

Current delivery contracts are subject/body centric. There is no explicit support for attachments or document references in notification payloads.

### Gap F - no idempotency / dedupe strategy
If upstream retries the same send command, this service can create duplicate outbound messages. That becomes embarrassing fast with real customers.

### Gap G - no operator/admin troubleshooting endpoints
There is no obvious operational surface for:
- listing failed notifications by tenant/channel/date
- replaying a failed notification manually
- inspecting provider response / last error in a support-friendly way

## 4.4 MVP readiness verdict for communication service

**Not yet MVP-ready**, but very close if scoped properly.

It can become MVP-ready without a huge rewrite if we:
- add business event/message orchestration
- add one more serious channel choice (preferably WhatsApp)
- harden tenant derivation and status tracking
- support attachments / document links

---

# 5. Billing service - current state audit

## 5.1 What is already good

The service has a strong shape for a billing foundation:
- subscriptions aggregate and lifecycle
- invoice aggregate and lifecycle
- repository/application/query layering
- schedulers for due billing and overdue checks
- entitlement model and caching
- dashboard/read APIs
- payment abstraction

Again, the architecture is not the weak part.
The weak part is that some critical money-moving behavior is still fake.

## 5.2 What is clearly implemented

### Subscription flows
Implemented:
- create subscription
- get tenant subscription
- cancel subscription

### Invoice flows
Implemented:
- generate invoice command
- get invoice by id
- list invoices by tenant context
- mark overdue background processing
- pay invoice endpoint via payment gateway abstraction

### Scheduler flows
Implemented:
- daily due-subscription billing scheduler
- hourly overdue invoice scanner

### Payment abstraction
Implemented interface:
- `IPaymentGateway.ProcessAsync(Guid invoiceId, Money amount, CancellationToken)`

## 5.3 What is not MVP-ready yet

### Gap A - invoice generation is hardcoded demo logic
`GenerateInvoiceCommandHandler` generates a single line item:
- `Base plan`, quantity `1`, amount `49 USD`
- hardcoded tax `4.9 USD`

That means current billing generation is not tied to:
- subscription plan catalog
- tenant package assignment
- add-ons / feature pricing
- billing cycle
- discounts
- tax rules
- proration

This is the biggest fake-MVP smell in the service besides Stripe.

### Gap B - Stripe gateway is a stub
`StripePaymentGateway` currently just returns `PaymentResult.Success`.

That means:
- no payment actually happens
- no payment method capture exists
- no provider transaction ids exist
- no async confirmation exists
- no failure cases are real

Calling this MVP-ready would be comedy.

### Gap C - no payment initiation model
There is no clear API for:
- creating checkout session
- creating payment intent
- attaching stored customer/payment method
- confirming off-session renewal charge

`POST /billing/invoices/{id}/pay` is too thin for real payment UX or automation.

### Gap D - no payment webhook ingestion / reconciliation
Real billing requires provider events for:
- payment succeeded
- payment failed
- charge refunded
- invoice payment action required
- subscription renewal issues

Without webhooks, state becomes inaccurate or synchronous-only fantasy.

### Gap E - missing finance API expected by travel-service
Travel service has `IBillingFinanceClient` expecting:
- `GET billing/invoices/tenant/{tenantId}`

The current billing controllers shown in the audit do not expose that exact endpoint shape.

So either:
- there is a missing controller/path elsewhere, or
- travel-service integration contract is currently broken/incomplete

That needs to be closed before claiming booking finance visibility is MVP-ready.

### Gap F - no invoice/customer document distribution flow
Billing records exist, but there is no complete path for:
- invoice PDF generation
- receipt generation
- sending invoice/reminder through communication service

### Gap G - scheduler idempotency risk
`BillingSchedulerService` generates invoices for due subscriptions and calls `subscription.RenewNextCycle()` in-process. The flow likely works in simple cases, but for MVP it needs explicit assurance around:
- duplicate scheduler runs
- concurrent nodes
- retry after partial failure
- invoice uniqueness per billing period

If deployed with multiple instances without leadership/locking, this can duplicate invoices.

## 5.4 MVP readiness verdict for billing service

**Not MVP-ready** as a real billing product surface.

It is MVP-foundation-ready, not payment-ops-ready.

To get to MVP, the payment and invoice realism gaps must be fixed first.

---

# 6. Cross-service integration gaps

## 6.1 Travel -> communication gap
Travel-service Phase 6/7 now supports:
- quotation share/public flow
- booking creation
- booking docs
- booking itinerary

But there is no clear audited implementation showing automatic communication events for:
- quote sent email/SMS/WhatsApp
- booking confirmation message
- booking document delivery

That means the business workflow is still fragmented.

## 6.2 Travel -> billing gap
Travel-service has `IBillingFinanceClient`, which indicates booking or tenant finance visibility should come from billing-service.

But the contract must be aligned and actually implemented.

## 6.3 Billing -> communication gap
Billing overdue / invoice-issued states should trigger communications, but there is no explicit orchestration shown for:
- send invoice issued notice
- send payment reminder
- send payment success receipt
- send payment failure / action-required message

For MVP, those should not be manual forever.

---

# 7. Target MVP architecture decisions

## 7.1 Communication channel decision
For MVP, commit to these channels:
- **Email**: required
- **SMS**: optional but strongly recommended
- **WhatsApp**: recommended for travel CRM MVP if target market expects it
- **In-app**: keep as secondary/supporting channel
- **Push**: do not treat as MVP-critical unless mobile app launch requires it immediately

## 7.2 Billing payment decision
For MVP, implement one real provider end-to-end.
Best obvious path from the codebase is:
- **Stripe** for real payments

Do not build abstract multi-provider complexity first. One real provider beats three fake ones.

## 7.3 Business communication pattern
Prefer introducing a message orchestration layer with event-friendly commands such as:
- `SendQuotationCustomerMessage`
- `SendBookingConfirmationMessage`
- `SendInvoiceIssuedMessage`
- `SendPaymentReminderMessage`
- `SendReceiptMessage`

Those can still resolve into generic notification/template sends internally.

This preserves the current service investment while making the product usable.

---

# 8. Required implementation changes - communication service

## 8.1 Priority C1 - tenant-safe API contract cleanup

### Problem
Several endpoints accept tenant ids from request payload or route on writes.

### Required changes
1. Derive tenant id from `ITenantContext` on all authenticated write operations.
2. Remove `TenantId` from write request DTOs where tenant should never be caller-controlled.
3. Keep route/query tenant ids only for explicit admin/internal system endpoints if genuinely needed.
4. Add tests covering tenant spoof attempts.

### Affected surfaces
At minimum:
- `SendNotificationRequest`
- `CreateTemplateRequest`
- any write path using request-supplied tenant id

### Done when
- normal app writes never accept caller-supplied tenant id
- tests prove cross-tenant spoof attempts fail

---

## 8.2 Priority C2 - business workflow messaging API

### Problem
Current API is too generic for real quote/booking workflows.

### Required changes
Add a higher-level workflow surface in communication-service or an orchestration layer consumed by travel/billing services.

### Recommended commands/endpoints
- `POST /communication/workflows/quotation-sent`
- `POST /communication/workflows/booking-confirmed`
- `POST /communication/workflows/invoice-issued`
- `POST /communication/workflows/payment-reminder`
- `POST /communication/workflows/payment-receipt`

### Request shape concept
Each workflow payload should support:
- recipient identity references
- preferred channel(s)
- template key
- placeholders
- optional attachment/document references
- idempotency key
- correlation/reference ids (`quotationId`, `bookingId`, `invoiceId`)

### Why this matters
This turns the service from a generic notification toy into a product-facing communication service.

---

## 8.3 Priority C3 - attachment/document-aware sends

### Problem
Quotes, itineraries, invoices, receipts, and booking confirmations often need files.

### Required changes
Extend notification send model to support either:
- direct attachments metadata, or
- document reference ids / signed URLs fetched from source services

### MVP recommendation
Do not start with raw file upload through communication-service.
Instead support:
- `DocumentReferences[]`
- signed URLs or storage-backed attachment fetch logic

### Minimum supported use cases
- quotation PDF link or attached PDF
- booking confirmation PDF / itinerary document
- invoice PDF
- receipt PDF

### Done when
A booking or invoice workflow can send a customer-facing email with at least one generated document attached or linked safely.

---

## 8.4 Priority C4 - WhatsApp channel support

### Problem
Travel ops commonly depend on WhatsApp. Current service does not support it.

### Required changes
1. Add new channel enum value: `WhatsApp`
2. Add provider abstraction for WhatsApp delivery
3. Implement one provider adapter, likely via Twilio WhatsApp or another approved provider
4. Extend recipient preferences to store WhatsApp-capable number and opt-in/out
5. Add template support for WhatsApp-safe body formatting

### MVP fallback
If full WhatsApp API onboarding is blocked, support:
- email as mandatory channel
- SMS as fallback
- design WhatsApp abstractions now so it is the next drop-in provider

But if the product promise includes WhatsApp at MVP, then do not fake it. Implement it for real.

---

## 8.5 Priority C5 - provider callback/webhook ingestion

### Problem
Current status model is optimistic and incomplete.

### Required changes
Add inbound webhook endpoints for provider delivery updates.

### Required tracked states
At least:
- delivered
- bounced / undelivered
- read/opened where supported
- provider error code / error reason

### Needed behavior
- locate notification by provider message id
- update status transition safely
- persist provider metadata/audit fields
- reject invalid signatures

### Done when
At least SendGrid/Twilio delivery feedback can update notification state asynchronously.

---

## 8.6 Priority C6 - idempotency and replay safety

### Problem
Duplicate sends are currently too easy.

### Required changes
1. Add `IdempotencyKey` and/or unique workflow reference constraints.
2. Prevent duplicate notification creation for the same semantic event within a configured window.
3. Add manual replay endpoint for failed sends.

### Suggested uniqueness rules
Examples:
- one `booking-confirmed` communication per booking/channel/template unless forced replay
- one `invoice-issued` communication per invoice/channel/template unless invoice version changes

---

## 8.7 Priority C7 - operational visibility

### Required additions
Add admin/support read endpoints for:
- list notifications by tenant/status/channel/date/reference
- get notification detail including attempts/provider id/last error
- replay failed notification
- cancel queued notification if not yet dispatched

This is boring but necessary. Support teams cannot debug vibes.

---

## 8.8 Priority C8 - push and in-app scope decision

### Push
Current `PushNotificationDispatcher` is just logging.
If mobile push is not in MVP scope, explicitly mark it non-MVP and keep it behind feature flags.
Do not pretend it exists.

### In-app
Current in-app path returns success immediately and relies on DB visibility. That is acceptable for MVP **if** product wording is honest and UI inbox/read surfaces are in place.
Otherwise it is just an implementation detail, not a user feature.

---

# 9. Required implementation changes - billing service

## 9.1 Priority B1 - replace fake Stripe gateway with real integration

### Problem
`StripePaymentGateway` always returns success.
That has to die.

### Required changes
Implement real Stripe behavior for at least one supported payment path.

### MVP options
#### Option A - hosted checkout session
Best if there is a customer payment UX in web app.

Support:
- create checkout session for invoice
- redirect customer to Stripe hosted page
- reconcile via webhook

#### Option B - payment intent / stored payment method
Best for admin-collected or subscription-renewal flows.

Support:
- create customer/payment method mapping
- create payment intent or off-session charge
- reconcile via webhook

### Recommendation
For MVP speed:
- use **Stripe Checkout** for manual invoice payment
- use webhook reconciliation
- defer advanced off-session renewals if needed

---

## 9.2 Priority B2 - payment state model expansion

### Problem
Current payment flow is too binary.

### Required changes
Expand billing model to represent:
- pending payment
- action required
- paid
- failed
- refunded
- partially refunded (optional later)

### Schema/domain updates
Either enrich `InvoiceStatus` and/or add a payment transaction entity such as:
- `InvoicePayment`
- `PaymentAttempt`

### Recommended new entity
`PaymentTransaction`

Fields:
- `id`
- `invoice_id`
- `tenant_id`
- `provider`
- `provider_payment_id`
- `amount`
- `currency`
- `status`
- `failure_code`
- `failure_message`
- `created_at`
- `updated_at`
- `completed_at`
- raw metadata JSON

Without this, finance debugging will be a mess.

---

## 9.3 Priority B3 - invoice generation from real commercial logic

### Problem
Invoices are currently hardcoded.

### Required changes
Invoice generation must derive from actual billing configuration.

### MVP minimum rules
Use:
- subscription/package assignment
- billing cycle
- package price
- add-on/feature charges if enabled
- tax policy hook or configurable tax rate

### Implementation path
1. Introduce pricing resolver service
2. Resolve active tenant package / subscription details
3. Build invoice line items from plan + add-ons
4. Persist billing period start/end on invoice
5. Add uniqueness rule for one invoice per tenant/subscription/billing-period

### Suggested invoice additions
Add fields like:
- `BillingPeriodStart`
- `BillingPeriodEnd`
- `ExternalInvoiceNumber`
- `PaymentReference`

---

## 9.4 Priority B4 - webhook ingestion for Stripe reconciliation

### Problem
No real async billing truth exists without provider callbacks.

### Required changes
Add Stripe webhook endpoint handling:
- checkout.session.completed
- payment_intent.succeeded
- payment_intent.payment_failed
- charge.refunded
- invoice.payment_failed if subscriptions are later provider-managed

### Required behavior
- verify webhook signature
- map provider event to invoice/payment transaction
- update invoice/payment state idempotently
- emit outbox/domain events for downstream communication

---

## 9.5 Priority B5 - tenant finance read API contract completion

### Problem
Travel-service expects tenant finance read access in a specific shape.

### Required changes
Add or align endpoint(s) such as:
- `GET /billing/invoices/tenant/{tenantId}` for internal service-to-service use
- optionally `GET /billing/tenants/{tenantId}/invoices`

### Requirements
- internal auth/authorization only
- tenant-safe filtering
- pagination support
- include fields needed for booking finance summary

### Minimum response shape
- invoice id
- tenant id
- status
- total amount
- currency
- due date
- paid at

That matches the current `BookingInvoiceDto` expectation closely.

---

## 9.6 Priority B6 - invoice send/reminder/receipt orchestration

### Problem
Billing records exist, but customer communication flow is missing.

### Required changes
On important billing events, publish or invoke communication workflows for:
- invoice issued
- invoice overdue reminder
- payment success receipt
- payment failure / action required

### Preferred mechanism
Use outbox/domain events and let a worker or subscriber call communication-service.
Do not tightly couple synchronous billing writes to provider messaging where avoidable.

---

## 9.7 Priority B7 - scheduler idempotency and concurrency safety

### Problem
Recurring jobs may generate duplicate invoices if deployed carelessly.

### Required changes
1. Add uniqueness constraints around billing period invoice generation.
2. Add idempotent command handling.
3. Optionally add distributed lock / leader-election semantics for recurring scheduler jobs.
4. Ensure transaction boundary covers both invoice creation and subscription renewal period advance.

### Done when
Re-running the billing scheduler for the same due subscription cannot create duplicate invoices for the same cycle.

---

## 9.8 Priority B8 - invoice document generation

### Required changes
Support generation of:
- invoice PDF
- payment receipt PDF

This can be minimal for MVP, but there must be a customer-facing artifact if invoices are being sent externally.

### MVP approach
Generate a basic HTML-to-PDF invoice/receipt document with:
- invoice number
- tenant name / branding
- line items
- total / tax / currency
- due date / paid status

---

# 10. Recommended implementation plan

## Phase A - close the fake parts first

### A1 Communication
- tenant-safe write contracts
- business workflow API for quote/booking/invoice messages
- attachment/document reference support

### A2 Billing
- real Stripe integration
- payment transaction model
- webhook reconciliation
- real invoice pricing resolver

This phase is mandatory.

## Phase B - close integration gaps

### B1 Communication
- quote sent / booking confirmed templates
- invoice issued / reminder / receipt templates
- provider callback endpoints

### B2 Billing
- internal invoice tenant read API for travel-service
- emit communication-driving events
- invoice PDF/receipt generation

## Phase C - operational hardening

### C1 Communication
- idempotency keys
- failed send replay
- support/admin filters
- better retry/backoff policy

### C2 Billing
- scheduler idempotency hardening
- duplicate prevention
- refund support stub/initial path
- finance audit observability

## Phase D - channel expansion

- WhatsApp provider
- optional real push provider if needed

---

# 11. PR breakdown

## PR 1 - communication service tenant safety + workflow contracts
Includes:
- remove caller-supplied tenant id from write DTOs
- add workflow endpoints/commands for quote, booking, invoice communications
- add idempotency key support on workflow sends
- tests for tenant spoof prevention

## PR 2 - communication service document-aware delivery + admin visibility
Includes:
- notification document references / attachments support
- list/detail/filter notification endpoints for ops
- replay failed notification endpoint
- quote/booking/invoice template seeds or examples

## PR 3 - communication service delivery callbacks + WhatsApp abstraction
Includes:
- provider callback/webhook endpoints
- async status update handling
- `WhatsApp` channel enum + provider abstraction
- optional first provider implementation if credentials/process are ready

## PR 4 - billing service real Stripe payment path
Includes:
- replace fake `StripePaymentGateway`
- create checkout/payment initiation endpoint(s)
- webhook reconciliation endpoint
- payment transaction persistence
- invoice pay flow refactor

## PR 5 - billing service pricing resolver + invoice realism
Includes:
- line item resolver from package/subscription state
- billing period fields
- uniqueness constraints for per-cycle invoice generation
- scheduler idempotency hardening

## PR 6 - billing/communication integration for invoice lifecycle
Includes:
- invoice issued / overdue / paid events
- communication-service invocation or event subscriber wiring
- invoice/receipt PDF generation
- reminder flow

## PR 7 - travel/communication/billing integration closure
Includes:
- quote sent -> customer communication
- booking confirmed -> customer communication
- travel-service finance read integration alignment
- end-to-end tests across workflow boundaries where possible

---

# 12. Detailed acceptance criteria

## 12.1 Communication service acceptance criteria

Communication service is MVP-ready when:
- [ ] all authenticated writes derive tenant from context, not request body
- [ ] a quote can trigger a customer email using a workflow-oriented API
- [ ] a booking confirmation can trigger a customer email using a workflow-oriented API
- [ ] invoice issued / overdue / receipt workflows exist
- [ ] email provider integration works with real credentials
- [ ] SMS provider integration works if enabled
- [ ] WhatsApp is either fully implemented or explicitly removed from MVP promise
- [ ] notification statuses can be updated from provider callbacks
- [ ] failed notifications can be listed and replayed
- [ ] duplicate workflow sends are prevented via idempotency/reference controls
- [ ] at least one document-aware send path exists for quote/invoice/booking communications

## 12.2 Billing service acceptance criteria

Billing service is MVP-ready when:
- [ ] invoice generation uses actual subscription/package pricing rules, not hardcoded values
- [ ] a real Stripe payment path exists
- [ ] payment success/failure is reconciled asynchronously via webhook
- [ ] invoice/payment transaction records are queryable and auditable
- [ ] scheduler runs are idempotent for the same billing cycle
- [ ] internal tenant invoice read API exists for downstream services
- [ ] invoice issued / overdue / paid events can drive communication workflows
- [ ] invoice and receipt artifacts can be generated for customer delivery

---

# 13. Testing checklist

## 13.1 Communication tests

### Domain/application
- [ ] idempotent duplicate send prevention
- [ ] recipient preference enforcement with critical override
- [ ] workflow request builds correct channel/template payload
- [ ] attachment/document references are validated

### Integration
- [ ] send quote message via workflow endpoint
- [ ] send booking confirmation via workflow endpoint
- [ ] send invoice reminder via workflow endpoint
- [ ] provider callback updates notification to delivered/bounced/read
- [ ] tenant spoof attempts fail on write APIs
- [ ] failed notification replay works

### Security
- [ ] webhook signatures verified for provider callbacks
- [ ] one tenant cannot query another tenant's notification detail

## 13.2 Billing tests

### Domain/application
- [ ] invoice generation reflects active package/subscription pricing
- [ ] duplicate generation for same billing period prevented
- [ ] payment transaction lifecycle handles success/failure/retry states

### Integration
- [ ] create checkout/payment session for invoice
- [ ] Stripe webhook marks invoice paid
- [ ] Stripe failure webhook marks action required / failed
- [ ] overdue scheduler marks overdue invoices correctly
- [ ] tenant finance internal read endpoint returns expected DTO shape

### Security
- [ ] internal billing read endpoints are not publicly tenant-spoofable
- [ ] webhook signatures required and validated

---

# 14. Suggested file-level implementation targets

## Communication service
Likely touch:
- `src/Api/Contracts/NotificationRequests.cs`
- `src/Api/Controllers/NotificationsController.cs`
- `src/Api/Controllers/TemplatesController.cs`
- new workflow controller/commands under `src/Application/Commands/...`
- `src/Domain/Aggregates/Notification.cs`
- dispatcher/provider abstractions for WhatsApp and attachments
- callback/webhook controller(s)
- query/read models for ops/admin visibility

## Billing service
Likely touch:
- `src/Application/Commands/GenerateInvoice/GenerateInvoiceCommandHandler.cs`
- `src/Application/Commands/ProcessPayment/ProcessPaymentCommandHandler.cs`
- `src/Infrastructure/Payments/StripePaymentGateway.cs`
- new Stripe webhook controller
- new payment transaction aggregate/repository/migration
- scheduler services for idempotency hardening
- invoice read controller(s) for tenant/internal integration

---

# 15. Final blunt recommendations

## Communication service
This service is not far off.
Do **not** rewrite it.
Just stop treating it like a generic notification sandbox and make it a real product workflow service.

Biggest wins:
1. tenant-safe writes
2. workflow APIs
3. attachment-aware sends
4. callback-driven delivery state
5. WhatsApp if the market expects it

## Billing service
This one needs more serious work because money is involved.
The architecture is fine, but the two biggest paths are still fake:
- invoice generation logic
- Stripe payment handling

That means this service should not be marketed as payment-ready yet.

Biggest wins:
1. real Stripe integration
2. real invoice pricing resolver
3. webhook reconciliation
4. scheduler idempotency
5. invoice communication/document flow

## Combined MVP truth
If someone asked, “Can we demo these services now?”
- yes

If someone asked, “Can we run an actual MVP customer workflow on them?”
- communication-service: almost, after targeted hardening
- billing-service: not safely until real payment and invoice logic land

That’s the honest answer.
