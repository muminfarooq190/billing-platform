# Voyara Travel CRM - Execution Plan for Phases 6 to 8

_Last updated: 2026-04-06_

This file is the **execution version** of the broader production-gap analysis.
It is intentionally more concrete:

- what to build
- in what order
- which tables/entities are needed
- which APIs to add/change
- how to split branches/PRs
- what success looks like at the end of each phase

---

## Delivery strategy

Do **not** try to build all of this in one mega branch.
That would become a cursed swamp.

Recommended structure:

- **Phase 6** = quotation maturity
- **Phase 7** = booking + fulfillment foundation
- **Phase 8** = timeline + audit + activity system

Within each phase, split work into small PRs:

1. domain/data model
2. write APIs
3. read/query APIs
4. document generation / integration behavior
5. tests + docs

---

# Phase 6 - Quotation Maturity

## Goal
Make quotations actually usable by a travel sales team.

Right now quotations are too thin. They need revisions, media, history, and a sendable customer-facing representation.

## Business outcomes

At the end of Phase 6, a salesperson should be able to:

- create a quotation
- revise it multiple times
- attach images/files/media to it
- track quotation status history
- send/share a formatted version
- know which revision the customer accepted

---

## Phase 6A - Quotation revision architecture

## New domain concepts

### 1. `QuotationRevision`
Purpose: immutable snapshot of each quote version.

#### Suggested fields
- `Id`
- `QuotationId`
- `TenantId`
- `RevisionNumber`
- `Status`
- `Title`
- `Destination`
- `TravelDate`
- `ReturnDate`
- `Travellers`
- `Currency`
- `CustomerName`
- `CustomerContactId`
- `Notes`
- `VisibleNotes`
- `InternalNotes`
- `ValidUntil`
- `TotalAmount`
- `SnapshotJson`
- `CreatedByUserId`
- `CreatedAt`

### 2. `QuotationStatusHistory`
Purpose: audit the commercial lifecycle.

#### Suggested fields
- `Id`
- `QuotationId`
- `TenantId`
- `FromStatus`
- `ToStatus`
- `Reason`
- `ChangedByUserId`
- `CreatedAt`

### 3. `QuotationOption` (optional but highly valuable)
Purpose: multiple package options in one proposal.

#### Suggested fields
- `Id`
- `QuotationRevisionId`
- `Title`
- `Description`
- `SortOrder`
- `TotalAmount`
- `Currency`

### 4. `QuotationSection`
Purpose: structured proposal blocks.

#### Suggested fields
- `Id`
- `QuotationRevisionId`
- `Type` (`Summary`, `Inclusions`, `Exclusions`, `Policy`, `Hotel`, `Transport`, `DayPlan`, `Notes`)
- `Title`
- `Content`
- `SortOrder`

---

## Phase 6B - Quotation attachments / images / media

This is critical.

## New domain concept

### `QuotationAttachment`
Purpose: support images, brochures, PDFs, documents.

#### Suggested fields
- `Id`
- `QuotationId`
- `QuotationRevisionId` (nullable if shared across revisions)
- `TenantId`
- `StorageKey`
- `OriginalFileName`
- `ContentType`
- `SizeBytes`
- `AttachmentType` (`Image`, `Pdf`, `Document`, `Brochure`, `Terms`, `Other`)
- `Caption`
- `SortOrder`
- `UploadedByUserId`
- `CreatedAt`
- `DeletedAt`

## Storage strategy

Do not store binary files in Postgres.
Use:
- local filesystem for dev
- object storage abstraction for prod (S3-compatible or Azure Blob later)

## Supporting infrastructure

### `IFileStorage`
Methods:
- `UploadAsync(...)`
- `DeleteAsync(...)`
- `GetSignedReadUrlAsync(...)`
- `GetPublicUrlAsync(...)` (optional)

### Implementations
- `LocalFileStorage`
- later: `S3FileStorage`

---

## Phase 6C - Quote send/share workflow

## New capabilities

### 1. `SendQuotationCommand`
Behavior:
- freeze current revision
- record sent timestamp
- create share token or public link token
- generate customer-visible HTML/PDF if required
- publish event `travel.quotation.sent` (new contract later)
- optionally notify customer

### 2. `GetQuotationPublicViewQuery`
Purpose: render customer-safe view without internal notes.

