# Phase 8 Implementation Spec - Timeline, Audit, and Notes System

_Last updated: 2026-04-06_

This document is the implementation-ready spec for **Phase 8**.

Goal: make Voyara explainable, supportable, and trustworthy by introducing:

1. activity timeline
2. audit logging
3. notes/comments
4. CRM event linking across key entities

This phase is what transforms the platform from “a set of records” into “a system with memory.”

---

# 1. Scope of Phase 8

Phase 8 covers:

- timeline/activity feed for CRM entities
- immutable audit log for sensitive mutations
- notes/comments system for collaboration
- integration of major domain events into timeline entries
- admin/support-facing visibility into record changes

Phase 8 does **not** include:
- advanced BI/reporting warehouse
- chat/helpdesk inbox
- external email ingestion
- enterprise compliance exports (later)

---

# 2. Why this phase matters

A production travel CRM must answer questions like:

- who changed this quote and when?
- what exactly was sent to the customer?
- when was this booking created from the quote?
- who uploaded the passport copy?
- why was the itinerary changed?
- what happened before the deal was marked accepted?

Without timeline + audit + notes, support and operations teams will distrust the system fast.

---

# 3. Domain model to add

## 3.1 New entity: `ActivityEntry`

Purpose: user-facing timeline of important CRM actions.

### Table: `activity_entries`

## Columns
- `id` UUID PK
- `tenant_id` UUID
- `entity_type` TEXT
- `entity_id` UUID
- `activity_type` TEXT
- `summary` TEXT
- `detail_json` JSONB
- `actor_user_id` UUID NULL
- `occurred_at` TIMESTAMPTZ
- `created_at` TIMESTAMPTZ

## Suggested `entity_type` values
- `Contact`
- `Quotation`
- `QuotationRevision`
- `Booking`
- `Traveler`
- `BookingItem`
- `BookingDocument`
- `Itinerary`
- `FollowUp`
- `Notification`
- `Invoice`

## Suggested `activity_type` values
- `Created`
- `Updated`
- `StatusChanged`
- `RevisionCreated`
- `Sent`
- `Viewed`
- `Accepted`
- `Rejected`
- `Expired`
- `BookingCreated`
- `TravelerAdded`
- `DocumentUploaded`
- `NotificationSent`
- `PaymentReceived`
- `CommentAdded`

## Notes
This table should be optimized for timeline reads, not deep compliance reporting.
That is audit log’s job.

---

## 3.2 New entity: `AuditLog`

Purpose: immutable, support/compliance-grade change trail.

### Table: `audit_logs`

## Columns
- `id` UUID PK
- `tenant_id` UUID
- `entity_type` TEXT
- `entity_id` UUID
- `action` TEXT
- `actor_user_id` UUID NULL
- `ip_address` TEXT NULL
- `user_agent` TEXT NULL
- `before_json` JSONB NULL
- `after_json` JSONB NULL
- `metadata_json` JSONB NULL
- `occurred_at` TIMESTAMPTZ

## Recommended usage
Capture audit for sensitive/important mutations:
- quote accepted/rejected/expired
- quote revision created
- booking cancelled
- booking status changed
- traveler updated
- attachment/document deleted
- user role changed
- tenant plan changed

## Notes
- `AuditLog` should be append-only
- never silently overwrite audit rows
- keep payloads concise but useful

---

## 3.3 New entity: `EntityNote`

Purpose: collaboration notes/comments on CRM records.

### Table: `entity_notes`

## Columns
- `id` UUID PK
- `tenant_id` UUID
- `entity_type` TEXT
- `entity_id` UUID
- `visibility` TEXT
- `content` TEXT
- `created_by_user_id` UUID NULL
- `created_at` TIMESTAMPTZ
- `updated_at` TIMESTAMPTZ
- `deleted_at` TIMESTAMPTZ NULL

## Suggested visibility values
- `Internal`
- `CustomerVisible`

