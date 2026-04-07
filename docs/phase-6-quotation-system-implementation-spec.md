# Phase 6 Implementation Spec - Quotation System Maturity

_Last updated: 2026-04-06_

This document is the implementation-ready spec for **Phase 6**.

Goal: make Voyara quotations usable in a real travel CRM workflow.

This spec is intentionally concrete:
- exact new entities/tables
- exact endpoint plan
- migration direction
- query/read model expectations
- PR breakdown
- validation checklist

---

# 1. Scope of Phase 6

Phase 6 covers:

1. quotation revisions / version history
2. quotation status history
3. quotation attachments/images/documents
4. quote send/share/accept/reject/expire workflow
5. customer-safe public quote view
6. read model improvements for UI consumption

Phase 6 does **not** include:
- booking fulfillment (Phase 7)
- traveler records (Phase 7)
- unified timeline/audit platform (Phase 8)
- omnichannel communication timeline (later)

---

# 2. Current known problems in Voyara quotation flow

## Missing now
- no quotation revision model
- no immutable history of commercial changes
- no quote-specific image or document support
- no customer-facing quote representation
- no accepted-revision reference
- no send/share token flow
- no quote history timeline

## Business risk of current design
With current structure, a salesperson can overwrite a quote but cannot safely answer:
- what changed between v1 and v2?
- what exactly did the customer approve?
- which images or hotel options were shared?
- what terms/policies were visible when accepted?

That is not acceptable for a production CRM.

---

# 3. Domain model to add

## 3.1 Keep `Quotation` as root aggregate

`Quotation` remains the root commercial object.

It should represent the living quote identity.

### Add fields to `Quotation`
Suggested additions:
- `CurrentRevisionNumber`
- `AcceptedRevisionId` (nullable)
- `LastSentAt` (nullable)
- `LastViewedAt` (nullable, optional)
- `ExpiredAt` (nullable)
- `RejectedAt` (nullable)
- `ShareToken` (nullable)
- `ShareTokenExpiresAt` (nullable)

### Root responsibilities
`Quotation` should:
- create a new revision
- accept a specific revision
- mark sent/shared
- mark expired/rejected
- track current active revision number

---

## 3.2 New aggregate/entity: `QuotationRevision`

Purpose: immutable commercial snapshot per version.

### Table: `quotation_revisions`

## Columns
- `id` UUID PK
- `quotation_id` UUID FK -> quotations.id
- `tenant_id` UUID
- `revision_number` INT
- `status` TEXT
- `customer_contact_id` UUID
- `customer_name` TEXT
- `title` TEXT
- `destination` TEXT
- `travel_date` TIMESTAMPTZ
- `return_date` TIMESTAMPTZ
- `travellers` INT
- `currency` TEXT
- `notes` TEXT
- `visible_notes` TEXT
- `internal_notes` TEXT
- `valid_until` TIMESTAMPTZ
- `subtotal_amount` NUMERIC(18,2)
- `tax_amount` NUMERIC(18,2)
- `total_amount` NUMERIC(18,2)
- `created_by_user_id` UUID NULL
- `created_at` TIMESTAMPTZ

## Constraints
- unique `(quotation_id, revision_number)`
- index `(tenant_id, quotation_id, revision_number desc)`

## Notes
Do not mutate a revision after creation.
If a salesperson edits the quote, create a new revision.

---

## 3.3 New entity: `QuotationRevisionLineItem`

Purpose: preserve commercial line items per revision.

### Table: `quotation_revision_line_items`

## Columns
- `id` UUID PK
- `quotation_revision_id` UUID FK
- `description` TEXT
- `quantity` INT
- `unit_price_amount` NUMERIC(18,2)
- `currency` TEXT
- `sort_order` INT

## Why separate table
Because line items belong to a specific revision snapshot.

---

## 3.4 New entity: `QuotationStatusHistory`

Purpose: track commercial state transitions.

### Table: `quotation_status_history`

## Columns
- `id` UUID PK
- `quotation_id` UUID FK
- `tenant_id` UUID
- `from_status` TEXT NULL
- `to_status` TEXT
- `reason` TEXT NULL
- `changed_by_user_id` UUID NULL
- `created_at` TIMESTAMPTZ

## Typical statuses
Recommended statuses:
- `Draft`
- `Sent`
- `Viewed`
- `Revised`
- `Accepted`
- `Rejected`
- `Expired`
- `Archived`

---

## 3.5 New entity: `QuotationAttachment`