### 3. `AcceptQuotationCommand`
Behavior:
- accept a specific revision
- lock accepted revision reference on quotation root
- create status history entry
- emit `quotation accepted` event

### 4. `RejectQuotationCommand` / `ExpireQuotationCommand`
Behavior:
- mark outcome with history entry
- preserve revision trail

---

## Phase 6D - API plan

## New/changed travel-service endpoints

### Quotation revisions
- `POST /travel/quotations/{id}/revisions`
- `GET /travel/quotations/{id}/revisions`
- `GET /travel/quotations/{id}/revisions/{revisionId}`

### Quotation history
- `GET /travel/quotations/{id}/history`

### Attachments
- `POST /travel/quotations/{id}/attachments`
- `GET /travel/quotations/{id}/attachments`
- `DELETE /travel/quotations/{id}/attachments/{attachmentId}`

### Send/share
- `POST /travel/quotations/{id}/send`
- `GET /travel/quotations/public/{shareToken}`
- `POST /travel/quotations/{id}/accept`
- `POST /travel/quotations/{id}/reject`

### Read model improvements
List endpoints should include:
- `CurrentRevisionNumber`
- `LastSentAt`
- `AcceptedRevisionNumber`
- `AttachmentCount`
- `HasImages`

---

## Phase 6E - Schema additions

### New tables
- `quotation_revisions`
- `quotation_status_history`
- `quotation_sections`
- `quotation_attachments`
- maybe `quotation_options`
- maybe `quotation_share_links`

### Existing table changes
`quotations` should probably gain:
- `CurrentRevisionNumber`
- `AcceptedRevisionId`
- `LastSentAt`
- `LastViewedAt` (optional)
- `ExpiredAt` (optional)

---

## Phase 6 branch plan

### PR 6.1 - quotation revision data model
- entities
- configs
- migrations
- repository changes

### PR 6.2 - revision/history APIs
- create revision
- list revisions
- history endpoint

### PR 6.3 - attachments/media
- file storage abstraction
- upload endpoints
- attachment queries

### PR 6.4 - send/share/accept workflow
- send command
- public quote view
- accept/reject/expire flow

### PR 6.5 - docs + tests
- integration tests
- sample payloads
- postman/readme updates

---

## Phase 6 exit criteria

Call Phase 6 done only when:

- quotations have revisions
- revision history is queryable
- attachments/images are supported
- a quote can be sent/shared
- accepted quote references a specific revision
- status history is recorded
- tests cover core flows

---

# Phase 7 - Booking & Fulfillment Foundation

## Goal
Turn accepted quotations into real operational travel work.

At the end of Phase 7, the system should move beyond “proposal CRM” into “trip operations CRM.”

---

## Phase 7A - Booking aggregate

## New domain concept

### `Booking`
Purpose: operational entity representing the confirmed commercial trip.

#### Suggested fields
- `Id`
- `TenantId`
- `QuotationId`
- `AcceptedRevisionId`
- `PrimaryContactId`
- `BookingNumber`
- `Status` (`Pending`, `Confirmed`, `PartiallyBooked`, `Ticketed`, `Travelled`, `Cancelled`)
- `TripName`
- `Destination`
- `StartDate`
- `EndDate`
- `TravellersCount`
- `Currency`
- `TotalSellAmount`
- `TotalCostAmount`
- `MarginAmount`
- `AssignedToUserId`
- `CreatedAt`
- `UpdatedAt`
- `CancelledAt`

## New command
- `CreateBookingFromQuotationCommand`

Behavior:
- only allowed from accepted quotation revision
- copy accepted commercial snapshot into booking
- emit booking-created event later

---

## Phase 7B - Traveler / passenger model

## New domain concept

### `Traveler`
Purpose: actual people traveling.

#### Suggested fields
- `Id`
- `BookingId`
- `TenantId`
- `FirstName`
- `LastName`
- `DateOfBirth`
- `Gender` (optional)
- `Email`
- `Phone`
- `PassportNumber`
- `PassportExpiry`
- `Nationality`
- `MealPreference`
- `SpecialAssistanceNotes`
- `EmergencyContact`
- `CreatedAt`
- `UpdatedAt`

This is necessary because contacts are not enough. One customer contact may book for multiple travelers.

---

## Phase 7C - Supplier fulfillment model

## New domain concept

### `BookingItem`
Purpose: operational booking units.

