# Implementation Spec - Subscription Entitlements & Feature Gating Across Services

_Last updated: 2026-04-09_

This document defines how Voyara should enforce subscription-based access across all services and endpoints.

Blunt goal:
if a tenant has not paid for or been granted a feature, they should not be able to use that feature's write APIs, protected reads, background jobs, or UI actions.

This must align with the current architecture:
- API Gateway
- per-service databases
- CQRS + MediatR
- tenant context propagated from gateway to downstream services
- billing-service already owns `Subscription` and `PlanType`

---

# 1. Problem statement

Right now billing-service knows the tenant's plan (`Free`, `Pro`, `Enterprise`), but the rest of the platform does not yet use that information as a hard authorization boundary.

That creates a serious product gap:
- a tenant may call `create quote` even if their plan should not allow quotations
- a tenant may upload documents or use premium workflows without entitlement
- frontend and API behavior can drift apart
- feature access becomes policy by convention instead of policy by code

We need a proper entitlement system.

---

# 2. Scope

This spec covers:
- subscription-plan to feature mapping
- enforcement model across gateway + services
- endpoint-level entitlement checks
- phase-wise rollout across existing services
- behavior for reads, writes, jobs, and public flows
- alignment with current architecture

This spec does not cover:
- pricing page UX
- payment processor product catalog modeling
- sales-admin custom deal workflows
- full enterprise contract negotiation tooling

---

# 3. Core design principle

## Plans are commercial labels. Entitlements are technical permissions.

Do not hardcode plan names deep inside travel-service, billing-service, communication-service, etc.

Instead:
- Billing owns subscription state and plan assignment
- An entitlement layer resolves effective features for a tenant
- Services check named capabilities such as:
  - `travel.quotation.create`
  - `travel.booking.create`
  - `travel.timeline.read`
  - `communication.notification.send`
  - `branding.theme.manage`

This keeps pricing flexible later.

---

# 4. Recommended target model

## 4.1 New domain concept: `FeatureEntitlement`

This can live logically under billing ownership first.

### Suggested fields
- `Id`
- `TenantId`
- `FeatureKey`
- `Granted` (bool)
- `Source` (`Plan`, `Override`, `Trial`, `AdminGrant`)
- `PlanType` (nullable snapshot)
- `LimitValue` (nullable; for quota-style features)
- `EffectiveFrom`
- `EffectiveTo` (nullable)
- `MetadataJson`
- `CreatedAt`
- `UpdatedAt`

## 4.2 Optional helper concept: `FeatureCatalog`

Purpose:
- central registry of known feature keys
- docs + validation + migration support

### Suggested fields
- `FeatureKey`
- `Service`
- `Category`
- `Description`
- `DefaultFree`
- `DefaultPro`
- `DefaultEnterprise`
- `IsQuota`
- `DefaultLimitValue`

---

# 5. Ownership model

## Billing-service remains source of truth

Billing-service should own:
- subscription plan state
- entitlement resolution records
- plan-to-feature defaults
- optional overrides

Other services should not own subscription policy.
They should consume effective entitlements.

---

# 6. Architecture alignment with current repo

Current repo characteristics:
- API Gateway validates JWT and forwards `x-tenant-id`
- each service has tenant context locally
- services use CQRS + MediatR
- services own their own databases
- direct cross-service DB access is forbidden by ADR

Therefore the best fit is:

## 6.1 Add an entitlement provider abstraction per service

### Suggested interface
`IFeatureGate`

Methods:
- `Task EnsureEnabledAsync(string featureKey, Guid tenantId, CancellationToken ct)`
- `Task<bool> IsEnabledAsync(string featureKey, Guid tenantId, CancellationToken ct)`
- `Task<int?> GetLimitAsync(string featureKey, Guid tenantId, CancellationToken ct)`

Each service consumes this abstraction before executing guarded handlers.

## 6.2 Back the abstraction via billing-service API or cached projection

Preferred rollout path:

### Phase 1
- billing-service exposes entitlement read endpoint(s)
- downstream services call billing-service through an internal client
- add short TTL cache to avoid hammering billing-service

### Phase 2
- billing publishes `billing.subscription.created`, `billing.subscription.cancelled`, `billing.subscription.updated`, `billing.entitlements.changed`
- downstream services maintain local entitlement read cache/projection if needed

This aligns with ADR-003 (database-per-service) and ADR-002 (outbox/events).

---

# 7. Enforcement layers

Use multiple layers, not just one.

## 7.1 Gateway layer

Purpose:
- cheap coarse blocking for clearly gated routes
- reduce waste before requests hit downstream services

