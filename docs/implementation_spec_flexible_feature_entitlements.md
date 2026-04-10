# Implementation Spec - Flexible SaaS Feature Entitlements (Feature-First, Not Plan-First)

_Last updated: 2026-04-10_

This document replaces the simplistic mental model of:
- `Free`
- `Pro`
- `Enterprise`

as the primary control surface for Voyara capabilities.

Blunt goal:
Voyara should behave like a **real SaaS platform with flexible commercial packaging**, where a tenant can pay to unlock exactly the features, bundles, quotas, and limits they need — without hardcoding the product into three rigid plan buckets forever.

That means:
- every meaningful product capability should become a named feature
- features should be resolved from data, not from giant `switch(planType)` blocks scattered around the codebase
- services should enforce feature entitlements consistently
- plans become optional commercial bundles, not the technical source of truth

---

# 1. Core problem

Current state (bad long-term shape):
- billing knows `PlanType`
- entitlement resolution often still behaves like `Free/Pro/Enterprise => fixed features`
- services check feature keys, but the feature set is still effectively hardcoded through plan buckets

That is okay for an early MVP.
It is **not okay** for a serious SaaS product that wants:
- custom packages
- add-ons
- à la carte features
- temporary grants
- grandfathered entitlements
- tenant-specific negotiated contracts
- usage-based limits
- flexible product evolution

If we keep the current shape too long, pricing logic becomes technical debt welded into the product.

---

# 2. Desired end state

Voyara should move to a **feature-first entitlement model**.

Meaning:
- a tenant has an **effective feature set**
- a feature can be granted by one or more commercial sources
- services only care whether a feature is enabled / limited / expired
- plans are just one possible source of entitlement, not the hardcoded truth

## Final decision model

An action should be allowed only if:

```text
user permission allows it
AND
tenant feature entitlement allows it
```

Where:
- identity-service answers: **who is allowed to perform this action?**
- billing/entitlement system answers: **has this tenant commercially unlocked this capability?**

Both layers stay separate.

---

# 3. Design principles

## 3.1 Every meaningful product capability becomes a feature key

Do not rely on vague plan names inside services.

Examples:
- `travel.quotation.create`
- `travel.quotation.send`
- `travel.booking.create`
- `travel.timeline.read`
- `travel.notes.write`
- `travel.audit.read`
- `communication.notification.send`
- `branding.theme.manage`
- `branding.assets.manage`
- `identity.audit.export`
- `identity.sso.manage`
- `identity.rbac.advanced`

Feature keys are the stable contract.

## 3.2 Plans become bundles, not logic branches

A plan like `Pro` may still exist commercially.
But technically it should mean:
- “this bundle grants these features by default”

not:
- “every service hardcodes `if (plan == Pro)`”

## 3.3 Entitlement data must be DB-backed and composable

A tenant’s effective features should be resolvable from:
- base commercial bundle(s)
- direct feature purchases
- add-ons
- temporary trial grants
- admin/sales overrides
- quota limits
- expiry windows

## 3.4 Services must never own pricing truth

Services may know feature keys.
They must not own:
- plan mapping
- sales exceptions
- contract packaging
- billing policy

Billing (or a dedicated entitlement domain under billing ownership) must own that truth.

---

# 4. Feature-first target model

## 4.1 `FeatureCatalog`

Central registry of every technical capability that can be commercially controlled.

Suggested fields:
- `FeatureKey`
- `Service`
- `Category`
- `DisplayName`
- `Description`
- `IsEnabledByDefault`
- `IsQuota`
- `Unit`
- `MetadataJson`
- `CreatedAt`
- `UpdatedAt`

Examples:
- `travel.quotation.create`
- `travel.quotation.send`
- `travel.booking.create`
- `communication.notification.send`
- `branding.assets.manage`

Purpose:
- stop feature-key drift
- validate known keys
- support admin UI and sales tooling later
- document which features exist

---

## 4.2 `CommercialPackage`

This replaces the simplistic idea of rigid plans as the only packaging unit.

A package can be:
- full bundle
- add-on
- vertical module
- premium capability pack
- trial pack

Suggested fields:
- `Id`
- `Code`
- `Name`
- `Category` (`BasePlan`, `Addon`, `Trial`, `Promo`, `LegacyBundle`)
- `Description`
- `IsActive`
- `BillingModel` (`Flat`, `UsageBased`, `PerSeat`, `OneTime`, `Contract`)
- `MetadataJson`
- `CreatedAt`
- `UpdatedAt`