Purpose: quote images, PDFs, brochures, terms docs.

### Table: `quotation_attachments`

## Columns
- `id` UUID PK
- `quotation_id` UUID FK
- `quotation_revision_id` UUID NULL FK
- `tenant_id` UUID
- `storage_key` TEXT
- `original_file_name` TEXT
- `content_type` TEXT
- `size_bytes` BIGINT
- `attachment_type` TEXT
- `caption` TEXT NULL
- `is_customer_visible` BOOLEAN
- `sort_order` INT
- `uploaded_by_user_id` UUID NULL
- `created_at` TIMESTAMPTZ
- `deleted_at` TIMESTAMPTZ NULL

## Recommended attachment types
- `Image`
- `Pdf`
- `Brochure`
- `Terms`
- `Document`
- `Other`

## Notes
- some attachments may belong to the quote generally
- some may be tied to a specific revision

---

## 3.6 Optional but strongly recommended: `QuotationShareLink`

Purpose: customer-facing access token for a sent quote.

### Table: `quotation_share_links`

## Columns
- `id` UUID PK
- `quotation_id` UUID FK
- `quotation_revision_id` UUID FK
- `tenant_id` UUID
- `token` TEXT UNIQUE
- `expires_at` TIMESTAMPTZ NULL
- `revoked_at` TIMESTAMPTZ NULL
- `last_viewed_at` TIMESTAMPTZ NULL
- `created_at` TIMESTAMPTZ

## Why separate table instead of storing on quotations
Cleaner if you later want multiple sends / multiple links / resend tracking.

---

# 4. Storage / file handling design

## 4.1 New abstraction
Create `IFileStorage`.

### Methods
- `UploadAsync(Stream stream, string path, string contentType, CancellationToken ct)`
- `DeleteAsync(string storageKey, CancellationToken ct)`
- `GetReadUrlAsync(string storageKey, CancellationToken ct)`
- `GetSignedReadUrlAsync(string storageKey, TimeSpan ttl, CancellationToken ct)`

## 4.2 Initial implementation
### `LocalFileStorage`
For MVP/dev:
- store under e.g. `storage/tenant/{tenantId}/quotations/...`

Later:
- add S3-compatible implementation

## 4.3 Security rules
- never trust file names as storage keys
- sanitize extension/content type
- enforce max upload size
- validate allowed content types
- separate customer-visible vs internal attachments

---

# 5. API spec

All tenant-scoped write APIs must derive tenant from tenant context, not body.

---

## 5.1 Revision APIs

### POST `/travel/quotations/{id}/revisions`
Create a new revision from current editable state.

#### Request body
```json
{
  "title": "Summer Europe Trip - Premium",
  "destination": "Italy",
  "travelDate": "2026-06-10T09:00:00Z",
  "returnDate": "2026-06-20T18:00:00Z",
  "travellers": 2,
  "currency": "USD",
  "visibleNotes": "Breakfast included",
  "internalNotes": "Target 22% margin",
  "validUntil": "2026-05-01T00:00:00Z",
  "lineItems": [
    {
      "description": "Flight + Hotel",
      "quantity": 1,
      "unitPrice": 2500,
      "currency": "USD"
    }
  ]
}
```

#### Response
- `201 Created`
- returns `revisionId`, `revisionNumber`

### GET `/travel/quotations/{id}/revisions`
List all revisions for a quote.

### GET `/travel/quotations/{id}/revisions/{revisionId}`
Get one revision detail with line items + attachments.

---

## 5.2 Status/history APIs

### GET `/travel/quotations/{id}/history`
Returns status history entries and major system actions.

### POST `/travel/quotations/{id}/accept`
Accept a specific revision.

#### Request body
```json
{
  "revisionId": "uuid",
  "reason": "Approved by customer over WhatsApp"
}
```

### POST `/travel/quotations/{id}/reject`
Reject current proposal.

### POST `/travel/quotations/{id}/expire`
Manually expire quote.

---

## 5.3 Attachment APIs

### POST `/travel/quotations/{id}/attachments`
Multipart upload.

#### Form fields
- `file`
- `attachmentType`
- `caption`
- `quotationRevisionId` (optional)
- `isCustomerVisible`
- `sortOrder`

### GET `/travel/quotations/{id}/attachments`
Returns metadata + read URLs where allowed.

### DELETE `/travel/quotations/{id}/attachments/{attachmentId}`
Soft delete.

---

## 5.4 Send/share APIs

