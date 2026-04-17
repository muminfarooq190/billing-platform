# Feature-Based Authorization and Entitlement Audit

_Last updated: 2026-04-17_

This document audits the billing-platform repo for **feature-based authorization / entitlement enforcement**.

Blunt summary:
Yes, the repo has a real architecture for feature-based authorization.
But it is a **layered and partially rolled-out system**, not a perfectly complete and uniformly enforced one everywhere.

The important distinction is:
- permissions answer: **is this user allowed to do this action?**
- entitlements answer: **has this tenant commercially unlocked this capability?**
- user-scoped feature assignments answer: **within a tenant, is this user allocated that purchased capability?**

That separation is present in repo thinking and partially present in repo implementation.

---

# 1. Audit question

## Do we have feature-based authorization?

## Honest answer
**Yes — in architecture, docs, and some live code.**

But:
- not every documented piece appears fully implemented in the current repo snapshot
- enforcement maturity varies by service
- some specs describe a more complete future state than the code currently proves

So the repo has:
- real feature-gate abstractions
- billing-owned entitlement resolution direction
- identity permission-based authorization
- user-aware feature access direction

But it does **not** yet prove a perfectly complete end-to-end rollout across every service and endpoint.

---

# 2. Core access model found in repo

The intended access model is clearly layered.

## Layer 1 - User permission / policy authorization
Identity-service handles who is allowed to do something.

Evidence:
- `RequirePermissionAttribute` exists in identity-service
- permission catalog doc exists
- identity permission specs and catalog separate permission meaning from entitlements

Example from docs:
- `identity.users.manage`
- `identity.roles.manage`
- `travel.quotation.write`
- `billing.invoices.read`

This is action/authority control.

---

## Layer 2 - Tenant commercial entitlement
Billing-service owns commercial feature availability.

Evidence:
- `docs/implementation_spec_flexible_feature_entitlements.md`
- `docs/implementation_spec_service_level_subscription_enforcement.md`
- `services/billing-service/src/Api/Controllers/EntitlementsController.cs`

Implemented controller evidence:
- `GET /billing/entitlements/me`
- `GET /billing/entitlements/{tenantId}`
- tenant grant/package/override mutation endpoints

This is product/package/feature access control.

---

## Layer 3 - User-scoped feature assignment inside tenant
The repo also has design direction for a third layer: tenant-scoped user/agent feature assignment.

Evidence:
- `docs/implementation_spec_tenant_scoped_user_feature_assignments.md`
- memory notes show a branch and implementation slices were done earlier

However, in the current repo snapshot audited here, the exact controller files implied by that spec were not found at the expected paths.

Conclusion:
- this layer is clearly part of platform design
- some implementation likely exists or existed on related branches
- current snapshot does not prove full downstream coverage from the files inspected in this audit alone

---

# 3. Evidence of actual code-level feature gate plumbing

## Travel-service
Confirmed:
- `services/travel-service/src/Application/Abstractions/IFeatureGate.cs`

This abstraction supports:
- tenant-level checks
- user-aware checks
- limit retrieval

Methods include:
- `EnsureEnabledAsync(featureKey, tenantId, ...)`
- `EnsureEnabledAsync(featureKey, tenantId, userId, ...)`
- `IsEnabledAsync(...)`
- `GetLimitAsync(...)`

That is real feature-gate infrastructure, not just theory.

## Communication-service
Confirmed:
- `services/communication-service/src/Application/Abstractions/IFeatureGate.cs`

Same shape as travel-service.

### Audit conclusion
Feature gating is a real shared service pattern in multiple services.

---

# 4. Evidence of real permission-based authorization

Confirmed:
- `services/identity-service/src/Infrastructure/Auth/RequirePermissionAttribute.cs`
- `docs/identity_permission_catalog.md`

This proves the repo has explicit permission-policy based authorization design and implementation.

Important repo design rule from docs:
- permissions and entitlements must stay separate

That is the correct design.

---

# 5. Evidence of billing-owned entitlement APIs

Confirmed file:
- `services/billing-service/src/Api/Controllers/EntitlementsController.cs`

Implemented endpoints include:
- `GET /billing/entitlements/me`
- `GET /billing/entitlements/{tenantId}`
- tenant grant/package/override mutation endpoints

This proves commercial entitlement resolution is not just a comment in docs; it has controller-level surface in billing-service.

---

# 6. What the docs clearly intend

Across the docs, the repo’s intended access rule is:

```text
ALLOW only if:
user permission allows it
AND tenant commercial entitlement allows it
AND (when required) user assignment allows it
```

This is explicitly described in:
- flexible entitlement spec
- tenant-scoped user feature assignment spec
- service-level subscription enforcement spec
- identity permission catalog

Architecturally, that is good and sane.

---

# 7. What is definitely implemented vs what is only partially proven

## Definitely proven by this audit
1. Permission-based authorization exists in identity-service.
2. Billing entitlement APIs exist.
3. Feature-gate abstractions exist in at least travel-service and communication-service.
4. Repo docs clearly separate permission layer from entitlement layer.
5. Repo docs also define user-scoped assignment as the next/extended layer.

## Partially proven / patchy from this snapshot
1. Exact downstream enforcement coverage for every premium handler is not fully proven by this audit.
2. User feature assignment controller/API implementation was not confirmed at the expected file paths in this snapshot.
3. Some files implied by specs (for example a feature access controller or assignment controller) were not found where expected.
4. Some expected concrete `CachedFeatureGate` implementation files were not found at the guessed paths even though the abstraction exists and docs mention them.

### Meaning
The platform has real foundations, but the rollout and file layout are not uniformly obvious from the current snapshot.

---

# 8. Practical interpretation for product/backend

## Yes, you have feature-based authorization in the repo
In the meaningful sense, yes.

Because the repo clearly has:
- permission policies
- entitlement APIs
- service-level feature gate abstraction
- documentation for layered enforcement

## But you do not yet have a fully finished, audit-clean, universally visible enforcement map
That is the missing maturity layer.

What is still needed for a truly strong answer is:
- per-service endpoint inventory
- handler-by-handler enforcement verification
- user-assignment rollout verification
- docs that distinguish current implementation from future target state more clearly

---

# 9. Biggest risk found in this audit

The biggest risk is not that the architecture is wrong.
The architecture is actually decent.

The risk is:
**implementation/documentation drift**

Meaning:
- specs describe a more complete model
- some code proves the model exists
- but enforcement coverage and exact implementation locations are not always easy to verify from the current snapshot

That creates ambiguity about what is actually enforced today versus what is intended soon.

---

# 10. Recommended next steps

## Short-term
1. audit each service for actual handler-level `IFeatureGate` usage
2. produce endpoint-to-feature-key mapping doc
3. verify whether user-scoped feature assignment APIs are present on current branch or another branch
4. document current implementation status vs target-state spec explicitly

## Medium-term
1. add tests for denied-vs-allowed feature behavior in travel-service and communication-service
2. make service enforcement coverage visible in docs/Postman
3. unify where `CachedFeatureGate` / billing client implementations live and document it cleanly

---

# 11. Final blunt conclusion

## Do we have feature-based authorization?
**Yes.**

## Is it architecturally layered the right way?
**Also yes.**

## Is it fully, uniformly, and trivially auditable everywhere right now?
**No — not from this repo snapshot alone.**

What exists is best described as:
- a real feature-authorization foundation
- a real permission/entitlement split
- partial user-assignment rollout direction
- uneven implementation visibility across services

That means the foundation is there, but the repo still needs a stricter implementation coverage audit if you want a crisp “every protected capability is definitely enforced here” answer.
