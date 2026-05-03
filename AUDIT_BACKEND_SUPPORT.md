# AUDIT_BACKEND_SUPPORT

## Scope
Audit of backend/API support for the Voyara portals using `C:\Users\uzayr\source\repos\billing-platform` as source of truth.

Reviewed areas:
- API gateway routing and middleware
- Identity service
- Billing service
- Travel service
- Communication service
- Webhook service
- Portal frontend scaffolds for expected API usage

---

## Executive summary

The repo already has a decent amount of backend surface area for an internal/admin portal, especially around travel CRM/quotations/bookings, billing entitlements, branding management, communications, and webhook administration.

The main blockers are not “missing controllers everywhere”; they are **integration mismatches**:

1. **Gateway blocks public portal flows**
   - The customer portal quote pages use public quotation endpoints (`/travel/quotations/public/{token}...`), but the gateway JWT middleware only exempts auth/health/metrics paths.
   - Result: these public endpoints work when called directly against `travel-service`, but **not through the gateway**.

2. **Portal branding reads are admin-protected**
   - Customer/admin portal scaffolds fetch `GET /tenant-branding` and `GET /tenant-branding/templates/{scope}`.
   - Both endpoints in identity currently require `branding.theme.manage` permission.
   - Result: these are **not suitable as-is for anonymous/public portal rendering**, and likely too privileged even for ordinary non-admin signed-in users.

3. **Gateway feature-entitlement rules do not accurately match several implemented routes**
   - Example: gateway checks `POST /api/travel/quotations/send`, but the implemented route is `POST /api/travel/quotations/{id}/send`.
   - Same pattern exists for booking documents and communication template listing.
   - Result: some intended entitlement checks will never fire.

4. **Direct service security posture is inconsistent**
   - Billing service has no authentication/authorization middleware enabled.
   - Communication service enables JWT auth, but controllers do not declare `[Authorize]`/permission attributes.
   - Webhook service relies on `x-tenant-id` only.
   - Result: these services are apparently meant to sit behind the gateway, but direct service access is much looser than the gateway contract.

Bottom line:
- **Admin/ops portal support:** mostly present at backend level.
- **Customer/public portal support:** partially present in travel, but currently mismatched with gateway and branding protection.
- **Main work needed:** route/contract cleanup, public-safe branding endpoints, gateway allowlist for intended public flows, and auth/permission hardening/alignment.

---

## Gateway/API contract overview

Gateway config (`services/api-gateway/src/appsettings.json`) exposes these public API prefixes:

- `/api/auth/*` -> identity `/auth/*`
- `/api/identity/*` -> identity `/identity/*`
- `/api/tenant-branding/*` -> identity `/tenant-branding/*`
- `/api/billing/*` -> billing `/billing/*`
- `/api/webhooks/*` -> webhook `/webhooks/*`
- `/api/travel/*` -> travel `/travel/*`
- `/api/communication/*` -> communication `/communication/*`
- `/api/geo-leads/*` -> geo-leads `/geo-leads/*`

### Important aliasing
There are effectively two route layers:

- **Gateway-facing aliases**: `/api/...`
- **Service-native routes**: `/identity/...`, `/tenant-branding/...`, `/billing/...`, `/travel/...`, `/communication/...`, `/webhooks/...`

The frontend scaffolds currently call **service-native routes directly** (`http://localhost:8080/tenant-branding`, `http://localhost:8082/travel/...`) instead of gateway `/api/...` routes, which conflicts with `frontend/README.md` guidance that integrations should be gateway-aware.

---

## Per-domain audit

## 1) Identity

### Implemented backend support

#### Auth
Identity service exposes:
- `POST /auth/register`
- `POST /auth/login`
- `POST /auth/refresh`
- `POST /auth/logout`
- `GET /.well-known/jwks.json`

Gateway alias:
- `/api/auth/*` -> `/auth/*`

