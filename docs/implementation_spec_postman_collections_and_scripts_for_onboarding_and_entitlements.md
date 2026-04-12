# Implementation Spec - Postman Collections, Environments, and Scripts for Onboarding, Flexible Entitlements, and User Feature Assignment

_Last updated: 2026-04-12_

This document defines how Postman artifacts should be updated so the API surface for self-onboarding, flexible entitlements, tenant branding, and per-user feature assignment can actually be exercised end-to-end.

Blunt goal:
stop having APIs that technically exist but require human memory gymnastics in Postman because IDs/tokens/tenant values are not being captured and reused properly.

We need Postman collections that:
- model real workflows
- store important IDs automatically
- reduce manual copy-paste
- make regression testing survivable

---

# 1. Problem statement

As new APIs are introduced for:
- tenant self-onboarding
- package and feature selection
- branding setup
- admin creation
- flexible entitlements
- user feature assignments

Postman must also evolve.

Without explicit Postman design we get:
- broken demo flows
- missing environment variables
- repeated manual copying of tenant IDs and user IDs
- fake "API is ready" confidence even though no one can run the full workflow cleanly

Postman should act like an executable API handbook, not a folder full of random requests.

---

# 2. Goals

The updated Postman setup should support:
- public onboarding flow from draft to provisioning
- admin login and portal bootstrap flow
- billing package and entitlement admin flows
- branding/theme update flows
- tenant user feature assignment flows
- negative entitlement checks
- script-based environment capture after each important step

---

# 3. Recommended collection structure

Create or update collections into clear workflow folders.

## Collection A - Public Onboarding
Requests:
1. Start onboarding session
2. Get onboarding session
3. Update company profile
4. List packages
5. Save commercial selection
6. Preview pricing
7. List theme presets
8. Save branding draft
9. Save admin account draft
10. Submit onboarding
11. Get provisioning status

## Collection B - Auth / Identity Bootstrap
Requests:
1. Admin login
2. Get current user profile
3. Get portal bootstrap
4. Invite tenant user
5. List tenant users

## Collection C - Billing / Flexible Entitlements
Requests:
1. List feature catalog
2. List packages
3. Create package
4. Update package
5. Assign packages to tenant
6. Create tenant feature override
7. Get tenant effective entitlements
8. Get my effective feature access

## Collection D - Branding / Themes
Requests:
1. Get tenant branding
2. Update tenant branding
3. Upload branding asset
4. List branding assets
5. Upsert template theme

## Collection E - User Feature Assignment
Requests:
1. Get tenant feature allocations
2. Get all feature assignments
3. Assign features to user
4. Bulk assign features
5. Revoke user feature assignment
6. Get user effective features
7. Get my feature access

## Collection F - Protected Workflow Validation
Requests:
1. Call protected endpoint without tenant entitlement
2. Call protected endpoint with tenant entitlement but no user assignment
3. Call protected endpoint with user assignment
4. Validate commercial/assignment failure responses

---

# 4. Recommended environment variables

At minimum, maintain these environment variables.

## Base/platform variables
- `baseUrl`
- `publicBaseUrl` (optional if separate)
- `gatewayBaseUrl`
- `apiVersion` (optional)

## Auth variables
- `accessToken`
- `refreshToken` (if applicable)
- `tokenType`
- `currentUserId`
- `currentUserEmail`

## Onboarding variables
- `onboardingSessionId`
- `onboardingSessionStatus`
- `onboardingProvisioningId`
- `desiredSlug`
- `companyName`

## Tenant variables
- `tenantId`
- `tenantSlug`
- `tenantStatus`
- `tenantDisplayName`

## Billing/commercial variables
- `basePackageCode`
- `addonPackageCode`
- `featureKey`
- `packageId`
- `commercialPackageId`
- `featureOverrideId` (if returned)

## Branding variables
- `themePreset`
- `brandingAssetId`
- `templateScope`

