# Implementation Spec - Travel CRM Critical Missing Features

_Last updated: 2026-04-09_

This document is a critical product/engineering assessment of what the current `travel-service` is still missing if Voyara wants to feel like a serious travel CRM SaaS and not just a decent workflow skeleton.

Blunt summary:
there is now a respectable quotation/booking/timeline base, but several high-impact CRM capabilities are still missing or underpowered.

The main gaps are no longer the obvious CRUD basics.
The real remaining gaps are:
- commercial control
- operational execution depth
- supplier/vendor workflows
- customer self-service depth
- tasking/automation maturity
- finance handoff integration
- reporting/search/export quality

---

# 1. Current reality check

## What travel-service already does reasonably well

The service now covers:
- contacts
- quotations + revisions
- send/share/public quote viewing
- booking creation from accepted quote
- travelers
- booking items
- booking documents
- follow-ups
- timeline
- audit log
- notes/comments
- entitlement gating for premium surfaces

That means the system has graduated beyond demo CRUD.
Good.

## What it still does not do well enough

If a real travel agency/team tried to run serious multi-user operations on this today, they would still hit hard walls around:
- approval workflows
- payments/deposits/collection visibility from travel context
- supplier confirmation lifecycle
- itinerary execution quality
- customer portal depth
- search/reporting/export
- team work assignment + SLA management
- automation triggers/reminders
- cancellation/refund/change workflows
- document generation and branded outputs at scale

---

# 2. Product gaps that are now most critical

## Tier A - must-have to feel like a real travel CRM
1. quotation approval + commercial controls
2. booking financial state / payment visibility
3. supplier confirmation workflow
4. customer portal actionability
5. task/reminder automation and SLA follow-up
6. global search / filtered reporting / export

## Tier B - high-value operational maturity
7. booking change / cancellation / refund workflows
8. itinerary day-plan and service-delivery depth
9. role-based internal workflow controls
10. document generation + branded PDF output consistency

## Tier C - scale/enterprise maturity
11. vendor management + supplier directory
12. analytics dashboards / KPI aggregates
13. queueing/workload management
14. custom workflow rules / automation engine

---

# 3. Proposed phase plan from here

This document proposes new phases after the current quotation/booking/timeline baseline.

---

# Phase 9 - Commercial Controls and Approval Workflow

## Goal
Stop quotations and bookings from being commercially free-form chaos.

## Why this matters
Right now users can create/send/revise quotes, but there is no serious commercial governance around:
- discount approvals
- margin protection
- final sign-off for premium/high-value bookings
- who is allowed to send what

That is dangerous in a real sales environment.

## Key features

### 9.1 Quotation approval workflow
New concepts:
- `QuotationApprovalPolicy`
- `QuotationApprovalRequest`
- `QuotationApprovalDecision`

Suggested use cases:
- quote over value threshold requires approval
- quote under margin threshold requires approval
- quote with manual discount > X% requires approval
- only approved quotes can be sent externally

### 9.2 Discount + margin controls
Add to quotation/revision model or calculated read layer:
- `SellAmount`
- `CostAmount`
- `MarginAmount`
- `MarginPercent`
- `DiscountAmount`
- `DiscountPercent`
- `PricingExceptionReason`

### 9.3 Send restrictions
Before `SendQuotationCommand` succeeds, enforce:
- required revision exists
- quote not expired
- approval not pending when approval is required
- approved when policy says so

## APIs
- `POST /travel/quotations/{id}/approval-requests`
- `POST /travel/quotations/{id}/approval-requests/{approvalRequestId}/approve`
- `POST /travel/quotations/{id}/approval-requests/{approvalRequestId}/reject`
- `GET /travel/quotations/{id}/approval-status`

## Exit criteria
- high-risk quotes cannot be sent without approval
- approval actions appear in timeline/audit
- pricing exceptions are visible in read models

---

# Phase 10 - Booking Finance Visibility and Payment State

## Goal
Make booking records financially meaningful from the travel side.

## Why this matters
A travel CRM without payment visibility is half-blind.
Sales and operations need to know:
- has the deposit been paid?
- is the balance overdue?
- what is outstanding?
- did a refund happen?

The billing-service may own invoices/payments, but travel-service needs the right financial read model.

## Key features