#### Users / invitations / roles / sessions / tenant ops
Implemented endpoints include:
- `POST /identity/users`
- `GET /identity/users`
- `GET /identity/users/{userId}`
- `PUT /identity/users/{userId}`
- `DELETE /identity/users/{userId}`
- `POST /identity/users/{userId}/suspend`
- `POST /identity/users/{userId}/reactivate`
- `POST /identity/users/invitations`
- `POST /identity/users/invitations/{id}/resend`
- `POST /identity/users/invitations/accept`
- `GET /identity/permissions`
- `GET /identity/roles`
- `POST /identity/roles`
- `PUT /identity/roles/{id}`
- `PUT /identity/users/{userId}/roles`
- `GET /identity/me/sessions`
- `DELETE /identity/me/sessions/{sessionId}`
- `DELETE /identity/users/{userId}/sessions`
- `POST /identity/logout`
- `GET /tenants/{id}`
- `POST /tenants/{id}/suspend`
- `GET /identity/tenant-settings`
- `PUT /identity/tenant-settings`
- MFA endpoints under `identity/me/mfa/*`

#### Audit/security
Implemented endpoints:
- `GET /identity/audit/users/{userId}`
- `GET /identity/security-events`
- `GET /identity/audit/export`

### Permissions/auth characteristics
Identity is the most mature service in permissioning terms:
- JWT auth is configured
- authorization policies are registered
- controllers use `RequirePermission(...)` for many admin endpoints

Key permission constants include:
- `identity.users.manage`
- `identity.roles.manage`
- `identity.audit.read`
- `identity.settings.manage`
- `identity.tenant.manage`
- `branding.theme.manage`

### Obvious gaps / mismatches

#### Missing or weak portal-oriented “current user” surface
For a portal, an endpoint like `GET /identity/me` or `GET /identity/profile` is conspicuously absent. There are session and MFA endpoints, but no obvious canonical “who am I / current user profile” endpoint.

#### Gateway feature key naming mismatch vs service permissions
Gateway entitlement rules use feature keys like:
- `identity.rbac.advanced`
- `identity.audit.export`

Identity service permission checks use:
- `identity.roles.manage`
- `identity.users.manage`
- `identity.audit.read`

This may be intentional (subscription features vs RBAC permissions), but it creates a two-layer model that is easy to misconfigure. In practice:
- gateway may allow a route based on subscription
- service may still deny based on permission
- or gateway may not check the right route at all

#### JWKS route mismatch at gateway edge
Identity exposes `GET /.well-known/jwks.json`, and gateway JWT middleware explicitly exempts that path, but YARP routes do **not** map `/.well-known/jwks.json` through the gateway. So the public key is reachable directly from identity, but not obviously through the gateway contract.

### Portal readiness verdict
- **Admin portal:** good backend coverage
- **Customer/public portal:** weak unless public-safe read endpoints are added or permission model is relaxed for selected reads

---

## 2) Branding

### Implemented backend support
Identity owns branding.

Implemented endpoints:
- `GET /tenant-branding`
- `PUT /tenant-branding`
- `GET /tenant-branding/assets`
- `POST /tenant-branding/assets`
- `DELETE /tenant-branding/assets/{assetId}`
- `GET /tenant-branding/files/{**storageKey}`
- `GET /tenant-branding/templates/{scope}`
- `PUT /tenant-branding/templates/{scope}`

Gateway alias:
- `/api/tenant-branding/*` -> `/tenant-branding/*`

### What the portals expect
Frontend scaffolds use:
- admin portal: `GET /tenant-branding`
- customer portal: `GET /tenant-branding`
- customer portal: `GET /tenant-branding/templates/{scope}`

### Critical mismatch
Both `TenantBrandingController` and `TenantTemplateThemesController` are decorated with:
- `RequirePermission(Permissions.Branding.ThemeManage)`

That means the exact reads the portals need are currently **branding-admin-only**.

This is especially problematic for:
- anonymous or semi-public quote pages
- normal traveler/customer views
- any admin shell user who should see branding but not manage themes

### Obvious missing endpoints
Likely missing for portal-safe usage:
- `GET /tenant-branding/public` or equivalent public branding read
- `GET /tenant-branding/templates/{scope}/public` or equivalent
- possibly a simplified branding payload for public quote/portal rendering

### Portal readiness verdict
- **Branding management:** implemented
- **Branding consumption by customer/public portal:** not properly supported with current permissions/gateway shape

