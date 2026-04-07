# Voyara Travel CRM - Production MVP Gap Analysis & Delivery Roadmap

_Last updated: 2026-04-06_

## Executive Summary

Voyara has made strong progress from a generic billing/microservices skeleton into a recognizable travel CRM foundation. The platform now has:

- tenant + user auth foundations
- billing/subscription/invoice services
- travel contacts, quotations, itineraries, and follow-ups
- communication templates + notifications
- webhook delivery infrastructure
- gateway routing, CORS, and readiness checks
- pagination/filtering on several read endpoints
- tenant-context enforcement on major controller entry points

That said, this is **not yet a production-ready SaaS travel CRM MVP**.

It is closer to a **technical foundation / internal alpha** than a production MVP.

The biggest gap is that the current system models travel CRM as CRUD over a few aggregates, while real travel CRM products are built around:

- **lead -> quote -> revision -> approval -> booking -> fulfillment -> payment -> support** lifecycle
- document-rich workflows (files, proposal PDFs, itinerary exports, vouchers, images, attachments)
- auditability and history at every commercial step
- operational status tracking beyond simple record editing
- role-based access and tenant-safe authorization across all services
- customer communication timelines, reminders, and approval trails

## Overall Verdict

### Current readiness level

- **Engineering foundation:** moderate
- **Feature completeness for travel CRM MVP:** low-to-moderate
- **Production SaaS readiness:** low
- **Travel operations readiness:** low

### Blunt assessment

If this were shown to a real travel agency, DMC, OTA back-office, or tour operator today:

- they could see the direction
- they could maybe demo a few flows
- but they could **not safely run daily business on it yet**

## What is already present

## Identity / SaaS basics

- tenant registration and auth
- refresh tokens / JWKS
- user CRUD
- user timestamps
- tenant-context scaffolding

## Travel service

- contacts
- quotations
- itineraries
- follow-ups
- pagination/filtering on list endpoints

## Billing

- subscriptions
- invoices
- invoice line items persisted
- list pagination
- dashboard

## Communication

- notifications
- templates
- recipient preferences
- unread count / mark-as-read

## Webhooks / integration

- subscription endpoints
- delivery logs
- replay support
- shared event contracts started

## Production-ish plumbing

- migrations instead of EnsureCreated / synchronize
- gateway communication route
- CORS
- health + readiness checks
- Swagger in main .NET services

---

## Major product gaps still blocking a true travel CRM MVP

## 1. Quotation system is too thin for real travel sales

This is the biggest product gap.

### Missing capabilities

- **quotation revision history / versioning**
- draft vs sent vs viewed vs expired vs approved workflow depth
- approval comments / negotiation trail
- branded quote output (PDF / shareable link)
- quotation snapshot at send time
- quotation comparison between revisions
- option sets / alternative itineraries within one proposal
- validity windows with auto-expiry behavior
- quote ownership / salesperson attribution
- profit / margin visibility per quote

### Specific gap you called out

Yes: **there are no image columns / media attachments for quotations**.

For a travel CRM, this matters because quotes often include:

- destination images
- hotel images
- room thumbnails
- package banners
- brochure pages / PDFs
- visa / inclusion / exclusion attachments

A production travel CRM normally supports either:

1. `quotation_attachments` / `quotation_media` table, or
2. reusable media library with references from quote sections / itinerary blocks

### Additional missing quote-level structures

- quote sections / blocks
- inclusions / exclusions
- cancellation policy text
- payment schedule terms
- supplier references / notes
- internal notes vs customer-visible notes
- quote status history

## 2. No booking / confirmed trip lifecycle

A real travel CRM does not end at quotation acceptance.

### Missing post-sale domain

- bookings / reservations aggregate
- passenger / traveler records
- booking references / PNRs / voucher numbers
- supplier bookings (hotel, flight, transport, tour)
- payment milestones / deposit / final balance
- travel documents / vouchers / confirmations
- rooming lists / occupancy / passport details
- visa / insurance / special assistance tracking

Current itineraries are useful, but they are **not yet equivalent to fulfillment operations**.

## 3. No timeline / activity history / auditability

There is no complete customer or quote timeline.

### Missing

- quotation history entries
- contact activity timeline
- notification history attached to CRM records
- who changed what and when
- before/after snapshots for sensitive commercial data
- audit trail for status changes
- login/security audit trail
- webhook / communication timeline surfaced per customer or quote

Real SaaS travel CRMs usually have a unified timeline like:

- contact created
- quote v1 created
- quote sent
- customer replied
- follow-up assigned
- quote revised to v2
- quote accepted
- deposit invoice issued
- payment received
- itinerary confirmed
- voucher emailed

That layer is currently missing.