Good for:
- entire route families that are unavailable on lower plans
- portal asset management endpoints
- premium admin/reporting endpoints

Do not rely only on gateway checks because:
- service handlers still need protection
- internal jobs and service-to-service calls can bypass gateway

## 7.2 Service/API layer

Controllers or endpoint filters can reject obviously gated requests early.

Example:
- `POST /travel/quotations` checks `travel.quotation.create`

## 7.3 Handler layer (mandatory)

Every protected command/query handler must enforce the feature internally.

This is the real boundary.

Example:
- `CreateQuotationCommandHandler` calls `EnsureEnabledAsync("travel.quotation.create", tenantId)`
- `UploadBookingDocumentCommandHandler` calls `EnsureEnabledAsync("travel.booking.documents.upload", tenantId)`

## 7.4 Job/background layer

Schedulers, message consumers, and outbox-driven workflows must also enforce entitlements.

Example:
- if a tenant loses `communication.notification.send`, queued campaign sends for that tenant should pause or fail gracefully
- if a tenant loses `travel.public-share`, public quotation generation should stop for new links

---

# 8. Failure behavior

## API response
Use clear responses.

Recommended:
- `403 Forbidden` for entitlement missing
- response body:
```json
{
  "error": "feature_not_enabled",
  "featureKey": "travel.quotation.create",
  "message": "Your current subscription does not include quotation creation."
}
```

## UI behavior
Frontend should:
- hide unavailable actions when possible
- but never rely on hiding alone
- show upgrade/contact-sales message when backend rejects

---

# 9. Feature-key naming convention

Use dot-separated keys:
- `<service>.<domain>.<action>`

Examples:
- `travel.quotation.create`
- `travel.quotation.send`
- `travel.quotation.attachments.upload`
- `travel.booking.create`
- `travel.booking.documents.upload`
- `travel.timeline.read`
- `travel.audit.read`
- `travel.notes.write`
- `communication.notification.send`
- `branding.theme.manage`
- `branding.assets.manage`

Keep names stable; treat them as contracts.

---

# 10. Phase-wise implementation plan

## Phase E1 - entitlement foundation in billing-service

### Goal
Create the canonical entitlement model.

### Add
- `FeatureEntitlement` model/table
- plan-to-feature resolution logic
- billing read API for effective entitlements
- billing tests

### New endpoints
- `GET /billing/entitlements/me`
- `GET /billing/entitlements/{tenantId}` (internal/admin only)

### Output
Billing can answer: what can tenant X do right now?

---

## Phase E2 - shared contract + service abstraction

### Goal
Make services able to ask entitlement questions cleanly.

### Add
- shared feature-key constants package or shared contracts file
- `IFeatureGate` abstraction in each service
- billing entitlement client
- cache-backed resolver

### Output
Travel, communication, identity, and webhook services can enforce gates consistently.

---

## Phase E3 - travel-service enforcement

### Goal
Protect the most commercially important product features.

### Guard these first

#### Quotations
- `POST /travel/quotations`
- `PUT /travel/quotations/{id}`
- `POST /travel/quotations/{id}/revisions`
- `POST /travel/quotations/{id}/attachments`
- `DELETE /travel/quotations/{id}/attachments/{attachmentId}`
- `POST /travel/quotations/{id}/send`
- `POST /travel/quotations/{id}/accept`
- `POST /travel/quotations/{id}/reject`
- public share creation inside send flow

#### Bookings
- `POST /travel/bookings/from-quotation/{quotationId}`
- `PATCH /travel/bookings/{id}/status`
- `POST /travel/bookings/{id}/cancel`
- traveler/item/document write endpoints

#### Phase 8 features
- timeline read
- notes create/update/delete
- admin audit read

### Example feature map
- `travel.quotation.create`
- `travel.quotation.revisions.write`
- `travel.quotation.attachments.write`
- `travel.quotation.send`
- `travel.booking.create`
- `travel.booking.manage`
- `travel.booking.documents.write`
- `travel.timeline.read`
- `travel.notes.write`
- `travel.audit.read`

---

## Phase E4 - communication-service enforcement

### Goal
Protect messaging/notification features.

### Guard likely capabilities
- send email notifications
- send WhatsApp/SMS notifications
- campaign/bulk messaging
- notification templates management
- delivery logs visibility if plan-gated

### Example feature keys
- `communication.notification.send`
- `communication.templates.manage`
- `communication.bulk.send`
- `communication.logs.read`

---

## Phase E5 - identity/admin enforcement

### Goal
Separate core auth from premium organization controls.

### Possible gated areas
- advanced RBAC
- audit exports
- SSO / SAML / enterprise identity
- admin analytics