---

## 3) Billing / entitlements

### Implemented backend support
Billing service exposes strong package/entitlement management coverage.

Representative endpoints:
- `GET /billing/dashboard`
- `GET /billing/features`
- `POST /billing/features`
- `PUT /billing/features/{featureKey}`
- `GET /billing/packages`
- `GET /billing/packages/{id}`
- `POST /billing/packages`
- `PUT /billing/packages/{id}`
- `PUT /billing/packages/{id}/features`
- `POST /billing/subscriptions`
- `GET /billing/subscriptions`
- `DELETE /billing/subscriptions/{id}`
- `POST /billing/invoices/generate`
- `GET /billing/invoices/{id}`
- `GET /billing/invoices`
- `POST /billing/invoices/{id}/pay`
- `GET /billing/invoices/tenant/{tenantId}`
- `GET /billing/entitlements/me`
- `GET /billing/entitlements/{tenantId}`
- `POST /billing/entitlements/{tenantId}/grants`
- `POST /billing/entitlements/{tenantId}/packages`
- `POST /billing/entitlements/{tenantId}/overrides`
- `GET /billing/tenants/{tenantId}/packages`
- `POST /billing/tenants/{tenantId}/packages`
- `PUT /billing/tenants/{tenantId}/packages/{assignmentId}`
- `DELETE /billing/tenants/{tenantId}/packages/{assignmentId}`
- `GET /billing/tenants/{tenantId}/feature-overrides`
- `POST /billing/tenants/{tenantId}/feature-overrides`
- `PUT /billing/tenants/{tenantId}/feature-overrides/{overrideId}`
- `DELETE /billing/tenants/{tenantId}/feature-overrides/{overrideId}`
- `GET /billing/tenants/{tenantId}/entitlements`
- `GET /billing/tenants/{tenantId}/entitlements/{featureKey}`
- `GET /billing/tenants/{tenantId}/feature-allocations`
- `GET /billing/tenants/{tenantId}/users/{userId}/features`
- `POST /billing/tenants/{tenantId}/users/{userId}/feature-assignments`
- `DELETE /billing/tenants/{tenantId}/users/{userId}/feature-assignments/{featureKey}`
- `GET /billing/feature-access/me`
- `POST /billing/webhooks/stripe`

### Portal usefulness
Good support exists for:
- subscription/package management
- per-tenant entitlements
- per-user feature access
- invoice retrieval/listing

This is enough for a serious admin billing console.

### Important auth/security observation
Billing service does **not** configure `AddAuthentication`, `UseAuthentication`, or `UseAuthorization` in `Program.cs`.

Controllers mostly protect tenant boundaries by comparing route tenantId to `tenantContext.TenantId`, but `tenantContext` can be satisfied from headers. That means direct access to billing-service appears much looser than gateway access.

### Obvious gaps / portal gaps
Potentially missing for a polished portal:
- explicit invoice download/PDF endpoint
- billing checkout/session style endpoints for hosted payment UX
- customer-facing subscription self-service endpoints distinct from admin package assignment operations
- explicit permissions on billing admin operations

### Portal readiness verdict
- **Admin portal billing console:** solid backend base
- **Customer self-service billing UX:** partial, but not clearly productized yet

---

## 4) Travel inquiries / quotations / bookings / itineraries

This is the richest domain in the repo.

### Implemented backend support

#### Inquiries / CRM intake
- `GET /travel/inquiries`
- `GET /travel/inquiries/{id}`
- `GET /travel/inquiries/{id}/history`
- `POST /travel/inquiries/{id}/assign`
- `POST /travel/inquiries/{id}/qualify`
- `POST /travel/inquiries/{id}/disqualify`
- `POST /travel/inquiries/{id}/mark-contacted`
- `POST /travel/inquiries/{id}/archive`
- `POST /travel/inquiries/{id}/convert-to-quotation`
- draft concept endpoints under `/travel/inquiries/{id}/concepts...`
- public lead intake: `POST /travel/public/inquiries`