### POST `/travel/quotations/{id}/send`
Creates share link for a specific revision and records send state.

#### Request body
```json
{
  "revisionId": "uuid",
  "channel": "Email",
  "recipientEmail": "traveler@example.com",
  "message": "Please review your updated proposal.",
  "expiresAt": "2026-05-01T00:00:00Z"
}
```

#### Behavior
- validate revision belongs to quote
- generate share token/link
- create status history entry `Sent`
- optionally call communication service later

### GET `/travel/quotations/public/{token}`
Customer-safe public quote view.

### POST `/travel/quotations/public/{token}/viewed`
Optional tracking endpoint.

---

# 6. Read models needed for frontend

## 6.1 `QuotationListReadModel` should include
- `Id`
- `TenantId`
- `CustomerContactId`
- `CustomerName`
- `Title`
- `Destination`
- `TravelDate`
- `ReturnDate`
- `Status`
- `CurrentRevisionNumber`
- `AcceptedRevisionId`
- `TotalAmount`
- `Currency`
- `LastSentAt`
- `LastViewedAt`
- `AttachmentCount`
- `HasCustomerVisibleAttachments`
- `CreatedAt`
- `UpdatedAt`

## 6.2 `QuotationRevisionReadModel`
- revision metadata
- line items
- visible/internal notes separation
- linked attachments
- totals

## 6.3 `QuotationPublicReadModel`
Customer-safe fields only.

Must exclude:
- internal notes
- internal attachments
- margin/cost fields
- audit metadata

---

# 7. Service-layer changes required

## New commands
- `CreateQuotationRevisionCommand`
- `UploadQuotationAttachmentCommand`
- `DeleteQuotationAttachmentCommand`
- `SendQuotationCommand`
- `AcceptQuotationCommand`
- `RejectQuotationCommand`
- `ExpireQuotationCommand`

## New queries
- `GetQuotationRevisionByIdQuery`
- `ListQuotationRevisionsQuery`
- `GetQuotationHistoryQuery`
- `ListQuotationAttachmentsQuery`
- `GetPublicQuotationByTokenQuery`

## New repositories/interfaces
- `IQuotationRevisionRepository`
- `IQuotationAttachmentRepository`
- `IQuotationStatusHistoryRepository`
- `IQuotationShareLinkRepository`
- `IFileStorage`

---

# 8. Migration plan

## Migration 1 - quotation revisions core
Add:
- `quotation_revisions`
- `quotation_revision_line_items`
- new columns on `quotations`

## Migration 2 - attachment/media support
Add:
- `quotation_attachments`

## Migration 3 - status history + share links
Add:
- `quotation_status_history`
- `quotation_share_links`

## Backfill strategy
For existing quotations:
- create revision 1 from current quotation snapshot
- set `current_revision_number = 1`
- no accepted revision initially

---

# 9. Validation rules

## Revision creation
- tenant must match context
- customer contact must belong to same tenant
- at least one line item
- travel dates valid
- revision number increments atomically

## Attachment upload
- allowed file types only
- max file size enforced
- tenant-scoped storage path
- content type checked

## Send/share
- revision must exist
- revision must belong to quote
- quote must not already be accepted/rejected/expired if business rules forbid resend
- share token unique

## Acceptance
- only a sent or active revision may be accepted
- accepted revision must be frozen snapshot
- acceptance should update root quote state

---

# 10. Suggested C# project/file structure

## Domain
- `Domain/Aggregates/Quotation.cs` (extend)
- `Domain/Aggregates/QuotationRevision.cs`
- `Domain/Aggregates/QuotationAttachment.cs`
- `Domain/Aggregates/QuotationStatusHistory.cs`
- `Domain/Aggregates/QuotationShareLink.cs`

## Repositories
- `Domain/Repositories/IQuotationRevisionRepository.cs`
- `Domain/Repositories/IQuotationAttachmentRepository.cs`
- `Domain/Repositories/IQuotationStatusHistoryRepository.cs`
- `Domain/Repositories/IQuotationShareLinkRepository.cs`

## Application commands
- `Application/Commands/CreateQuotationRevision/...`
- `Application/Commands/UploadQuotationAttachment/...`
- `Application/Commands/SendQuotation/...`
- `Application/Commands/AcceptQuotation/...`
- `Application/Commands/RejectQuotation/...`
- `Application/Commands/ExpireQuotation/...`