## Notes
Most notes will be internal.
Customer-visible notes are useful later for selective shared context.

---

## 3.4 Optional helper entity: `EntityLink`

Purpose: connect related timeline items across modules.

### Table: `entity_links`

## Columns
- `id` UUID PK
- `tenant_id` UUID
- `source_entity_type` TEXT
- `source_entity_id` UUID
- `target_entity_type` TEXT
- `target_entity_id` UUID
- `link_type` TEXT
- `created_at` TIMESTAMPTZ

## Why optional
Useful when one event should appear in multiple contexts:
- booking created from quotation
- notification linked to quotation
- invoice linked to booking

You can skip this in first pass if timeline fan-out is simpler to do in application code.

---

# 4. Timeline design rules

## Timeline is not raw audit
Timeline entries should be readable by humans.

Good timeline summary:
- `Quotation revision v3 created`
- `Quote sent to jane@example.com`
- `Booking confirmed`
- `Passport copy uploaded for Jane Doe`

Bad timeline summary:
- `UPDATE quotations SET ...`

## Timeline should combine user and system actions
Include both:
- user-triggered actions
- important system events/jobs

## Timeline should be entity-first
Primary timeline endpoints should focus on one record at a time.

Examples:
- quote timeline
- booking timeline
- contact timeline

---

# 5. Audit design rules

## Audit is for traceability
Audit entries should preserve before/after snapshots for meaningful changes.

## Snapshot guidance
Store only relevant changed fields where possible.
Avoid dumping massive blobs if a concise diff-style snapshot is enough.

## Security guidance
Never store plaintext secrets or sensitive auth material in audit rows.
Mask if necessary.

---

# 6. Notes design rules

## Notes should support collaboration
Teams should be able to leave context like:
- customer prefers evening departures
- waiting on visa documents
- supplier promised upgrade if booked this week

## Notes should be soft-deletable
Do not hard-delete collaborative context by default.

## Notes should be tenant-scoped and entity-scoped
A note must always belong to a tenant and an entity.

---

# 7. API spec

All tenant-scoped APIs derive tenant from tenant context.

---

## 7.1 Timeline APIs

### GET `/travel/timeline/{entityType}/{entityId}`
Returns paginated timeline.

#### Query params
- `page`
- `pageSize`

#### Response shape
```json
{
  "items": [
    {
      "id": "uuid",
      "entityType": "Quotation",
      "entityId": "uuid",
      "activityType": "Sent",
      "summary": "Quote sent to jane@example.com",
      "detail": {
        "revisionNumber": 3,
        "channel": "Email"
      },
      "actorUserId": "uuid",
      "occurredAt": "2026-04-06T12:00:00Z"
    }
  ],
  "page": 1,
  "pageSize": 20,
  "totalCount": 42
}
```

### Convenience endpoints (optional but nice)
- `GET /travel/quotations/{id}/timeline`
- `GET /travel/bookings/{id}/timeline`
- `GET /travel/contacts/{id}/timeline`

---

## 7.2 Notes APIs

### POST `/travel/{entityType}/{entityId}/notes`
Create note.

#### Request body
```json
{
  "visibility": "Internal",
  "content": "Customer asked for a Rome alternative with better hotel location."
}
```

### GET `/travel/{entityType}/{entityId}/notes`
List notes.

### PUT `/travel/notes/{noteId}`
Update note.

### DELETE `/travel/notes/{noteId}`
Soft delete note.

---

## 7.3 Audit APIs

Audit should likely be admin/support protected.

### GET `/admin/audit/{entityType}/{entityId}`
Returns audit log entries.

#### Query params
- `page`
- `pageSize`

Optional additional endpoints:
- `GET /admin/audit/user/{userId}`
- `GET /admin/audit/tenant/{tenantId}`

---

# 8. Write-path integration plan

Timeline and audit cannot rely only on manual inserts everywhere forever.
That gets messy fast.

Recommended approach:

## 8.1 Introduce abstractions