#### Quotations
- `POST /travel/quotations`
- `POST /travel/quotations/{id}/revisions`
- `POST /travel/quotations/{id}/send`
- `POST /travel/quotations/{id}/approval-requests`
- `POST /travel/quotations/{id}/approval-requests/{approvalRequestId}/approve`
- `POST /travel/quotations/{id}/approval-requests/{approvalRequestId}/reject`
- `POST /travel/quotations/{id}/accept`
- `POST /travel/quotations/{id}/reject`
- `POST /travel/quotations/{id}/expire`
- `GET /travel/quotations/{id}`
- `GET /travel/quotations/{id}/history`
- `GET /travel/quotations/{id}/revisions`
- `GET /travel/quotations/{id}/revisions/{revisionId}`
- `POST /travel/quotations/{id}/attachments`
- `GET /travel/quotations/{id}/attachments`
- `DELETE /travel/quotations/{id}/attachments/{attachmentId}`
- `GET /travel/quotations`
- public quote endpoints:
  - `GET /travel/quotations/public/{token}`
  - `POST /travel/quotations/public/{token}/viewed`
  - `POST /travel/quotations/public/{token}/accept`
  - `POST /travel/quotations/public/{token}/reject`
- `PUT /travel/quotations/{id}`
- legacy conversion endpoint: `POST /travel/quotations/{id}/convert`

#### Bookings / itinerary / documents / travelers / items
- `POST /travel/bookings/from-quotation/{quotationId}`
- `GET /travel/bookings`
- `GET /travel/bookings/{id}`
- `GET /travel/bookings/{id}/financial-summary`
- `GET /travel/bookings/{id}/itinerary`
- `POST /travel/bookings/{id}/itinerary`
- traveler CRUD-like operations under `/travel/bookings/{id}/travelers`
- booking item CRUD/workflow operations under `/travel/bookings/{id}/items`
- `POST /travel/bookings/{id}/documents`
- `GET /travel/bookings/{id}/documents`
- `DELETE /travel/bookings/{id}/documents/{documentId}`

#### Standalone itinerary / timeline / reporting / workflow hub
- `POST /travel/itineraries`
- `GET /travel/itineraries/{id}`
- `GET /travel/itineraries`
- `PUT /travel/itineraries/{id}`
- `GET /travel/activity`
- `GET /travel/timeline/{entityType}/{entityId}`
- `GET /travel/quotations/{id}/timeline`
- `GET /travel/bookings/{id}/timeline`
- `GET /travel/search`
- `GET /travel/reports/bookings`
- `GET /travel/export/bookings.csv`
- `GET /travel/workflow-hub`
- admin audit: `GET /admin/audit/{entityType}/{entityId}`
- PDFs:
  - `GET /travel/documents/quotations/{quotationId}/revisions/{revisionId}/pdf`
  - `GET /travel/documents/bookings/{bookingId}/itinerary/pdf`

### Portal usefulness
For an internal ops portal, travel support is very good:
- inquiry pipeline
- quotation lifecycle
- booking lifecycle
- itinerary/document support
- activity/timeline/audit/reporting

For customer/public portal usage, only a **small subset** is portal-ready:
- public quote retrieval/accept/reject/viewed
- branding-dependent quote rendering in frontend scaffold

### Critical gateway mismatch: public flows blocked
The customer portal scaffold uses:
- `GET /travel/quotations/public/{token}`
- `POST /travel/quotations/public/{token}/accept`
- `POST /travel/quotations/public/{token}/reject`

These endpoints exist in the travel service, but the gateway JWT middleware does **not** exempt `/api/travel/quotations/public/*`.

So:
- direct service calls work
- gateway calls require bearer token
- that defeats the intended public quote-link experience

### Permission/feature mismatch examples
Travel service uses granular permissions like:
- `travel.inquiries.read/write`
- `travel.bookings.read/write`
- `travel.timeline.read`
- `travel.documents.read`
- `travel.quotation.read/write`
- `travel.audit.read`

Gateway feature entitlements are much coarser and sometimes route-misaligned:
- checks `POST /api/travel/quotations/send` but actual route is `POST /api/travel/quotations/{id}/send`
- checks `POST /api/travel/bookings/documents` but actual route is `POST /api/travel/bookings/{id}/documents`
- checks `GET /api/travel/timeline` but implemented timeline reads also include:
  - `/api/travel/activity`
  - `/api/travel/quotations/{id}/timeline`
  - `/api/travel/bookings/{id}/timeline`