#### Suggested fields
- `Id`
- `BookingId`
- `TenantId`
- `Type` (`Flight`, `Hotel`, `Transfer`, `Visa`, `Insurance`, `Tour`, `Train`, `Other`)
- `SupplierName`
- `SupplierReference`
- `Status`
- `StartAt`
- `EndAt`
- `Location`
- `SellAmount`
- `CostAmount`
- `Currency`
- `Notes`
- `VoucherNumber`
- `CreatedAt`
- `UpdatedAt`

This gives operations something real to manage.

---

## Phase 7D - Booking documents & vouchers

## New domain concept

### `BookingDocument`
Purpose: store vouchers, confirmations, supplier docs, invoices, passports, etc.

#### Suggested fields
- `Id`
- `BookingId`
- `TenantId`
- `StorageKey`
- `DocumentType` (`Voucher`, `Ticket`, `Invoice`, `PassportCopy`, `Visa`, `Insurance`, `Other`)
- `OriginalFileName`
- `ContentType`
- `SizeBytes`
- `VisibleToCustomer`
- `CreatedAt`

---

## Phase 7E - API plan

### Bookings
- `POST /travel/bookings/from-quotation/{quotationId}`
- `GET /travel/bookings`
- `GET /travel/bookings/{id}`
- `PATCH /travel/bookings/{id}/status`
- `POST /travel/bookings/{id}/cancel`

### Travelers
- `POST /travel/bookings/{id}/travelers`
- `PUT /travel/bookings/{id}/travelers/{travelerId}`
- `DELETE /travel/bookings/{id}/travelers/{travelerId}`
- `GET /travel/bookings/{id}/travelers`

### Booking items
- `POST /travel/bookings/{id}/items`
- `PUT /travel/bookings/{id}/items/{itemId}`
- `PATCH /travel/bookings/{id}/items/{itemId}/status`
- `GET /travel/bookings/{id}/items`

### Booking docs
- `POST /travel/bookings/{id}/documents`
- `GET /travel/bookings/{id}/documents`
- `DELETE /travel/bookings/{id}/documents/{documentId}`

---

## Phase 7 schema additions

### New tables
- `bookings`
- `travelers`
- `booking_items`
- `booking_documents`

### Existing linkages
- billing should later link invoices to booking / quotation / traveler scope where relevant
- communication should be able to tag notifications to booking id

---

## Phase 7 branch plan

### PR 7.1 - booking root model
- booking aggregate
- migration
- create-from-quotation command

### PR 7.2 - travelers
- traveler entity + APIs

### PR 7.3 - booking items / supplier ops
- booking items + statuses

### PR 7.4 - booking documents
- docs + voucher storage metadata

### PR 7.5 - read models + tests
- list/detail queries
- ops-focused read models
- tests

---

## Phase 7 exit criteria

Call Phase 7 done only when:

- an accepted quotation can become a booking
- bookings have travelers
- bookings have supplier/ops items
- booking statuses exist
- documents/vouchers can be attached
- operations can manage a trip beyond just editing itinerary text

---

# Phase 8 - Timeline, Audit, and Activity System

## Goal
Make the platform trustworthy, explainable, and supportable.

This phase is what turns “records in tables” into a real CRM memory system.

---

## Phase 8A - Unified activity timeline

## New domain concept

### `ActivityEntry`
Purpose: event/timeline feed for CRM records.

#### Suggested fields
- `Id`
- `TenantId`
- `EntityType` (`Contact`, `Quotation`, `Booking`, `Itinerary`, `FollowUp`, `Notification`, `Invoice`)
- `EntityId`
- `ActivityType` (`Created`, `Updated`, `StatusChanged`, `Sent`, `Viewed`, `Accepted`, `Rejected`, `CommentAdded`, `ReminderTriggered`, `PaymentReceived`, `DocumentUploaded`)
- `Summary`
- `DetailJson`
- `ActorUserId`
- `OccurredAt`

## Timeline sources
Generate entries from:
- commands
- domain events
- important system jobs
- communication events

---

## Phase 8B - Audit log

## New domain concept

### `AuditLog`
Purpose: immutable compliance/support trail.

#### Suggested fields
- `Id`
- `TenantId`
- `EntityType`
- `EntityId`
- `Action`
- `ActorUserId`
- `BeforeJson`
- `AfterJson`
- `IpAddress`
- `UserAgent`
- `OccurredAt`

