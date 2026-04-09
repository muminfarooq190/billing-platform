# Phase 8 API Examples - Timeline, Audit, and Notes

_Last updated: 2026-04-09_

This file documents the implemented Phase 8 endpoints in the travel service.

---

## Timeline

### Get entity timeline

`GET /travel/timeline/{entityType}/{entityId}?page=1&pageSize=20`

Example:

```http
GET /travel/timeline/Quotation/11111111-1111-1111-1111-111111111111?page=1&pageSize=20
```

### Convenience timeline endpoints

```http
GET /travel/quotations/{id}/timeline?page=1&pageSize=20
GET /travel/bookings/{id}/timeline?page=1&pageSize=20
```

Typical activity types now emitted include:
- `RevisionCreated`
- `BookingCreated`
- `Sent`
- `Viewed`
- `TravelerAdded`
- `DocumentUploaded`
- `CommentAdded`
- `StatusChanged`
- `Updated`

---

## Audit

### Get audit log for an entity

`GET /admin/audit/{entityType}/{entityId}?page=1&pageSize=20`

Example:

```http
GET /admin/audit/Quotation/11111111-1111-1111-1111-111111111111?page=1&pageSize=20
```

Audit payloads include:
- `beforeJson`
- `afterJson`
- `metadataJson`
- actor metadata when available

Currently wired audit events include:
- quotation accepted
- quotation rejected
- quotation expired
- quotation revision created
- traveler updated
- booking item status changed

---

## Notes

### Create note

`POST /travel/{entityType}/{entityId}/notes`

```json
{
  "visibility": "Internal",
  "content": "Customer prefers evening departures."
}
```

### List notes

`GET /travel/{entityType}/{entityId}/notes`

### Update note

`PUT /travel/notes/{noteId}`

```json
{
  "visibility": "CustomerVisible",
  "content": "Updated note content"
}
```

### Soft delete note

`DELETE /travel/notes/{noteId}`

Notes are tenant-scoped, entity-scoped, and soft deleted.

---

## Enrichment highlights

The timeline now includes useful cross-entity and operational events such as:
- quotation sent and customer viewed
- quotation attachment uploaded/deleted
- booking document uploaded/deleted
- traveler added/removed
- booking item added/updated/status changed
- follow-up created and updated/completed on both the follow-up and linked contact timeline

---

## Validation summary

- timeline, audit, and notes remain tenant-scoped
- audit is append-only
- notes use soft delete
- public quote view records timeline activity without exposing audit/note data
