# Implementation Spec - Tenant-Scoped User/Agent Feature Assignments on Top of Flexible Tenant Packages

_Last updated: 2026-04-12_

This document extends the flexible entitlement model so a tenant can buy a package of features and then selectively assign those purchased capabilities to individual internal users/agents.

Blunt goal:
stop treating tenant-level purchase as if every user inside that tenant should automatically get every purchased feature.
That is lazy SaaS modeling.

A tenant may buy 4 features and decide:
- agent A gets 2 of them
- agent B gets a different 2
- admin gets everything
- everyone else sees unassigned features as disabled

That requires a second layer of assignment on top of tenant commercial entitlement.

This spec does **not** replace tenant entitlement resolution.
It adds **seat/user-level feature allocation inside the tenant boundary**.

---

# 1. Problem statement

Current flexible entitlement direction solves:
- what the tenant has commercially purchased
- what package/add-on/override grants at tenant level

It does **not yet** solve:
- which internal users/agents may actually use which purchased features
- how partial assignment works when tenant wants selective enablement
- how to disable purchased features for some users without removing the tenant subscription entirely

Real SaaS behavior needed:
- tenant buys premium capabilities
- tenant admin allocates capabilities to specific users/agents
- non-assigned users cannot use those capabilities even though the tenant owns them
- some features may be unrestricted for all tenant users, while others may be assignment-controlled

---

# 2. Final access rule

A user action should be allowed only if:

```text
user permission allows it
AND
tenant commercially owns the feature
AND
user/agent is assigned the feature (when assignment is required)
```

That means three distinct layers:

## 2.1 Permission layer
Identity/service authorization.
Example:
- can this user manage quotations?
- can this user access bookings?

## 2.2 Tenant commercial entitlement layer
Billing/commercial truth.
Example:
- did this tenant buy `travel.booking.create`?
- did this tenant buy `branding.theme.manage`?

## 2.3 User assignment layer
Tenant-admin allocation truth.
Example:
- within this tenant, is user X assigned `travel.booking.create`?

All three must stay separate.

---

# 3. Scope

This spec covers:
- internal tenant users and agents
- assignment of tenant-owned features to individual users
- optional quantity/seat policies
- APIs and enforcement model
- admin UX/backend contract direction

This spec does not cover:
- public customer-facing users
- arbitrary consumer marketplace billing
- cross-tenant shared seats

---

# 4. Terminology

## Tenant feature entitlement
A feature the tenant has purchased or been granted commercially.

## Assignable feature
A tenant-owned feature that requires explicit user/agent assignment before an internal user can use it.

## Auto-available feature
A tenant-owned feature that becomes available to all internal users automatically, subject to permissions.

## User feature assignment
A tenant-scoped grant of an assignable feature to a specific internal user/agent.

## Allocation policy
The rule that determines whether a feature:
- is unlimited across tenant users
- requires explicit assignment
- is limited by seat count or quantity

---

# 5. Design principles

## 5.1 Tenant purchase is not the same as user access
A tenant owning a feature does not automatically mean every user should get it.

## 5.2 Assignment policy must be data-driven
Do not hardcode random special cases like:
- this feature is manually assigned in UI only
- that feature is globally on for everyone

Put policy in the catalog/model.

## 5.3 Commercial truth stays in billing-service
Even if identity-service stores user metadata, billing/commercial domain should still own:
- whether feature exists commercially
- whether feature is assignable
- seat/allocation policy
- assignment limits

## 5.4 Enforcement should be composable and explicit
Services should evaluate one effective access result, not hand-roll commercial + assignment logic in every handler.

## 5.5 Admins need visibility
Tenant admins must be able to answer:
- what did we buy?
- which features are assignable?
- who currently has what?
- which seats are still available?

---

# 6. Feature assignment model

A feature in the catalog should define an allocation mode.

## Suggested allocation modes
- `TenantWide` — all internal tenant users may use it if they have permission
- `ExplicitUserAssignment` — must be assigned per user/agent
- `SeatLimitedAssignment` — explicit assignment required and total assignments capped
- `QuotaShared` — tenant-level quota shared across assigned or all allowed users
- `QuotaPerAssignedUser` — each assigned user gets a personal quota amount

Not every feature needs the same policy.

### Examples
- `travel.timeline.read` -> maybe `TenantWide`
- `travel.booking.create` -> maybe `ExplicitUserAssignment`
- `communication.notification.send` -> maybe `SeatLimitedAssignment` or `QuotaShared`
- `branding.theme.manage` -> likely `ExplicitUserAssignment`
- `identity.audit.export` -> likely `ExplicitUserAssignment`

---

# 7. Data model changes

## 7.1 Extend `FeatureCatalog`
Add fields such as:
- `AssignmentMode`
- `RequiresExplicitAssignment`
- `DefaultAssignmentLimit` (nullable)
- `AssignmentUnit` (`Users`, `Agents`, `Seats`, `None`)
- `CanBeAssignedToRolesInsteadOfUsers` (future, optional)
- `MetadataJson`