### 10.1 Booking financial summary read model
Suggested fields:
- `DepositRequired`
- `DepositPaid`
- `BalanceDue`
- `OutstandingAmount`
- `PaidAmount`
- `PaymentStatus`
- `NextPaymentDueAt`
- `LastPaymentAt`
- `InvoiceCount`

### 10.2 Booking ↔ billing linkage
Need stable references between:
- booking
- quotation
- invoice
- payment
- refund

Likely via billing events + projection, not cross-DB access.

### 10.3 Travel-side payment timeline entries
Generate activity/timeline entries for:
- invoice issued
- deposit paid
- balance paid
- refund processed
- payment overdue

## APIs
- `GET /travel/bookings/{id}/financial-summary`
- `GET /travel/bookings/{id}/invoices`
- maybe `GET /travel/bookings/{id}/payments`

## Exit criteria
- booking detail clearly shows paid/outstanding state
- finance events are visible in timeline
- operations can tell whether a booking is financially safe to fulfill

---

# Phase 11 - Supplier and Fulfillment Workflow

## Goal
Turn booking items into a real supplier execution workflow.

## Why this matters
Current booking items exist, but supplier workflow is still thin.
Real teams need to track:
- requested vs confirmed vs ticketed vs cancelled
- supplier contact/channel
- reconfirmation dates
- vouchers/tickets issued
- operational blockers

## Key features

### 11.1 Booking item lifecycle expansion
Add richer statuses such as:
- `Requested`
- `PendingSupplier`
- `Confirmed`
- `Ticketed`
- `Issued`
- `Failed`
- `Cancelled`
- `RefundPending`

### 11.2 Supplier confirmation tracking
New fields on `BookingItem` or separate entity:
- `ConfirmationDeadline`
- `ConfirmedAt`
- `ConfirmedBy`
- `SupplierContactName`
- `SupplierContactEmail`
- `SupplierContactPhone`
- `VoucherIssuedAt`
- `IssueDeadline`

### 11.3 Supplier notes / ops blockers
Structured ops notes and blocker states for each item.

## APIs
- `POST /travel/bookings/{id}/items/{itemId}/request-confirmation`
- `POST /travel/bookings/{id}/items/{itemId}/confirm`
- `POST /travel/bookings/{id}/items/{itemId}/issue`
- `POST /travel/bookings/{id}/items/{itemId}/cancel`

## Exit criteria
- booking items have a meaningful supplier lifecycle
- operations can track what is still pending with suppliers
- confirmations/ticketing show up in timeline

---

# Phase 12 - Customer Portal Depth and Self-Service

## Goal
Make the customer portal actually useful, not just branded.

## Why this matters
Branding is nice, but customers ultimately care whether they can do anything useful.
Right now portal theming exists, but customer portal capabilities are still shallow.

## Key features

### 12.1 Customer actions
- approve/reject quotes from portal
- upload traveler documents
- update traveler info subject to rules
- download vouchers/invoices
- acknowledge itinerary updates
- respond to notes/messages where allowed

### 12.2 Portal state views
- booking status board
- payment status
- document checklist
- due actions / pending tasks

### 12.3 Portal-safe customer notes/messages
Need a proper customer-visible communication lane, not just internal notes.

## APIs
- `POST /travel/quotations/public/{token}/accept`
- `POST /travel/quotations/public/{token}/reject`
- `POST /travel/bookings/public/{token}/documents`
- `GET /travel/bookings/public/{token}/summary`
- `GET /travel/bookings/public/{token}/checklist`

## Exit criteria
- customer portal supports real actions, not just passive display
- customer document handoff becomes self-service
- quote acceptance does not require manual backchanneling

---

# Phase 13 - Tasking, SLA, and Automation

## Goal
Move from passive records to active work management.

## Why this matters
Follow-ups exist, but they are not yet a full internal execution layer.
Real teams need:
- assignee queues
- overdue work visibility
- reminders/escalations
- automation triggers from business events

## Key features

### 13.1 Work queue model
Unified pending work for:
- follow-ups
- pending approvals
- unconfirmed booking items
- missing traveler docs
- overdue payments (read-only from billing)
- expiring quotes

### 13.2 Automation rules
Examples:
- when quote sent and not viewed in 48h → create follow-up
- when booking item confirmation deadline nears → create ops reminder
- when traveler passport missing → create document task
- when deposit overdue → create finance follow-up

### 13.3 SLA states
Track:
- due soon
- overdue
- escalated
- blocked