### Obvious missing endpoints
Depending on portal roadmap, likely missing customer-facing APIs include:
- customer booking retrieval by share token / traveler identity
- customer itinerary view endpoints separate from internal booking ids
- quote attachment download endpoint guaranteed to work through gateway/public contract
- customer communication/activity feed around a quote/booking

### Portal readiness verdict
- **Admin portal travel ops:** strong
- **Customer portal/public quote experience:** partially implemented but blocked/misaligned at gateway and branding layers

---

## 5) Communications

### Implemented backend support
Implemented endpoints:
- `POST /communication/notifications`
- `POST /communication/notifications/workflows/{workflowType}`
- `GET /communication/notifications`
- `GET /communication/notifications/{id}`
- `GET /communication/notifications/recipient/{recipientId}`
- `GET /communication/notifications/recipient/{recipientId}/unread-count`
- `PATCH /communication/notifications/{id}/read`
- `POST /communication/notifications/{id}/replay`
- `PUT /communication/recipient-preferences`
- `GET /communication/recipient-preferences/{recipientId}`
- `POST /communication/templates`
- `GET /communication/templates/{id}`
- `GET /communication/templates`
- `PUT /communication/templates/{id}`

Infrastructure also clearly supports:
- email
- SMS
- WhatsApp
- push
- in-app
- template rendering backed by identity branding
- recipient resolution via travel contact data

### Portal usefulness
Good support exists for:
- admin notification logs
- template management
- recipient preferences
- workflow-triggered sends
- unread count / recipient inbox style reads

### Important auth/security observation
Communication service enables authentication/authorization middleware, but controllers do **not** appear to declare `[Authorize]` or permission attributes.

That means authorization relies heavily on gateway placement and tenant context header/claim handling rather than controller-level policy declarations.

### Gateway feature-entitlement mismatch
Gateway rule:
- `GET /api/communication/templates/tenant` -> `communication.templates.manage`

But implemented route is:
- `GET /communication/templates`

So this gateway entitlement rule does not map to a real route pattern.

### Obvious missing endpoints
Potentially missing for full portal UX:
- template delete/archive endpoint
- notification bulk actions
- communication thread/conversation model
- customer-safe inbox endpoints that do not expose generic recipientId-driven access patterns

### Portal readiness verdict
- **Admin communications center:** decent backend base
- **Customer messaging/inbox experience:** partial only

---

## 6) Webhooks

### Implemented backend support
Webhook service exposes:
- `GET /webhooks/deliveries`
- `GET /webhooks/deliveries/{id}`
- `POST /webhooks/deliveries/{id}/replay`
- `GET /webhooks/subscriptions`
- `POST /webhooks/subscriptions`
- `DELETE /webhooks/subscriptions/{id}`

Gateway alias:
- `/api/webhooks/*` -> `/webhooks/*`

Also present:
- delivery queue/processor
- signing service
- billing event consumers/listeners
- delivery log module

### Portal usefulness
Enough exists for a first admin webhook console:
- create/list/deactivate subscriptions
- inspect delivery logs
- replay failed deliveries

### Auth/security observation
Webhook service appears to rely on `x-tenant-id` extraction only; no JWT/permission guard is visible in `main.ts` or controller decorators.

Again, this suggests the real security boundary is expected to be the gateway, not the service itself.

### Obvious missing endpoints
Likely missing for a polished portal webhook admin:
- update subscription endpoint
- rotate/reveal signing secret endpoint policy
- pause/resume without delete
- webhook test-fire endpoint
- event-type catalog endpoint

### Portal readiness verdict
- **Admin webhook console:** basic but usable
- **Advanced webhook management:** incomplete

---

## Cross-cutting route/contract aliases

## Gateway aliases
- `/api/auth/*` <-> identity `/auth/*`
- `/api/identity/*` <-> identity `/identity/*`
- `/api/tenant-branding/*` <-> identity `/tenant-branding/*`
- `/api/billing/*` <-> billing `/billing/*`
- `/api/travel/*` <-> travel `/travel/*`
- `/api/communication/*` <-> communication `/communication/*`
- `/api/webhooks/*` <-> webhook `/webhooks/*`