## 4. No files / documents / media subsystem

A production travel CRM needs a first-class file system.

### Missing

- file uploads
- object storage integration
- attachment metadata table
- signed URLs / access control
- per-tenant storage separation
- image thumbnails / previews
- document classification
- quote PDF generation output storage
- itinerary brochure export
- voucher / invoice / receipt storage

Without this, the product stays text-first while the travel industry is document-heavy.

## 5. Missing lead pipeline and CRM sales workflow

Contacts exist, but a true CRM requires pipeline mechanics.

### Missing

- leads / enquiries
- lead stages
- source attribution (website, WhatsApp, referral, ads, repeat customer)
- owner / assignment rules
- SLA / first response tracking
- lost reason tracking
- conversion metrics
- tasks / reminders tied to pipeline stages
- opportunity amount forecasting

Right now the platform feels more like records + quote objects than an actual sales CRM.

## 6. Tenant enforcement is improved but not fully mature

Tenant context exists and several controllers were corrected.

### Still needed for production-grade safety

- enforce tenant context consistently in **all** tenant-scoped controllers and background consumers
- verify repository/query layer also filters by tenant where required
- remove request-body `tenantId` from contracts where it should no longer be client-supplied
- middleware / auth policy to standardize tenant extraction instead of scattered controller assumptions
- negative tests for cross-tenant access attempts

## 7. Authorization model is too light

Travel SaaS usually needs richer permissions than Owner/Admin/Member.

### Missing

- role/permission matrix
- branch / team scoping
- sales vs ops vs finance vs support permissions
- field-level / action-level restrictions
- approval permissions
- export/download restrictions
- audit access permissions

## 8. Communication is not yet CRM-thread aware

Notifications exist, but communications are not yet modeled as a unified customer conversation stream.

### Missing

- conversation timeline per contact / trip / quote
- inbound email capture
- WhatsApp / chat transcript linkage
- quote send/open tracking
- templated quote/itinerary share workflows
- reminder automation rules
- communication-to-record linking

## 9. Reporting is still too shallow for SaaS travel ops

### Missing

- sales funnel metrics
- quote conversion rate
- agent productivity dashboards
- destination-wise revenue
- booking margin analytics
- overdue follow-up analytics
- repeat customer analytics
- payment collection reports
- operational SLA dashboards

## 10. Operational reliability gaps remain

### Missing / partially missing

- stronger CI coverage for new multi-service flows
- end-to-end integration tests across gateway -> service -> db
- contract tests for shared events
- seed/demo workflows for travel CRM scenarios
- background job observability for delivery failures / retries / dead-letter analysis
- stronger config validation and startup fail-fast checks
- backup/restore guidance
- secrets management guidance
- rate limiting / abuse controls by tenant and endpoint

---

## Travel-service specific gap analysis

This is where the largest product delta remains.

## Current travel-service strengths

- basic CRUD/read flows exist for contacts, quotations, itineraries, follow-ups
- list filtering and pagination are in place
- data model has enough shape to continue building on

## Missing travel-service MVP-critical items

### Quotations

- **quotation version history**
- quote revisions with immutable snapshots
- sent/viewed/accepted timeline events
- attachments/images/media
- customer-facing formatted quote output
- optional packages / multiple quote options
- line-item tax/discount/margin structures
- supplier references and cost vs sell price

### Itineraries

- customer-facing itinerary rendering/export
- itinerary versions / history
- day-level media attachments
- booking/voucher linkage
- traveler-level personalization

### Follow-ups

- recurring reminders
- reminder automations
- ownership rules / SLA breach indicators
- activity log integration

### Contacts

- richer profile fields (passport, nationality, DOB, preferences, loyalty, tags by type)
- linked trips / quotes / payments timeline
- company / corporate account hierarchy
- duplicate detection / merge flow

### General travel operations

- booking entity
- traveler / passenger entity
- supplier entity
- task entity
- notes entity with internal/public visibility
- attachments entity
- audit / history entity

---

## What real production travel CRM SaaS products usually include

Based on current market patterns across travel CRM / agency OS products, production-ready systems usually combine:

## Sales CRM layer

- lead capture
- Kanban pipeline
- assignment / round-robin / SLA
- quote creation and revision
- quote PDF / branded links
- reminders and follow-ups

## Operations layer

- itinerary builder
- booking management
- supplier tracking
- passenger data
- task checklists
- travel document/voucher generation

## Finance layer

- invoices
- deposits / installments
- vendor payments
- margins / commissions
- refunds / credit notes

## Communication layer

- email templates
- WhatsApp / SMS / push / in-app notifications
- conversation timeline
- quote send/open reminders

## Governance layer

- audit trail
- role permissions
- tenant isolation
- backups
- observability
- support tooling