### `IActivityWriter`
Methods:
- `WriteAsync(ActivityEntry entry, CancellationToken ct)`
- maybe helpers per activity type

### `IAuditWriter`
Methods:
- `WriteAsync(AuditLog entry, CancellationToken ct)`

### `IActorContext`
Expose:
- `UserId`
- `TenantId`
- `IpAddress`
- `UserAgent`

This avoids repeating request plumbing everywhere.

---

## 8.2 First write integration targets

Wire timeline/audit generation into these high-value actions first:

### Quotation actions
- revision created
- quote sent
- quote accepted/rejected/expired
- attachment uploaded/deleted

### Booking actions
- booking created from quote
- booking status changed
- traveler added/updated/deleted
- booking item added/status changed
- booking document uploaded/deleted

### CRM actions
- contact created/updated
- follow-up created/completed

### Identity/admin actions
- user role changed
- tenant plan changed

---

# 9. Read model spec

## 9.1 `ActivityEntryReadModel`
- `Id`
- `EntityType`
- `EntityId`
- `ActivityType`
- `Summary`
- `DetailJson`
- `ActorUserId`
- `OccurredAt`

## 9.2 `AuditLogReadModel`
- `Id`
- `EntityType`
- `EntityId`
- `Action`
- `ActorUserId`
- `IpAddress`
- `UserAgent`
- `BeforeJson`
- `AfterJson`
- `OccurredAt`

## 9.3 `EntityNoteReadModel`
- `Id`
- `EntityType`
- `EntityId`
- `Visibility`
- `Content`
- `CreatedByUserId`
- `CreatedAt`
- `UpdatedAt`

---

# 10. Migration plan

## Migration 1 - activity timeline
Add:
- `activity_entries`

## Migration 2 - audit logs
Add:
- `audit_logs`

## Migration 3 - entity notes
Add:
- `entity_notes`

## Optional migration 4
Add:
- `entity_links`

## Indexing
Add indexes on:
- `(tenant_id, entity_type, entity_id, occurred_at desc)` for activity
- `(tenant_id, entity_type, entity_id, occurred_at desc)` for audit
- `(tenant_id, entity_type, entity_id, created_at desc)` for notes

---

# 11. Suggested C# project/file structure

## Domain
- `Domain/Aggregates/ActivityEntry.cs`
- `Domain/Aggregates/AuditLog.cs`
- `Domain/Aggregates/EntityNote.cs`
- optionally `Domain/Aggregates/EntityLink.cs`

## Repositories
- `Domain/Repositories/IActivityEntryRepository.cs`
- `Domain/Repositories/IAuditLogRepository.cs`
- `Domain/Repositories/IEntityNoteRepository.cs`

## Application abstractions
- `Application/Abstractions/IActivityWriter.cs`
- `Application/Abstractions/IAuditWriter.cs`
- `Application/Abstractions/IActorContext.cs`

## Application commands
- `CreateEntityNoteCommand`
- `UpdateEntityNoteCommand`
- `DeleteEntityNoteCommand`

## Application queries
- `GetTimelineQuery`
- `GetAuditLogQuery`
- `ListEntityNotesQuery`

## API
- `Controllers/TimelineController.cs`
- `Controllers/NotesController.cs`
- `Controllers/AdminAuditController.cs`

---

# 12. PR breakdown

## PR 8.1 - timeline foundation
Status: completed on branch `feat/phase-8-timeline-foundation`

Includes:
- activity entry model
- timeline query endpoints
- first write hooks for quotation + booking creation/status flows

Implemented:
- `ActivityEntry` model + repository + writer abstraction
- `GET /travel/timeline/{entityType}/{entityId}`
- `GET /travel/quotations/{id}/timeline`
- `GET /travel/bookings/{id}/timeline`
- first write hooks for quotation revision creation and booking creation from quotation
- activity timeline migration
- activity timeline test coverage

### Must pass
- timeline entries appear for key flows
- pagination works