## APIs
- `GET /travel/work-queue`
- `POST /travel/automation/rules` (later/admin)
- `POST /travel/follow-ups/{id}/complete`
- `POST /travel/tasks/{id}/reassign`

## Exit criteria
- key business events produce actionable work
- overdue items are visible and assignable
- teams stop relying on memory/WhatsApp/manual chasing

---

# Phase 14 - Reporting, Search, and Export

## Goal
Make the system findable and measurable.

## Why this matters
Without strong search and exports, the CRM remains operationally frustrating.
Without reporting, management is blind.

## Key features

### 14.1 Global travel search
Search across:
- contact names
- booking numbers
- destinations
- quote titles
- traveler names
- supplier references
- passports / document metadata where allowed

### 14.2 Reporting endpoints
Examples:
- quote conversion rate
- bookings by destination/date range/status
- pending supplier confirmations
- overdue follow-ups
- traveler document completeness
- revenue / margin by month (read-integrated with billing)

### 14.3 Export
Exportable formats:
- CSV for lists
- PDF/print for branded customer outputs
- maybe XLSX later

## APIs
- `GET /travel/search?q=...`
- `GET /travel/reports/bookings`
- `GET /travel/reports/quotations`
- `GET /travel/reports/follow-ups`
- `GET /travel/export/bookings.csv`

## Exit criteria
- operations can search quickly across travel entities
- managers can access real operational reports
- admins can export filtered data without DB access

---

# Phase 15 - Change, Cancellation, and Refund Workflow

## Goal
Handle the ugly real-world stuff.

## Why this matters
Travel products are not linear. Things change.
A serious CRM needs to handle:
- booking changes
- service amendments
- customer cancellations
- supplier penalties
- refunds/credits

## Key features

### 15.1 Change requests
New concept:
- `BookingChangeRequest`

Examples:
- change dates
- add traveler
- remove traveler
- room upgrade
- route change

### 15.2 Cancellation workflow
Track:
- who requested cancellation
- cancellation reason
- effective date
- penalties
- refund eligibility
- supplier cancellation state

### 15.3 Refund/credit read linkage
Need billing-side projections into travel context.

## APIs
- `POST /travel/bookings/{id}/change-requests`
- `POST /travel/bookings/{id}/cancel`
- `GET /travel/bookings/{id}/change-history`

## Exit criteria
- amendments and cancellations are first-class workflows
- refund/cancellation state is visible and auditable

---

# 4. Architecture alignment guidance

## Keep ownership clear
- travel-service owns CRM and operations workflows
- billing-service owns invoices/payments/refunds as financial truth
- identity-service owns branding and tenant config
- communication-service owns delivery/templates

## Do not violate database-per-service
Travel should consume finance/branding data through:
- APIs
- cached clients
- events/projections

## Prefer projection/read models for cross-domain visibility
Examples:
- booking financial summary
- customer portal checklist state
- pending supplier confirmation dashboard

---

# 5. Recommended execution order

If you want maximum product value from here, do this in order:

1. Phase 9 - Commercial controls / quote approvals
2. Phase 10 - Booking finance visibility
3. Phase 11 - Supplier fulfillment workflow
4. Phase 12 - Customer self-service depth
5. Phase 13 - Tasking/SLA/automation
6. Phase 14 - Search/reporting/export
7. Phase 15 - Change/cancellation/refund workflow

Why this order:
- first prevent bad commercial decisions
- then expose money state at booking level
- then improve actual operations
- then improve customer and internal execution
- then optimize visibility/reporting
- then handle advanced edge-case lifecycle complexity

---

# 6. Definition of done for “serious travel CRM”

Voyara starts to feel like a genuinely serious travel CRM when:
- quotes have governance, not just creation
- bookings show financial truth, not just itinerary truth
- supplier execution is structured and trackable
- customers can act in portal, not just view
- operations run from queues/tasks, not memory
- management can search/report/export without engineering help
- changes/cancellations/refunds are first-class workflows

---

# 7. Final blunt conclusion

The current travel-service is no longer weak.
But it is still strongest at:
- selling the trip
- creating the booking record
- showing history

It is still weaker than a mature travel CRM at:
- governing commercial decisions
- running supplier operations at depth
- exposing finance state in-context
- enabling customer self-service
- giving teams queue/reporting power
- handling ugly real-life amendments and refunds

That is the next real frontier.