Use this for critical mutations:
- quote revision created
- quote status changed
- booking cancelled
- traveler changed
- payment state changed
- user role changed

---

## Phase 8C - Notes/comments system

## New domain concept

### `EntityNote`
Purpose: comments and collaboration.

#### Suggested fields
- `Id`
- `TenantId`
- `EntityType`
- `EntityId`
- `Visibility` (`Internal`, `CustomerVisible`)
- `Content`
- `CreatedByUserId`
- `CreatedAt`
- `UpdatedAt`
- `DeletedAt`

This matters because teams need collaboration context, not just data records.

---

## Phase 8D - API plan

### Timeline
- `GET /travel/timeline/{entityType}/{entityId}`
- maybe `GET /travel/contacts/{id}/timeline`
- maybe `GET /travel/quotations/{id}/timeline`
- maybe `GET /travel/bookings/{id}/timeline`

### Notes
- `POST /travel/{entityType}/{entityId}/notes`
- `GET /travel/{entityType}/{entityId}/notes`
- `PUT /travel/notes/{noteId}`
- `DELETE /travel/notes/{noteId}`

### Audit
Admin/internal only:
- `GET /admin/audit/{entityType}/{entityId}`

---

## Phase 8 schema additions

### New tables
- `activity_entries`
- `audit_logs`
- `entity_notes`

### Optional indexing
Add indexes on:
- `(tenant_id, entity_type, entity_id, occurred_at desc)`
- `(tenant_id, actor_user_id, occurred_at desc)`

---

## Phase 8 branch plan

### PR 8.1 - activity/timeline model
- table + write pipeline
- timeline query API

### PR 8.2 - audit infrastructure
- audit capture helpers
- critical mutation capture

### PR 8.3 - notes/comments
- entity notes model + APIs

### PR 8.4 - communication timeline integration
- attach notification entries to CRM entities where possible

### PR 8.5 - tests + admin docs
- integration tests
- support/admin guidance

---

## Phase 8 exit criteria

Call Phase 8 done only when:

- quotes/bookings/contacts have timeline visibility
- critical changes have audit entries
- team notes/comments exist
- support/admin can explain what happened to a record

---

# Cross-phase technical rules

These rules apply across Phases 6 to 8.

## 1. Tenant enforcement

Every new endpoint must use server-derived tenant context.
Do not reintroduce trust in client-supplied tenant IDs where avoidable.

## 2. Files are metadata in DB, binaries in storage

Do not stuff documents into Postgres rows.

## 3. Public quote/share views must be safe

Customer-facing quote views must never leak:
- internal notes
- margin/cost values
- internal attachments
- audit data

## 4. Revision snapshots should be immutable

Once created, a revision is history, not mutable working state.

## 5. Add integration tests for every new lifecycle handoff

At minimum:
- quote revision creation
- quote send
- quote accept
- booking creation from quote
- traveler creation
- document attachment metadata
- timeline event generation

---

# Recommended branch names

## Phase 6
- `feat/phase-6-quotation-revisions`
- `feat/phase-6-quotation-attachments`
- `feat/phase-6-quotation-send-share`

## Phase 7
- `feat/phase-7-bookings-core`
- `feat/phase-7-travelers`
- `feat/phase-7-booking-documents`

## Phase 8
- `feat/phase-8-activity-timeline`
- `feat/phase-8-audit-log`
- `feat/phase-8-entity-notes`

---

# Suggested order of execution

If you want the highest product value fastest, do this exact order:

1. Phase 6A - quotation revisions
2. Phase 6B - quotation attachments/images
3. Phase 6C - send/share/accept flow
4. Phase 7A - booking from accepted quote
5. Phase 7B - traveler records
6. Phase 8A - timeline
7. Phase 8B - audit log
8. Phase 7C/7D - supplier ops + documents
9. Phase 8C - notes/comments

Why this order:
- first fix the sales artifact (quote)
- then create the confirmed-trip handoff
- then make the whole thing observable and trustworthy

---

# Final blunt summary

If Voyara wants to feel like a real travel CRM SaaS and not a generic microservices demo with travel nouns pasted on it, the next serious work should be:

- quotation revisions
- quotation attachments/images/documents
- accepted quote -> booking lifecycle
- timeline/audit system

Everything else is secondary until those exist.