## PR 8.2 - audit infrastructure
Status: completed on branch `feat/phase-8-timeline-foundation`

Includes:
- audit log model
- actor context
- before/after snapshot helpers
- admin audit endpoint

Implemented:
- `AuditLog` model + repository + writer abstraction
- `IActorContext` with HTTP-backed actor metadata
- `GET /admin/audit/{entityType}/{entityId}`
- audit hooks for quotation accept/reject/expire, quotation revision creation, traveler update, and booking item status change
- audit log migration
- audit-focused test coverage

### Must pass
- important mutations create audit rows
- admin endpoint returns useful snapshots

## PR 8.3 - notes/comments
Status: completed on branch `feat/phase-8-timeline-foundation`

Includes:
- entity note model
- create/list/update/delete note APIs

Implemented:
- `EntityNote` model + repository
- `POST /travel/{entityType}/{entityId}/notes`
- `GET /travel/{entityType}/{entityId}/notes`
- `PUT /travel/notes/{noteId}`
- `DELETE /travel/notes/{noteId}`
- timeline hook for `CommentAdded`
- entity notes migration
- note CRUD and soft-delete test coverage

### Must pass
- notes are entity-scoped and tenant-safe
- soft delete works

## PR 8.4 - integration enrichment
Status: completed on branch `feat/phase-8-timeline-foundation`

Includes:
- notification/payment/linking enrichment
- cross-entity timeline propagation where needed

Implemented:
- quotation sent and viewed timeline entries
- quotation attachment upload/delete timeline entries
- booking document upload/delete timeline entries
- traveler add/delete timeline entries
- booking item add/update timeline entries
- follow-up create/update/complete propagation to both `FollowUp` and linked `Contact` timelines

### Must pass
- quote/booking timeline becomes genuinely useful

## PR 8.5 - tests + docs
Status: completed on branch `feat/phase-8-timeline-foundation`

Includes:
- integration tests
- postman updates
- admin/support docs

Implemented:
- expanded targeted travel-service tests covering timeline, audit, notes, attachments, documents, share-link viewing, and workflow coverage
- phase 8 API examples doc
- implementation spec updated to reflect completed PRs

---

# 13. Validation and behavior rules

## Timeline rules
- every entry must be tenant-scoped
- entity type/id required
- summary should be human-readable
- occurredAt should reflect event time, not query time

## Audit rules
- append-only
- actor metadata captured when available
- do not store secrets
- before/after should be null only when truly not applicable

## Notes rules
- note content required
- visibility required
- only notes for tenant-owned entities allowed
- soft delete only

---

# 14. Test checklist

## Timeline tests
- quote revision creation adds timeline entry
- booking creation from quote adds timeline entry
- traveler add adds timeline entry
- attachment upload adds timeline entry

## Audit tests
- accept quote writes audit row
- cancel booking writes audit row
- user role change writes audit row

## Notes tests
- create/list/update/delete note
- note tenant isolation
- deleted notes hidden from standard list

## Security tests
- cannot read another tenant's notes/timeline/audit
- admin audit endpoint locked properly
- customer/public quote routes do not expose internal audit/note data

---

# 15. Definition of done for Phase 8

Phase 8 is done only when:

- quotes, bookings, and contacts have usable timeline endpoints
- important business mutations write audit logs
- notes/comments work across major CRM entities
- support/admin can inspect record history
- tenant enforcement applies to all timeline/audit/note endpoints
- build/migrations/tests pass
- docs/postman are updated

---

# 16. Final blunt recommendation

If you want the fastest practical value from Phase 8, do this order:

1. `PR 8.1` activity timeline
2. `PR 8.3` notes/comments
3. `PR 8.2` audit infrastructure
4. `PR 8.4` integration enrichment
5. `PR 8.5` tests/docs

Why this order:
- timeline gives immediate product visibility
- notes give immediate team collaboration value
- audit gives governance depth

Together, they make the CRM feel alive and accountable instead of silent and suspicious.