Examples:
- `base.starter`
- `base.operations`
- `addon.audit-plus`
- `addon.branding-pro`
- `addon.notifications-10k`
- `promo.q2-trial`

---

## 4.3 `CommercialPackageFeature`

Maps a package to the features it grants.

Suggested fields:
- `Id`
- `CommercialPackageId`
- `FeatureKey`
- `Granted`
- `LimitValue` (nullable)
- `MetadataJson`
- `CreatedAt`
- `UpdatedAt`

Examples:
- `base.starter` grants `travel.quotation.create`
- `addon.audit-plus` grants `travel.audit.read`
- `addon.notifications-10k` grants `communication.notification.send` with `LimitValue=10000`

---

## 4.4 `TenantSubscriptionPackage`

Represents which commercial packages a tenant currently has.

Suggested fields:
- `Id`
- `TenantId`
- `CommercialPackageId`
- `Source` (`Subscription`, `ManualGrant`, `Contract`, `Trial`, `Promo`, `Migration`)
- `Status` (`Active`, `Scheduled`, `Expired`, `Cancelled`)
- `EffectiveFrom`
- `EffectiveTo`
- `MetadataJson`
- `CreatedAt`
- `UpdatedAt`

A tenant may have:
- one base package
- multiple add-ons
- temporary promotional package

This is the core flexibility missing today.

---

## 4.5 `TenantFeatureOverride`

Direct per-tenant adjustment when reality gets messy.

Suggested fields:
- `Id`
- `TenantId`
- `FeatureKey`
- `Granted`
- `LimitValue`
- `Reason`
- `Source` (`Support`, `Sales`, `Migration`, `AdminGrant`, `IncidentCompensation`)
- `EffectiveFrom`
- `EffectiveTo`
- `CreatedBy`
- `CreatedAt`
- `UpdatedAt`

Purpose:
- negotiated contracts
- temporary fixes
- migration parity
- short-term grants without inventing fake plan types

---

## 4.6 `EffectiveFeatureEntitlement`

Resolved result used by services.

Suggested fields:
- `TenantId`
- `FeatureKey`
- `Granted`
- `LimitValue`
- `ResolvedFromJson`
- `EffectiveFrom`
- `EffectiveTo`
- `UpdatedAt`

This can be:
- computed live
- cached
- projected
- stored as read model

Services should consume the **effective** result, not recreate the commercial merge logic themselves.

---

# 5. Replace the current hardcoded resolver mindset

Current anti-pattern:

```csharp
private static IReadOnlyList<(string FeatureKey, bool Granted, int? LimitValue)> GetDefinitions(PlanType planType)
```

Why it is not enough:
- assumes one tenant = one rigid plan bucket
- does not support add-ons cleanly
- does not support custom contracts cleanly
- does not support multiple commercial sources composing together
- makes packaging changes require code changes
- turns pricing into deployment work

## Recommended replacement

Instead of:

```text
PlanType -> hardcoded feature matrix
```

Use:

```text
TenantSubscriptionPackage(s)
+ CommercialPackageFeature(s)
+ TenantFeatureOverride(s)
--------------------------------
= EffectiveFeatureEntitlement(s)
```

That is the correct SaaS shape.

---

# 6. What “wrap everything in features” really means

It does **not** mean feature flags for every tiny line of code.
That would be cursed.

It means every **commercially meaningful capability boundary** gets a stable feature key.

## Good candidates
- create quotation
- send quotation
- create booking
- read audit timeline
- create notes
- send notifications
- manage branding
- manage advanced RBAC
- export data
- enable SSO

## Bad candidates
- open modal
- show button color
- internal helper method
- micro UI behaviors with no commercial meaning

Think in terms of:
- sellable capability
- upgradeable capability
- limitable capability
- support-overridable capability

---

# 7. Layered SaaS access model

## 7.1 Identity layer (who)
Owned by identity-service.

Examples:
- `identity.users.manage`
- `identity.roles.manage`
- `branding.theme.manage`

This decides:
- does this user have authority to attempt the action?

## 7.2 Entitlement layer (whether tenant bought it)
Owned by billing/entitlement domain.

Examples:
- `travel.quotation.send`
- `travel.audit.read`
- `communication.notification.send`

This decides:
- has the tenant unlocked this capability commercially?

## 7.3 Final rule