This keeps assignment behavior tied to the feature definition.

---

## 7.2 `TenantFeatureAllocation`
Represents tenant-level allocatable settings for a feature.

Suggested fields:
- `Id`
- `TenantId`
- `FeatureKey`
- `AssignmentMode`
- `MaxAssignments` (nullable)
- `AssignmentCount` (derived or cached)
- `IsAssignmentRequired`
- `InheritedFromPackage` (bool)
- `SourcePackageId` (nullable)
- `CreatedAt`
- `UpdatedAt`

Purpose:
- converts commercial purchase into tenant-usable assignment policy
- lets package/default values be overridden at tenant level if allowed

---

## 7.3 `TenantUserFeatureAssignment`
Represents assigning a feature to a specific internal user/agent.

Suggested fields:
- `Id`
- `TenantId`
- `UserId`
- `FeatureKey`
- `Status` (`Active`, `Revoked`, `Expired`)
- `AssignedByUserId`
- `AssignedAt`
- `RevokedByUserId` (nullable)
- `RevokedAt` (nullable)
- `EffectiveFrom`
- `EffectiveTo`
- `Notes` (nullable)
- `MetadataJson`

Constraints:
- unique active assignment for (`TenantId`, `UserId`, `FeatureKey`)
- tenant boundary enforced always

---

## 7.4 Optional `TenantRoleFeatureAssignment` (future)
If later needed, tenant can assign feature to role instead of individual user.

Suggested fields:
- `Id`
- `TenantId`
- `RoleId`
- `FeatureKey`
- `Status`
- `AssignedAt`
- `AssignedByUserId`

This is optional for later.
Do **not** start here unless role-based assignment is already a product need.

---

## 7.5 Effective read model: `UserEffectiveFeatureAccess`
Suggested fields:
- `TenantId`
- `UserId`
- `FeatureKey`
- `TenantGranted`
- `UserAssigned`
- `AssignmentRequired`
- `Granted`
- `LimitValue`
- `ResolvedFromJson`
- `UpdatedAt`

This can be computed or projected.
Services should preferably consume this effective result instead of rebuilding logic every time.

---

# 8. Resolution algorithm

For any `tenantId + userId + featureKey`:

## Step 1 - check tenant commercial entitlement
Resolve tenant effective entitlement from packages + add-ons + overrides.
If tenant does not own the feature -> deny immediately.

## Step 2 - load feature assignment policy
Read assignment mode from feature catalog / tenant allocation.

## Step 3 - resolve user assignment if required
If feature is `TenantWide`:
- `UserAssigned = true` by policy

If feature requires explicit assignment:
- check `TenantUserFeatureAssignment`
- active assignment must exist within effective window

## Step 4 - combine

```text
Granted = TenantGranted AND (AssignmentNotRequired OR UserAssigned)
```

## Step 5 - apply permission check in service layer
Even if feature access is granted, user still needs the relevant permission/role authorization.

---

# 9. Example behavior

## Example A - tenant buys 4 features
Tenant owns:
- `travel.quotation.create`
- `travel.booking.create`
- `branding.theme.manage`
- `communication.notification.send`

Assignments:
- Agent A -> quotation + booking
- Agent B -> notification send + quotation
- Admin -> all 4
- Agent C -> none

Result:
- Agent C sees all 4 as disabled/unavailable
- Agent A cannot manage branding
- Agent B cannot create bookings

That is exactly the intended product behavior.

## Example B - tenant-wide feature
Tenant owns `travel.timeline.read` with assignment mode `TenantWide`.

Result:
- any permitted internal user can access timeline
- no individual assignment records needed

## Example C - seat-limited feature
Tenant buys package with `communication.notification.send` limited to 2 assigned users.

Assignments:
- User 1 active
- User 2 active
- User 3 assignment attempt -> rejected until one seat is revoked or upgraded

---

# 10. API design

Billing-service should own commercial + assignment administration APIs, potentially with internal coordination with identity-service for user lookup.

## 10.1 Tenant feature availability APIs

### GET `/billing/tenants/{tenantId}/feature-allocations`
Returns tenant-owned features and how they can be assigned.

Response example:
```json
[
  {
    "featureKey": "travel.booking.create",
    "tenantGranted": true,
    "assignmentMode": "ExplicitUserAssignment",
    "maxAssignments": 2,
    "activeAssignments": 1,
    "remainingAssignments": 1
  }
]
```

### GET `/billing/tenants/{tenantId}/users/{userId}/features`
Returns effective feature access for one user.

### GET `/billing/tenants/{tenantId}/feature-assignments`
Query all assignments with filters by feature/user/status.

---

## 10.2 Assignment mutation APIs

### POST `/billing/tenants/{tenantId}/users/{userId}/feature-assignments`
Assign one or more features to a user.