## User assignment variables
- `adminUserId`
- `agentUserId`
- `secondaryUserId`
- `assignedFeatureKey`

---

# 5. Scripting principles

## 5.1 Store values automatically after successful requests
If a response contains a reusable identifier, save it.
Do not make humans hunt through JSON every time.

## 5.2 Keep scripts defensive
Do not assume every response body shape is identical.
Check existence before saving.

## 5.3 Prefer fewer, richer variables
Do not create 40 nearly identical variables unless necessary.

## 5.4 Clear stale variables when flow resets
If a new onboarding session starts, overwrite old session variables so people do not accidentally submit the wrong flow.

---

# 6. Example Postman test scripts

## 6.1 Start onboarding session
Use in the Tests tab.

```javascript
pm.test("status is success", function () {
  pm.expect(pm.response.code).to.be.oneOf([200, 201]);
});

const json = pm.response.json();

if (json.sessionId) {
  pm.environment.set("onboardingSessionId", json.sessionId);
}
if (json.status) {
  pm.environment.set("onboardingSessionStatus", json.status);
}
if (json.companyName) {
  pm.environment.set("companyName", json.companyName);
}
```

## 6.2 Save commercial selection / pricing preview
```javascript
const json = pm.response.json();

if (json.lineItems && json.lineItems.length > 0) {
  const baseLine = json.lineItems[0];
  if (baseLine.code) {
    pm.environment.set("basePackageCode", baseLine.code);
  }
}

if (json.total !== undefined) {
  pm.environment.set("pricingTotal", String(json.total));
}
```

## 6.3 Submit onboarding
```javascript
pm.test("submit accepted", function () {
  pm.expect(pm.response.code).to.be.oneOf([200, 202]);
});

const json = pm.response.json();
if (json.provisioningId) {
  pm.environment.set("onboardingProvisioningId", json.provisioningId);
}
if (json.status) {
  pm.environment.set("onboardingSessionStatus", json.status);
}
```

## 6.4 Provisioning status poll
```javascript
const json = pm.response.json();

if (json.tenantId) {
  pm.environment.set("tenantId", json.tenantId);
}
if (json.tenantStatus) {
  pm.environment.set("tenantStatus", json.tenantStatus);
}
if (json.tenantSlug) {
  pm.environment.set("tenantSlug", json.tenantSlug);
}
```

## 6.5 Login script
```javascript
const json = pm.response.json();

if (json.accessToken) {
  pm.environment.set("accessToken", json.accessToken);
}
if (json.refreshToken) {
  pm.environment.set("refreshToken", json.refreshToken);
}
if (json.user && json.user.id) {
  pm.environment.set("currentUserId", json.user.id);
  pm.environment.set("adminUserId", json.user.id);
}
if (json.user && json.user.email) {
  pm.environment.set("currentUserEmail", json.user.email);
}
```

## 6.6 Tenant user invitation / creation script
```javascript
const json = pm.response.json();

if (json.userId) {
  pm.environment.set("agentUserId", json.userId);
}
```

## 6.7 Assign feature to user script
```javascript
const json = pm.response.json();

if (json.featureKey) {
  pm.environment.set("assignedFeatureKey", json.featureKey);
}
```

---

# 7. Recommended pre-request scripts

## Authorization header helper
At collection level:
```javascript
const token = pm.environment.get("accessToken");
if (token) {
  pm.request.headers.upsert({ key: "Authorization", value: `Bearer ${token}` });
}
```

## Tenant header helper if platform uses explicit tenant headers
```javascript
const tenantId = pm.environment.get("tenantId");
if (tenantId) {
  pm.request.headers.upsert({ key: "X-Tenant-Id", value: tenantId });
}
```

Only use this if the actual platform contract requires it.
Do not fake headers the gateway/service does not really use.

---

# 8. Example request chaining workflow