```text
ALLOW only if:
user permission check passes
AND tenant entitlement check passes
```

This separation must remain clean.

---

# 8. Architecture fit with current repo

Current repo characteristics:
- api-gateway forwards tenant context
- services are database-per-service
- CQRS + MediatR in service boundaries
- billing-service already owns subscription state
- identity-service now owns roles/permissions/MFA/session state

Best fit:

## 8.1 Billing-service owns feature-commercial truth
Billing should own:
- feature catalog
- package catalog
- package→feature mapping
- tenant package assignments
- tenant feature overrides
- effective entitlement resolution

## 8.2 Other services consume effective entitlements
Each service should use an abstraction like:
- `IFeatureGate`

Methods:
- `EnsureEnabledAsync(featureKey, tenantId)`
- `IsEnabledAsync(featureKey, tenantId)`
- `GetLimitAsync(featureKey, tenantId)`

## 8.3 API Gateway may do coarse route blocking
Useful as optimization only.
Real protection still belongs in handlers/services.

---

# 9. New data model proposal for billing-service

## 9.1 Tables

### `feature_catalog`
- `feature_key` PK
- `service`
- `category`
- `display_name`
- `description`
- `is_enabled_by_default`
- `is_quota`
- `unit`
- `metadata_json`
- `created_at`
- `updated_at`

### `commercial_packages`
- `id`
- `code`
- `name`
- `category`
- `description`
- `billing_model`
- `is_active`
- `metadata_json`
- `created_at`
- `updated_at`

### `commercial_package_features`
- `id`
- `commercial_package_id`
- `feature_key`
- `granted`
- `limit_value`
- `metadata_json`
- `created_at`
- `updated_at`

### `tenant_subscription_packages`
- `id`
- `tenant_id`
- `commercial_package_id`
- `source`
- `status`
- `effective_from`
- `effective_to`
- `metadata_json`
- `created_at`
- `updated_at`

### `tenant_feature_overrides`
- `id`
- `tenant_id`
- `feature_key`
- `granted`
- `limit_value`
- `reason`
- `source`
- `effective_from`
- `effective_to`
- `created_by`
- `created_at`
- `updated_at`

### optional `effective_feature_entitlements`
- `tenant_id`
- `feature_key`
- `granted`
- `limit_value`
- `resolved_from_json`
- `effective_from`
- `effective_to`
- `updated_at`

---

# 10. Resolution algorithm

For each tenant + feature:

## Step 1 - collect active package grants
Load all active `tenant_subscription_packages`.
Resolve package features from `commercial_package_features`.

## Step 2 - merge package grants
If multiple packages contribute to same feature:
- `Granted = any active source grants true`
- `LimitValue = combine according to feature policy`

### Limit merge policy examples
- max of limits
- additive limits
- latest override wins
- explicit merge rule per feature

This policy should be feature-configurable where needed.

## Step 3 - apply tenant overrides
Overrides beat package defaults.

## Step 4 - remove expired records
Anything outside effective window does not count.

## Step 5 - return effective entitlement set
Services consume only the final effective set.

---

# 11. Suggested feature categories

## Travel
- `travel.quotation.create`
- `travel.quotation.send`
- `travel.quotation.attachments.write`
- `travel.booking.create`
- `travel.booking.manage`
- `travel.timeline.read`
- `travel.notes.write`
- `travel.audit.read`
- `travel.export.read`

## Communication
- `communication.notification.send`
- `communication.bulk.send`
- `communication.templates.manage`
- `communication.logs.read`

## Branding
- `branding.theme.manage`
- `branding.assets.manage`
- `branding.templates.manage`

## Identity / Admin
- `identity.rbac.advanced`
- `identity.audit.export`
- `identity.sso.manage`
- `identity.domain.manage`
- `identity.support.impersonation`

## Billing / Commercial admin
- `billing.invoices.read`
- `billing.usage.read`
- `billing.entitlements.manage`

---

# 12. Migration strategy from rigid plans to flexible features

## Phase F1 - Introduce new entitlement tables without breaking current behavior
- keep existing plan-based resolver working
- create `feature_catalog`
- create `commercial_packages`
- create `commercial_package_features`
- seed packages equivalent to current plan buckets
  - e.g. `legacy.free`, `legacy.pro`, `legacy.enterprise`

Goal:
old behavior still works, but data model becomes feature-first.

