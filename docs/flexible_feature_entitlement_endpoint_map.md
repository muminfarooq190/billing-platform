# Flexible Feature Entitlement Endpoint Map

Last updated: 2026-04-11

This is the operator-facing map for the rollout work that is actually enforced today.

## Travel service
- `POST /api/travel/quotations` -> `travel.quotation.create`
- `POST /api/travel/quotations/send/*` -> `travel.quotation.send`
- `POST /api/travel/bookings/from-quotation/*` -> `travel.booking.create`
- `POST /api/travel/bookings/documents/*` -> `travel.booking.documents.upload`
- `GET /api/travel/timeline/*` -> `travel.timeline.read`
- `GET /api/travel/admin/audit/*` -> `travel.audit.read`
- entity note write commands -> `travel.notes.write`
- public quotation accept/reject handlers -> `travel.quotation.send`

## Communication service
- `POST /api/communication/notifications` -> `communication.notification.send`
- `GET /api/communication/notifications/recipient/{recipientId}?tenantId=` -> `communication.logs.read`
- `GET /api/communication/notifications/recipient/{recipientId}/unread-count?tenantId=` -> `communication.logs.read`
- `POST /api/communication/templates` -> `communication.templates.manage`
- `PUT /api/communication/templates/{id}` -> `communication.templates.manage`
- `GET /api/communication/templates/tenant/{tenantId}` -> `communication.templates.manage`

## Identity service
- `GET /api/identity/audit/users/{userId}` -> `identity.audit.read`
- `GET /api/identity/security-events` -> `identity.audit.read`
- `GET /api/identity/audit/export` -> `identity.audit.export`
- `GET|POST|PUT /api/identity/roles*` -> `identity.rbac.advanced`
- `PUT /api/identity/users/{userId}/roles` -> `identity.rbac.advanced`
- tenant branding theme endpoints -> `branding.theme.manage`
- tenant branding asset file reads -> `branding.assets.manage`
- tenant settings endpoints -> `identity.settings.manage`
- tenant admin endpoints -> `identity.tenant.manage`

## Gateway coarse preflight coverage
The gateway now coarse-blocks some obvious premium routes, but it is not authoritative. Service handlers remain the real control point.

Covered route families:
- travel quotation create/send, booking create/doc upload, timeline, audit
- communication notification send, notification log reads, template create/update/list
- identity audit reads/export, advanced RBAC routes

## Known gaps still not solved on this branch
- no implemented communication bulk/campaign endpoint to wrap yet
- no implemented identity SSO/domain-management surface to gate yet
- no implemented impersonation/support-access flow to gate yet
- gateway metadata is still config-based and coarse, not derived from route attributes
- postman collection has not yet been fully refreshed to include every failure/success entitlement example