## End-to-end happy path
1. Start onboarding session -> save `onboardingSessionId`
2. Save commercial selection -> save selected package values
3. Preview pricing -> save `pricingTotal`
4. Save branding draft
5. Save admin draft
6. Submit onboarding -> save `onboardingProvisioningId`
7. Get provisioning status -> save `tenantId`, `tenantSlug`, `tenantStatus`
8. Login as admin -> save `accessToken`, `adminUserId`
9. Get portal bootstrap -> verify tenant/feature state
10. Invite/create agent user -> save `agentUserId`
11. Assign feature to agent -> save `assignedFeatureKey`
12. Call protected API as assigned user -> expect success
13. Call protected API as unassigned user -> expect denial

That is the minimum realistic regression flow.

---

# 9. Negative test scenarios to encode in Postman

## Commercial denial
- tenant lacks purchased feature
- protected call returns appropriate entitlement failure

## Assignment denial
- tenant owns feature
- current user lacks assignment
- protected call returns appropriate assignment/access failure

## Validation failure
- invalid package code
- invalid feature key
- duplicate assignment
- assignment beyond seat cap
- onboarding session expired

## Auth failure
- missing token
- wrong tenant/user context

Collections should include these intentionally, not just happy path fluff.

---

# 10. Response assertions

For important requests, assert more than status code.

Examples:
- onboarding response contains `sessionId`
- provisioning response contains expected status shape
- entitlement response contains expected `featureKey` and `granted`
- assignment-denied protected response contains machine-readable failure code

Suggested checks:
```javascript
pm.test("response has tenantId", function () {
  const json = pm.response.json();
  pm.expect(json.tenantId).to.exist;
});
```

---

# 11. Collection variable naming conventions

Be boring and consistent.

Use:
- singular IDs: `tenantId`, `packageId`, `agentUserId`
- feature names: `featureKey`, `assignedFeatureKey`
- statuses: `tenantStatus`, `onboardingSessionStatus`

Avoid nonsense like:
- `id1`
- `currentthing`
- `foo`
- `latestuser`

That stuff becomes unmaintainable fast.

---

# 12. Documentation expectations in Postman

Every important request should have description text explaining:
- what it does
- what variables it expects
- what variables it saves
- what next request usually follows

Example:
- "Uses `onboardingSessionId` from prior request"
- "Stores `tenantId` and `tenantStatus` on success"

That turns Postman into runnable docs.

---

# 13. Suggested artifact outputs

Depending on current repo setup, update or create:
- main Postman collection JSON(s)
- environment JSON(s) for local/dev/test
- collection-level auth helper scripts
- folder/request-level tests scripts
- README or docs note explaining intended execution order

If repo stores Postman artifacts under version control, recommended folders could be:
- `postman/collections/`
- `postman/environments/`
- `docs/postman/` or README references

Use whatever existing repo convention already exists.
Do not invent a new folder structure if one already exists.

---

# 14. Rollout sequence for Postman updates

## Phase PM1 - baseline environment cleanup
- standardize environment variables
- add collection-level auth script

## Phase PM2 - onboarding flow folder
- add start/select/pricing/branding/admin/provisioning requests
- add save scripts

## Phase PM3 - entitlement/admin folders
- add feature catalog/package/assignment requests
- add negative validation coverage

## Phase PM4 - protected endpoint verification
- add real success/failure regression requests

## Phase PM5 - docs and examples
- add descriptions, execution notes, sample payload guidance

---

# 15. Definition of done

Postman work is done only when:
- end-to-end onboarding can be executed without manual JSON digging after every request
- important IDs/tokens/status values are automatically stored
- billing/branding/assignment workflows are represented clearly
- positive and negative entitlement scenarios are covered
- environment and collection naming are consistent and maintainable

---

# 16. Final blunt recommendation

If the Postman collections require someone to manually copy five IDs between requests every time, then the collections are not good enough.

Use scripts to capture:
- session IDs
- provisioning IDs
- tenant IDs
- tokens
- user IDs
- feature keys

That is the difference between demo theater and an actually usable API workflow.