# Implementation Spec - Flexible Feature Entitlements Rollout & Cross-Service Migration

_Last updated: 2026-04-10_

This document is the rollout follow-up to:
- `docs/implementation_spec_flexible_feature_entitlements.md`
- `docs/implementation_spec_flexible_feature_entitlements_billing_service.md`

Its purpose is to define how Voyara moves from:
- rigid plan-driven gating

to:
- feature-first commercial entitlements enforced consistently across services.

---

# 1. Rollout goal

Blunt goal:
stop having a nice architecture doc while half the product still behaves like random premium logic glued together by convention.

This rollout must make feature entitlements real in:
- billing-service
- api-gateway
- travel-service
- communication-service
- identity-service (for premium admin/security features)
- docs/tests/postman

---

# 2. Rollout principles

## 2.1 Backward-compatible first
Do not break existing tenants while migrating away from `PlanType` assumptions.

## 2.2 Handler enforcement is mandatory
Gateway checks are optional optimization.
Real protection belongs in handlers/services.

## 2.3 Every premium capability needs an explicit feature key
No more hidden premium behavior living only in UI or service folklore.

## 2.4 Public flows must also respect entitlements
Public quote/customer flows must resolve tenant and enforce feature state.

---

# 3. Cross-service rollout phases

## Phase R1 - billing-service foundation
Deliver:
- feature catalog
- package catalog
- tenant package assignments
- tenant overrides
- DB-backed effective entitlement resolution

Exit criteria:
- billing can answer the effective feature set for any tenant without a hardcoded plan switch

## Phase R2 - shared feature contract cleanup
Deliver:
- shared feature-key constants/contracts
- stable `IFeatureGate` usage pattern across services
- cached billing entitlement client

Exit criteria:
- services ask one clean question: is feature X enabled for tenant Y?

## Phase R3 - travel-service full commercial wrapping
Required feature boundaries:
- quotation create/update/send/share
- booking create/manage
- attachments/documents if premium
- notes write
- timeline read
- audit/report/export reads
- public quotation decision flows

Exit criteria:
- no meaningful premium travel workflow bypasses entitlement checks

## Phase R4 - communication-service full commercial wrapping
Required feature boundaries:
- notification send
- bulk send
- template management
- log visibility if premium

Exit criteria:
- outbound communication costs/features are commercially governed

## Phase R5 - identity premium feature wrapping
Required feature boundaries where productized:
- advanced RBAC
- audit export
- SSO/domain management
- impersonation/support access

Exit criteria:
- identity premium admin/security features are commercially gated cleanly

## Phase R6 - gateway route awareness
Deliver:
- route-to-feature metadata
- optional preflight entitlement blocking for coarse premium routes

Exit criteria:
- obvious premium routes can fail early without replacing service enforcement

## Phase R7 - docs, tests, and admin visibility
Deliver:
- feature-to-endpoint catalog
- postman examples
- admin/internal entitlement inspection docs
- migration notes

Exit criteria:
- humans can understand and operate the system without reverse-engineering code

---

# 4. Required endpoint audit checklist

## Travel-service
Must audit all handlers/controllers for:
- quote create/edit/send/share
- public quote accept/reject
- booking creation and status changes
- notes create/update/delete
- timeline reads
- audit/reporting/export reads
- follow-up/work queue actions if sold as premium

## Communication-service
Must audit for:
- send message/notification
- bulk/campaign operations
- template CRUD
- delivery log/reporting reads

## Identity-service
Must audit for:
- advanced RBAC actions
- audit export
- SSO/domain management
- support impersonation

## API Gateway
Must audit for:
- any route families worth coarse blocking
- internal-only vs public route handling

---

# 5. Feature wrapping rules

For each feature:
1. define the feature key
2. register it in catalog
3. seed package mapping if applicable
4. document controller/handler usage
5. enforce in handler/service
6. add negative tests for missing entitlement
7. add positive tests for enabled entitlement

If any of those are missing, the feature wrapping is incomplete.

---

# 6. Public flow rule

For any public flow:
- resolve resource owner tenant
- evaluate tenant entitlements
- only then execute action

Examples:
- public quotation acceptance/rejection
- future customer portals
- branded public booking/payment links

Public must never mean bypass.

---

# 7. Migration safety strategy

## Keep legacy package compatibility
Seed:
- `legacy.free`
- `legacy.pro`
- `legacy.enterprise`

That lets old tenants behave the same while architecture evolves.

## Migrate incrementally
Do not wait for every service to be perfect before switching billing to DB-backed entitlements.
Start with compatibility packages and move service enforcement in phases.

## Keep tests around old and new behavior
Need both:
- legacy plan-equivalent expectations
- new flexible package expectations

---

# 8. Required documentation outputs

As rollout progresses, maintain:
- master flexible entitlement spec
- billing-service design spec
- rollout/migration spec
- feature catalog doc
- per-service endpoint-to-feature map where needed
- postman examples for failure/success cases

---

# 9. Definition of done

Rollout is done only when:
- billing owns DB-backed flexible entitlement truth
- all meaningful premium features are mapped to stable feature keys
- handlers enforce entitlements across services
- public flows cannot bypass tenant commercial rules
- docs and tests reflect real enforcement behavior
- rigid three-plan hardcoding is no longer the real technical control surface