## Phase F2 - Migrate plan mapping into data
- remove hardcoded `GetDefinitions(PlanType)` feature matrix
- resolve `legacy.free/pro/enterprise` from DB tables instead
- keep `PlanType` only as a commercial compatibility label if needed

Goal:
same behavior, but no more hardcoded plan switch as technical truth.

## Phase F3 - Allow multi-package tenants
- support one base package + multiple add-ons
- add entitlement merge logic
- support quota features

Goal:
tenant can buy exactly what they need.

## Phase F4 - Add tenant override tooling
- admin/sales override APIs
- override audit trail
- expiry windows

Goal:
support negotiated deals and temporary grants without code changes.

## Phase F5 - Expand enforcement everywhere
- audit all services and endpoints
- ensure every meaningful premium capability has a feature key
- enforce in gateway (coarse) + service handler (mandatory)

Goal:
“wrap everything in features” becomes true in practice.

## Phase F6 - UI/admin/commercial tooling
- entitlement admin UI
- package editor
- tenant effective feature viewer
- upgrade impact preview

Goal:
operational usability.

---

# 13. Endpoint and service enforcement guidance

## Billing-service
Must own:
- feature catalog APIs
- package catalog APIs
- package assignment APIs
- override APIs
- effective entitlement resolution APIs

Suggested APIs:
- `GET /billing/features`
- `GET /billing/packages`
- `POST /billing/packages`
- `PUT /billing/packages/{id}`
- `PUT /billing/tenants/{tenantId}/packages`
- `POST /billing/tenants/{tenantId}/feature-overrides`
- `GET /billing/tenants/{tenantId}/entitlements`
- `GET /billing/entitlements/me`

## Travel-service
Must check feature keys in handlers for:
- quotation create/send/edit
- booking create/manage
- timeline read
- notes write
- audit read
- export/reporting actions

## Communication-service
Must check for:
- notification send
- bulk send
- template management
- logs read

## Identity-service
May check premium identity/admin features for:
- advanced RBAC
- audit export
- SSO/domain management
- impersonation

---

# 14. Pure SaaS examples

## Example A - tiny tenant
Tenant buys:
- base CRM starter package
- no audit add-on
- no branding add-on

Effective features:
- can create quotes
- can send quotes
- cannot access audit history
- cannot customize branding assets

## Example B - growing agency
Tenant buys:
- base operations package
- branding add-on
- notifications 10k add-on

Effective features:
- can create bookings
- can manage branding
- can send notifications with monthly limit 10k

## Example C - enterprise contract
Tenant buys:
- custom contract package
- advanced RBAC
- audit export
- SSO
- manual temporary migration grant for premium support feature

Effective features:
- resolved from multiple packages + override
- no code change required

That is the whole point.

---

# 15. Rules for feature-key governance

## Add a new feature only when the capability is commercially meaningful
Good:
- `travel.audit.read`
- `identity.sso.manage`

Bad:
- `travel.button.blue`
- `travel.modal.open`

## Each new feature must be added in all required places
1. `feature_catalog`
2. seed/default package mapping if applicable
3. docs
4. service enforcement points
5. tests

## Feature keys are contracts
Treat them like public API.
Do not rename casually.

---

# 16. Tests required

## Billing / Entitlements
- package-only resolution
- multi-package merge resolution
- override beats package default
- expired override ignored
- quota merge behavior works correctly

## Travel / Communication / Identity services
- missing entitlement returns 403
- enabled entitlement allows action
- quota limit surfaces correctly where relevant
- public flows do not bypass entitlement checks

## Gateway
- coarse blocking works where configured
- gateway does not become sole enforcement point

---

# 17. Definition of done

This feature-first entitlement system is done only when:
- there is no hardcoded plan-feature switch as the technical source of truth
- billing owns DB-backed feature and package definitions
- tenant commercial access resolves from packages + overrides + limits
- services enforce stable feature keys consistently
- feature keys cover all meaningful premium/productized capabilities
- plans are optional bundles, not architectural handcuffs
- docs clearly describe the feature catalog and enforcement model

---

# 18. Final blunt recommendation

Do **not** keep building Voyara around:

```text
Free / Pro / Enterprise
```

as if that is the final architecture.

That is pricing-page thinking, not real SaaS systems thinking.

Build around:
- feature catalog
- package catalog
- tenant package assignments
- direct feature overrides
- effective entitlement resolution
- stable service enforcement

Then you can still sell three plans if you want.
But the product won’t be trapped by them.
