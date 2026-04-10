# Implementation Spec - Flexible Feature Entitlements Billing-Service Design

_Last updated: 2026-04-10_

This document is the billing-service follow-up to:
- `docs/implementation_spec_flexible_feature_entitlements.md`

Its purpose is to define the concrete billing-service schema, APIs, resolution flow, and migration path needed to support a pure flexible SaaS entitlement model.

Blunt goal:
make billing-service the single commercial source of truth for:
- feature catalog
- package catalog
- tenant package assignments
- tenant feature overrides
- effective feature entitlement resolution

---

# 1. Billing-service responsibilities

Billing-service should own:
- commercial package definitions
- package-to-feature mapping
- tenant active purchased packages
- per-tenant overrides
- quota limits
- effective entitlement computation
- admin/internal APIs for querying and mutating commercial access

Billing-service should not push this logic into other services.
Other services should consume effective entitlements via API/client/cache.

---

# 2. Proposed billing-service domain model

## 2.1 `FeatureCatalogEntry`
Suggested fields:
- `FeatureKey`
- `Service`
- `Category`
- `DisplayName`
- `Description`
- `IsQuota`
- `Unit`
- `MetadataJson`
- `CreatedAt`
- `UpdatedAt`

## 2.2 `CommercialPackage`
Suggested fields:
- `Id`
- `Code`
- `Name`
- `Category`
- `BillingModel`
- `Description`
- `IsActive`
- `MetadataJson`
- `CreatedAt`
- `UpdatedAt`

## 2.3 `CommercialPackageFeature`
Suggested fields:
- `Id`
- `CommercialPackageId`
- `FeatureKey`
- `Granted`
- `LimitValue`
- `LimitMergePolicy`
- `MetadataJson`
- `CreatedAt`
- `UpdatedAt`

## 2.4 `TenantSubscriptionPackage`
Suggested fields:
- `Id`
- `TenantId`
- `CommercialPackageId`
- `Source`
- `Status`
- `ExternalSubscriptionReference`
- `EffectiveFrom`
- `EffectiveTo`
- `MetadataJson`
- `CreatedAt`
- `UpdatedAt`

## 2.5 `TenantFeatureOverride`
Suggested fields:
- `Id`
- `TenantId`
- `FeatureKey`
- `Granted`
- `LimitValue`
- `Reason`
- `Source`
- `EffectiveFrom`
- `EffectiveTo`
- `CreatedBy`
- `MetadataJson`
- `CreatedAt`
- `UpdatedAt`

## 2.6 optional `EffectiveFeatureEntitlementReadModel`
Suggested fields:
- `TenantId`
- `FeatureKey`
- `Granted`
- `LimitValue`
- `ResolvedFromJson`
- `EffectiveFrom`
- `EffectiveTo`
- `UpdatedAt`

---

# 3. Suggested database tables

## `feature_catalog`
- `feature_key` PK
- `service`
- `category`
- `display_name`
- `description`
- `is_quota`
- `unit`
- `metadata_json`
- `created_at`
- `updated_at`

## `commercial_packages`
- `id`
- `code`
- `name`
- `category`
- `billing_model`
- `description`
- `is_active`
- `metadata_json`
- `created_at`
- `updated_at`

## `commercial_package_features`
- `id`
- `commercial_package_id`
- `feature_key`
- `granted`
- `limit_value`
- `limit_merge_policy`
- `metadata_json`
- `created_at`
- `updated_at`

## `tenant_subscription_packages`
- `id`
- `tenant_id`
- `commercial_package_id`
- `source`
- `status`
- `external_subscription_reference`
- `effective_from`
- `effective_to`
- `metadata_json`
- `created_at`
- `updated_at`

## `tenant_feature_overrides`
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
- `metadata_json`
- `created_at`
- `updated_at`

## optional `effective_feature_entitlements`
- `tenant_id`
- `feature_key`
- `granted`
- `limit_value`
- `resolved_from_json`
- `effective_from`
- `effective_to`
- `updated_at`

---

# 4. Resolution rules

For a given tenant:

## Step 1
Load active `tenant_subscription_packages`.

## Step 2
Load all `commercial_package_features` for those packages.

## Step 3
Group by `FeatureKey`.

## Step 4
Resolve `Granted` using package merge rules.
Recommended first pass:
- `Granted = true` if any active package grants the feature

## Step 5
Resolve `LimitValue` using feature/package merge policy.
Recommended supported merge modes:
- `Max`
- `Sum`
- `LatestWins`
- `OverrideOnly`

## Step 6
Apply `tenant_feature_overrides` last.
Overrides win.

## Step 7
Return effective entitlement set or store into read model.

---

# 5. Suggested APIs

## Feature catalog
- `GET /billing/features`
- `POST /billing/features`
- `PUT /billing/features/{featureKey}`

## Commercial packages
- `GET /billing/packages`
- `GET /billing/packages/{id}`
- `POST /billing/packages`
- `PUT /billing/packages/{id}`
- `PUT /billing/packages/{id}/features`

## Tenant package assignments
- `GET /billing/tenants/{tenantId}/packages`
- `PUT /billing/tenants/{tenantId}/packages`
- `POST /billing/tenants/{tenantId}/packages`
- `DELETE /billing/tenants/{tenantId}/packages/{assignmentId}`

## Tenant overrides
- `GET /billing/tenants/{tenantId}/feature-overrides`
- `POST /billing/tenants/{tenantId}/feature-overrides`
- `PUT /billing/tenants/{tenantId}/feature-overrides/{overrideId}`
- `DELETE /billing/tenants/{tenantId}/feature-overrides/{overrideId}`

## Effective entitlements
- `GET /billing/entitlements/me`
- `GET /billing/tenants/{tenantId}/entitlements`
- `GET /billing/tenants/{tenantId}/entitlements/{featureKey}`

---

# 6. Migration from current state

## Phase B1 - schema introduction
- add new tables
- seed `feature_catalog`
- seed `legacy.free`, `legacy.pro`, `legacy.enterprise`
- map current plan feature matrix into `commercial_package_features`

## Phase B2 - compatibility layer
- keep current `PlanType`
- derive active `TenantSubscriptionPackage` from existing subscription rows
- maintain backward-compatible entitlement APIs

## Phase B3 - resolver cutover
- replace hardcoded `GetDefinitions(PlanType)` with DB-backed resolution
- preserve existing behavior during migration

## Phase B4 - add-ons and overrides
- allow multiple active packages
- support overrides and expiry windows

## Phase B5 - admin tooling
- add internal/admin mutation APIs
- add audit trail and tests

---

# 7. Required tests

## Unit tests
- package-only resolution
- multi-package merge behavior
- override precedence
- expiry behavior
- limit merge rules

## Integration tests
- `GET /billing/entitlements/me`
- package assignment APIs
- override APIs
- backward compatibility with legacy plan-derived tenants

---

# 8. Definition of done

Billing-service part is done when:
- feature/package/override data is DB-backed
- effective entitlement resolution no longer depends on hardcoded plan switch logic
- legacy plan behavior can still be represented through seeded packages
- downstream services can query effective entitlements safely
- tests cover merge/override/expiry cases