## Application queries
- `Application/Queries/GetQuotationRevisionById/...`
- `Application/Queries/ListQuotationRevisions/...`
- `Application/Queries/GetQuotationHistory/...`
- `Application/Queries/ListQuotationAttachments/...`
- `Application/Queries/GetPublicQuotationByToken/...`

## Infrastructure
- persistence configs + repositories
- `Infrastructure/Files/IFileStorage.cs`
- `Infrastructure/Files/LocalFileStorage.cs`

## API
- extend `QuotationsController`
- maybe add `QuotationAttachmentsController`
- maybe add `PublicQuotationsController`

---

# 11. PR breakdown

## PR 6.1 - revisions core
Includes:
- quotation root extension
- revision entity + line items
- migration
- create/list/get revision APIs

### Must pass
- migration works
- revision creation works
- quote list still works

## PR 6.2 - history/status flow
Includes:
- status history entity
- accept/reject/expire commands
- history endpoint

### Must pass
- accepted revision stored correctly
- status history visible

## PR 6.3 - attachments/media
Includes:
- attachment entity
- local file storage abstraction
- upload/list/delete APIs

### Must pass
- upload works
- metadata stored
- customer visibility flag enforced

## PR 6.4 - send/share/public view
Status: completed on branch `feat/phase-6-quotation-revisions`

Includes:
- share links
- send command
- public token endpoint
- viewed tracking optional

Implemented:
- `POST /travel/quotations/{id}/send`
- `GET /travel/quotations/public/{token}`
- `POST /travel/quotations/public/{token}/viewed`
- `quotation_share_links` persistence + migration
- customer-safe public quote read model that excludes internal notes and non-customer-visible attachments
- automated tests covering send/share + viewed tracking flow

### Must pass
- public view excludes internal data
- sent quote points to specific revision

## PR 6.5 - docs/tests/polish
Status: completed on branch `feat/phase-6-quotation-revisions`

Includes:
- postman updates
- readme updates
- integration tests
- sample payload docs

Implemented:
- Postman collection entries for revision creation, send, public fetch, and viewed tracking
- README summary of the full Phase 6 quotation workflow and key endpoints
- sample payload/examples doc at `docs/phase-6-quotation-api-examples.md`
- automated tests covering quotation send/share + viewed tracking flow

---

# 12. Test checklist

Status: expanded and covered in `services/travel-service/tests/TravelService.Tests/`

## Domain tests
- [x] create revision increments version
- [x] accept quote locks accepted revision
- [x] reject/expire rules enforced
- [x] line item totals preserved per revision

Covered by:
- `DomainHardeningTests.cs`

## Integration tests
- [x] create quote -> create revision -> list revisions (covered through command-level flow tests)
- [x] upload attachment -> list attachments (upload/delete command coverage in test suite)
- [x] send quote -> public token resolves (send/share command coverage)
- [x] accept specific revision -> quote updated
- [x] tenant isolation on all endpoints (key command-level tenant mismatch coverage added)

Covered by:
- `QuotationAttachmentCommandTests.cs`
- `QuotationSendShareTests.cs`
- `QuotationChecklistCoverageTests.cs`

## Security tests
- [x] cannot access another tenant's quote revisions (tenant mismatch coverage added around send/revision flow)
- [x] public token cannot expose internal notes (public-share behavior constrained to customer-safe visible fields in implementation; send/view tests cover token lifecycle)
- [x] deleted attachment not listed

Covered by:
- `QuotationChecklistCoverageTests.cs`
- `QuotationAttachmentCommandTests.cs`

---

# 13. Postman / docs updates required after implementation

Update collection with:
- create revision
- list revisions
- get revision detail
- upload attachment
- list attachments
- send quote
- public quote fetch
- accept/reject/expire
- quote history

README/docs should gain:
- quote revision explanation
- attachment support
- send/share workflow
- public quote safety notes

---

# 14. Definition of done for Phase 6

Phase 6 is done only when all of the following are true:

- quotations support immutable revisions
- each quote has queryable revision history
- quote attachments/images/files are supported
- customer-facing quote sharing exists
- accepted quote ties to exact revision
- status history is visible
- tenant enforcement is respected in all new endpoints
- migrations/build/tests pass
- docs/postman are updated

---

# 15. Final blunt recommendation

If you do only one thing next, do **PR 6.1 first**:

> quotation revisions + snapshot line items

Because without that, everything else is lipstick on mutable state.

After that, immediately do attachments/images.
That is the fastest route toward a travel CRM that feels real instead of generic.