### Example feature keys
- `identity.rbac.advanced`
- `identity.sso.manage`
- `identity.audit.export`

---

## Phase E6 - gateway route awareness

### Goal
Add coarse route blocking where useful.

### Add
- route-to-feature config map in api-gateway
- middleware or transform step that resolves feature requirement
- optional caching by tenant

### Important
Gateway enforcement is optimization, not the sole defense.

---

# 11. Endpoint coverage guidance by service

## api-gateway
Should understand:
- tenant identity
- route metadata
- coarse entitlement requirement

Should not own:
- commercial truth
- plan resolution logic beyond cached calls

## billing-service
Should own:
- plan definitions
- entitlement resolution
- overrides
- billing/admin APIs for plan state

## travel-service
Should enforce:
- create/update/delete commands
- premium read APIs like audit/timeline if needed by plan
- share/send/public link generation rules

## communication-service
Should enforce:
- outbound send actions
- premium template/editing/logging capabilities

## identity-service
Should enforce:
- premium admin/security capabilities if productized

## webhook-service
Usually less plan-gated directly, but can enforce:
- outbound webhook delivery feature
- event subscriptions per tenant
- webhook retry/replay visibility

---

# 12. Limits and quotas

Some features are binary. Some are quota-based.

## Binary examples
- can create quotations
- can read audit logs
- can upload documents

## Quota examples
- max quotations per month
- max attachments per quotation
- max active users
- max notifications per month
- max storage bytes for branding assets

Use `LimitValue` in entitlement records or a parallel quota model.

---

# 13. Public and indirect flows

Do not forget indirect feature usage.

## Example: quote share links
If tenant lacks `travel.quotation.send`, they must not be able to:
- create new share links
- trigger quote send endpoint

But existing public links need a policy:
- either remain valid until expiry
- or become invalid immediately when plan loses entitlement

Recommended first pass:
- block new link creation immediately
- do not retro-break already-sent public links unless business requires it

---

# 14. Data model suggestions

## Billing-service new tables

### `feature_entitlements`
- `id`
- `tenant_id`
- `feature_key`
- `granted`
- `source`
- `plan_type`
- `limit_value`
- `effective_from`
- `effective_to`
- `metadata_json`
- `created_at`
- `updated_at`

### optional `plan_feature_defaults`
- `id`
- `plan_type`
- `feature_key`
- `granted`
- `limit_value`
- `created_at`
- `updated_at`

### optional `tenant_feature_overrides`
- `id`
- `tenant_id`
- `feature_key`
- `granted`
- `limit_value`
- `reason`
- `created_at`
- `updated_at`

---

# 15. CQRS integration pattern

## Commands
Protected command handlers should call feature gate before aggregate mutation.

## Queries
Protect premium queries too.
Example:
- audit logs
- export endpoints
- advanced reporting

## Read model projection jobs
Some projections may continue to build regardless of tenant plan, but access to those reads should still be controlled.

---

# 16. Suggested rollout by PRs

## PR E1 - billing entitlement model
- tables
- domain/service logic
- internal read endpoint

## PR E2 - shared feature keys + resolver client
- shared constants
- service-level resolver abstraction
- cache support

## PR E3 - travel-service write endpoint enforcement
- quotation + booking + notes + audit checks
- tests

## PR E4 - communication and gateway enforcement
- notification/template gating
- route metadata checks

## PR E5 - quotas + overrides + docs
- quota support
- admin override model
- docs/postman

---

# 17. Minimum tests required

## Billing
- entitlement resolution by plan
- override beats default
- expired override no longer applies

## Travel
- tenant without `travel.quotation.create` gets 403
- tenant without `travel.booking.create` cannot create booking from quote
- tenant without `travel.notes.write` cannot create notes
- tenant without `travel.audit.read` cannot access audit endpoint

## Communication
- tenant without send entitlement cannot send notifications

## Gateway
- mapped premium route blocks before proxy when entitlement missing

---

# 18. Definition of done

This work is done only when:
- plan state resolves to explicit feature entitlements
- protected endpoints enforce required features in handlers
- gateway optionally performs coarse route blocking
- premium flows fail with clear 403 errors
- tests cover missing-entitlement scenarios
- docs list which endpoints depend on which feature keys

---

# 19. Final blunt recommendation

Do not implement this as scattered `if (plan == Pro)` checks all over the repo.
That becomes unmaintainable garbage fast.

Implement:
- billing-owned entitlement source of truth
- service-local `IFeatureGate`
- stable feature keys
- handler-level enforcement
- optional gateway optimization

That gives you product control without violating the current architecture.