## Direct-service aliases already used by frontend
- admin portal calls `IDENTITY_BASE_URL/tenant-branding`
- customer portal calls `IDENTITY_BASE_URL/tenant-branding`
- customer portal calls `IDENTITY_BASE_URL/tenant-branding/templates/{scope}`
- customer portal calls `TRAVEL_BASE_URL/travel/quotations/public/{token}...`

This means the frontend currently assumes **service-native** contracts, not the gateway contract.

## Legacy alias
Travel has an explicit legacy compatibility endpoint:
- `POST /travel/quotations/{id}/convert`

The code comments say preferred path is:
- accepted quotation -> booking -> `POST /travel/bookings/{id}/itinerary`

---

## Likely permission / access mismatches

## A) Public quotation endpoints exist, but gateway requires JWT
Impact: customer portal public quote flow breaks if routed through gateway.

## B) Branding read endpoints require `branding.theme.manage`
Impact: portal rendering depends on endpoints that are effectively admin-theme-management endpoints.

## C) Gateway feature-entitlement rules do not match several real routes
Examples:
- configured: `POST /api/travel/quotations/send`
  - real: `POST /api/travel/quotations/{id}/send`
- configured: `POST /api/travel/bookings/documents`
  - real: `POST /api/travel/bookings/{id}/documents`
- configured: `GET /api/communication/templates/tenant`
  - real: `GET /api/communication/templates`

Impact: subscription enforcement intended by gateway may silently not happen.

## D) Service-level auth hardening is inconsistent
- identity: strong-ish and explicit
- travel: explicit permission checks on many endpoints
- communication: auth middleware present, but no visible endpoint-level authorize attributes
- billing: no auth middleware configured
- webhook: no JWT auth visible

Impact: behavior differs depending on whether traffic goes through gateway or directly to a service.

## E) Frontend contract does not match repo guidance
`frontend/README.md` says API integration should go through the gateway-aware shared client package, but current portal pages call services directly.

Impact: alias drift, duplicated route assumptions, and bypassing gateway-only policies.

---

## Obvious missing endpoints by portal concern

## Customer/public portal
Likely missing or not properly exposed:
- public-safe tenant branding read
- public-safe template theme read
- gateway-accessible public quotation endpoints
- customer-facing booking/itinerary retrieval flows
- customer-facing authenticated profile endpoint (`/identity/me` style)

## Admin portal
Mostly covered, but likely still missing:
- explicit dashboard aggregation endpoints across domains
- richer webhook management actions (update/test/rotate secret)
- billing/customer self-service flows separate from internal package assignment
- communication delete/archive/versioning endpoints

---

## Practical conclusion

### What is already supported well
- internal travel operations
- user/role/invitation administration
- tenant branding management
- billing package/entitlement management
- notification/template management
- basic webhook subscription/delivery administration

### What is most obviously not aligned yet
- public/customer portal routing through the gateway
- public branding consumption
- consistent service-level auth/permission posture
- gateway entitlement rule accuracy
- a canonical gateway-aware frontend contract

---

## Recommended next fixes (highest value first)

1. **Make public quote flow truly public at gateway level**
   - exempt intended public quotation endpoints from JWT middleware, or add dedicated public gateway routes.

2. **Add portal-safe branding read endpoints**
   - separate read-for-rendering from theme-management permissions.

3. **Fix gateway feature entitlement route patterns**
   - especially quotation send, booking documents, and communication template list.

4. **Standardize frontend on gateway routes**
   - use `/api/...` via shared client, not direct service-native URLs.

5. **Harden service-level auth consistency**
   - especially billing, communication, and webhook services.

6. **Add a current-user profile endpoint**
   - likely under `/identity/me` for both admin and customer experiences.

---

## Overall verdict

- **Admin portal backend support:** **mostly implemented**
- **Customer/public portal backend support:** **partially implemented but currently mismatched**
- **Biggest risk:** not lack of backend code, but **gateway/permission/contract drift** between services and portal expectations