Request example:
```json
{
  "featureKeys": [
    "travel.quotation.create",
    "travel.booking.create"
  ],
  "effectiveFrom": "2026-04-12T00:00:00Z"
}
```

### DELETE `/billing/tenants/{tenantId}/users/{userId}/feature-assignments/{featureKey}`
Revoke active assignment.

### POST `/billing/tenants/{tenantId}/feature-assignments/bulk`
Bulk assign/revoke features across multiple users.

Request example:
```json
{
  "operation": "Assign",
  "featureKey": "communication.notification.send",
  "userIds": ["user-1", "user-2"]
}
```

---

## 10.3 Self-visibility APIs

### GET `/billing/feature-access/me`
Returns current authenticated user’s effective feature access.
Useful for portal shell/UI to hide unavailable modules cleanly.

### GET `/billing/feature-access/me/{featureKey}`
Lightweight endpoint for targeted checks if needed.

---

# 11. Validation rules

## Assignment creation must fail if:
- tenant does not own the feature
- feature is not assignable
- maximum assignment limit has been reached
- user does not belong to the tenant
- duplicate active assignment already exists
- assignment window is invalid

## Assignment revoke must:
- preserve audit trail
- not hard-delete unless policy explicitly allows

## Admin authorization required
Only tenant admins or authorized tenant managers should be able to manage assignments.

---

# 12. Service enforcement pattern

Services should not manually inspect raw assignment tables.
They should use a shared abstraction such as:
- `IUserFeatureAccessGate`

Suggested methods:
- `EnsureUserFeatureEnabledAsync(tenantId, userId, featureKey)`
- `IsUserFeatureEnabledAsync(tenantId, userId, featureKey)`
- `GetUserFeatureAccessAsync(tenantId, userId, featureKey)`

Evaluation should internally combine:
- tenant entitlement
- assignment requirement
- user assignment result

Permission checks still remain in service/identity authorization layer.

---

# 13. UI/portal behavior expectations

Tenant admin UI should show:
- purchased features
- assignment mode for each
- seats/limits available
- current assignees
- easy assign/revoke actions

End-user UI should:
- hide or disable modules not assigned
- explain why feature is unavailable when relevant
- avoid surfacing paid-but-unassigned features as broken links

Suggested UX messages:
- "Not included in your tenant subscription"
- "Available to your organization but not assigned to your account"
- "Ask your tenant admin for access"

That distinction matters.

---

# 14. Identity-service interaction

Identity-service remains source of truth for:
- tenant users
- roles
- agent/admin identities
- permissions

Billing-service or entitlement domain should integrate with identity-service for:
- validating that user belongs to tenant
- optionally listing tenant users for assignment UX

Avoid duplicating identity ownership into billing.

---

# 15. Postman/API workflow implications

Postman coverage should include:
- tenant owns feature but user unassigned -> expect 403/feature-disabled response
- tenant owns feature and user assigned -> success
- tenant does not own feature -> expect commercial entitlement failure
- assignment creation/revocation flows
- self-access endpoint validation

These flows are expanded in the dedicated Postman spec.

---

# 16. Suggested implementation phases

## Phase UFA1 - model + APIs
- extend feature catalog with assignment policy
- create allocation and assignment tables
- add admin assignment APIs
- add self-access query endpoint

## Phase UFA2 - effective access resolution
- build user effective feature access resolver
- add caching/read model if needed
- add audit logging

## Phase UFA3 - service enforcement
- travel-service: enforce on premium workflows
- communication-service: enforce on send/template features
- identity-service: enforce on premium admin/security features
- frontend: consume `/billing/feature-access/me`

## Phase UFA4 - UX and bulk admin tooling
- bulk assign/revoke
- assignment usage dashboard
- seat exhaustion messages

---

# 17. Tests required

## Billing/entitlements
- tenant-owned + assigned => granted
- tenant-owned + unassigned + assignment required => denied
- tenant-owned + tenant-wide => granted
- seat cap enforced
- revoked/expired assignment ignored
- tenant boundary violation rejected

## Services
- handler returns 403 when feature purchased but not assigned
- handler succeeds when assigned
- permissions still enforced independently

## UI/integration
- `/billing/feature-access/me` drives module visibility correctly
- admin assignment flows update effective access immediately or within acceptable cache delay

---

# 18. Definition of done

This feature is done only when:
- tenant purchase and user assignment are modeled separately
- assignable features can be granted to specific tenant users/agents
- non-assigned users are blocked from purchased-but-unassigned features
- services enforce effective user feature access consistently
- admin APIs exist to manage assignments cleanly
- docs/tests/postman reflect the real access model

---

# 19. Final blunt recommendation

Do not fake this with UI toggles.
If the backend still says "tenant owns it so everyone can use it," then the product model is lying.

Build the real chain:
- tenant buys feature
- tenant admin allocates feature
- assigned user gets access
- non-assigned user does not

That is the sane SaaS design.