Voyara currently covers pieces of the finance and platform layers, and the skeleton of CRM/operations, but not the full end-to-end business workflow.

---

## Recommended new phase plan

Below is a better phase plan for taking Voyara from current state to a **production-level travel CRM SaaS MVP**.

## Phase 6 - Travel Quote System Maturity (Highest product priority)

### Goal
Make quotations genuinely usable by travel sales teams.

### Deliverables
- quotation revisions / version history
- quotation status history table
- quote snapshots at send time
- quote attachments / images / brochure media
- inclusions / exclusions / terms / policies
- internal notes vs client-visible notes
- customer-facing quote share endpoint
- PDF quote generation pipeline
- quote accepted / rejected / expired flow
- quote analytics basics

### Why first
This is the most obvious missing product capability and directly affects whether travel teams can sell with the platform.

## Phase 7 - Booking & Fulfillment Core

### Goal
Bridge the gap between accepted quotation and actual trip operations.

### Deliverables
- booking aggregate
- traveler/passenger records
- supplier/service booking records
- voucher / confirmation artifacts
- booking status workflow
- payment milestone linkage
- internal operations checklist

### Why
Without this, quotation acceptance has nowhere meaningful to go.

## Phase 8 - Timeline, Audit, and Activity System

### Goal
Create a single source of truth for who did what and what happened to each customer/trip.

### Deliverables
- activity/event timeline model
- audit log table(s)
- quote history entries
- contact timeline
- communication linkage to CRM records
- user action tracking
- admin audit view

### Why
A SaaS CRM without history becomes untrustworthy very fast.

## Phase 9 - Files, Media, and Document Generation

### Goal
Add the document-heavy layer real travel businesses need.

### Deliverables
- file upload service / storage abstraction
- attachment entity model
- quotation media support
- itinerary exports / brochures
- invoice/receipt/voucher document storage
- signed download URLs
- image thumbnail support

### Why
Travel products are image/document heavy. This is not optional for real customer usage.

## Phase 10 - Lead Pipeline & Sales Operations

### Goal
Turn the system into a real CRM, not just records + quotes.

### Deliverables
- lead entity
- lead stages / Kanban
- assignment / ownership
- source attribution
- lost reasons
- SLA timers
- reminders/tasks linked to deals
- conversion analytics

## Phase 11 - Authorization & Tenant Security Hardening

### Goal
Make multi-tenant operation trustworthy.

### Deliverables
- permission matrix
- policy-based authorization
- branch/team access scopes
- negative tenancy tests
- request contract cleanup to remove client-supplied tenant IDs where inappropriate
- service-wide tenant enforcement audit
- admin boundary review

## Phase 12 - Communication Timeline & Automation

### Goal
Connect CRM operations to customer communication.

### Deliverables
- communication timeline per contact / quote / booking
- outbound event templates for travel lifecycle moments
- reminder automation engine
- quote send / read / chase flows
- omnichannel integration roadmap

## Phase 13 - Analytics, SaaS Ops, and Enterprise Readiness

### Goal
Make the MVP operationally deployable and commercially useful.

### Deliverables
- sales dashboards
- conversion and revenue analytics
- support/admin tooling
- tenant provisioning polish
- backup/restore runbooks
- rate limits / abuse protection
- SLOs / monitoring dashboards
- production deployment docs

---

## Immediate priority backlog (shortlist)

If the goal is "closest path to a production MVP" rather than broad platform polish, do these next in order:

1. **Quotation revisions/history**
2. **Quotation attachments/images/PDF output**
3. **Booking aggregate + accepted quote handoff**
4. **Timeline/audit log**
5. **Traveler/passenger + document model**
6. **Tenant/security cleanup across all services**
7. **Communication timeline + quote send workflow**

---

## Recommended success criteria for calling it a production MVP

Do **not** call Voyara production MVP ready until all of these are true:

- a lead/contact can become a quote
- a quote supports revisions and history
- a quote can be sent in branded customer-facing form
- quote assets/images/documents are supported
- accepted quote becomes a booking/confirmed trip workflow
- payments and invoices can be linked to that lifecycle
- all key records have timeline/audit visibility
- all tenant-scoped APIs are enforced by context, not caller input
- build/test/CI and operational docs are green and repeatable

---

## Final blunt conclusion

Voyara is **promising**, but today it is **not yet a production-ready SaaS travel CRM MVP**.

The platform foundation is good enough to continue from.
The product layer — especially in travel-service — still needs major work around:

- quote maturity
- attachments/images/documents
- history/auditability
- booking fulfillment
- operational workflow depth

If the next effort focuses on those instead of more generic platform polish, the product can move from "technical prototype" toward "real travel CRM MVP" much faster.
