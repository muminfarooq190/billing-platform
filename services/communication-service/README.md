# Communication Service MVP notes

This service now includes a pragmatic MVP pass for business-facing outbound messaging.

## Tenant-safe writes

Authenticated write endpoints no longer accept caller-controlled tenant ids for:
- notification sends
- workflow sends
- template creation/listing
- recipient preferences updates/reads

Tenant scope is derived from `ITenantContext` / `x-tenant-id`.

## Workflow endpoints

`POST /communication/notifications/workflows/{workflowType}`

Supported workflow types:
- `quotation-sent`
- `booking-confirmed`
- `invoice-issued`
- `payment-reminder`
- `payment-receipt`

Workflow sends resolve through the same notification pipeline and support:
- `referenceId`
- `correlationId`
- optional `idempotencyKey`
- optional `documents` attachment/document metadata
- optional freeform `metadata`
- optional template override / placeholder data

## Ops visibility

New notification ops endpoints:
- `GET /communication/notifications`
- `GET /communication/notifications/{id}`
- `POST /communication/notifications/{id}/replay`

List filters include tenant-scoped status, channel, workflow type, reference id, correlation id, and recipient id.

## Document-aware sends

Notification payloads can now carry document metadata references rather than binary payloads. This is intentionally MVP-safe: store links/metadata here, keep blob storage elsewhere.
