# Implementation Spec - Tenant Self-Onboarding, Package Selection, Branding Setup, Admin Creation, and Initial Portal Activation

_Last updated: 2026-04-12_

This document defines the target self-serve onboarding flow where a new tenant can register themselves, choose their package/features/theme, create their admin account, and start using their portal without manual back-office creation for every customer.

Blunt goal:
make Voyara capable of onboarding a new tenant like a proper SaaS product instead of requiring invisible operator magic for every account.

This spec assumes some foundational APIs may already exist for tenant creation, identity registration, branding, and entitlements.
The point here is to define the complete business flow, missing APIs/contracts, orchestration, and rollout shape.

---

# 1. Problem statement

Current repo direction supports pieces of:
- tenant-aware identity
- billing/subscription management
- branding/theming
- feature entitlements

But self-serve SaaS onboarding needs these pieces connected into one coherent journey.

The target journey is:
1. tenant starts registration
2. selects package and/or individual features
3. sees pricing summary
4. chooses theme/branding basics
5. creates primary admin account
6. tenant subscription and feature entitlements are provisioned
7. admin signs in
8. admin assigns features to internal agents/users
9. tenant portal becomes operational

If we do this badly, we get:
- half-created tenants
- orphan subscriptions
- admin account exists but tenant not provisioned
- branding stored before tenant is valid
- portal activated without package/feature coherence
- postman/test flows that do not represent reality

---

# 2. Scope

This spec covers:
- self-serve tenant registration
- package/feature selection
- pricing summary contract
- branding/theme selection during onboarding
- tenant admin account creation
- initial provisioning/orchestration
- initial portal readiness and post-signup first-run flow

This spec does not cover:
- payment gateway implementation details in depth
- invoicing engine details
- advanced partner/reseller onboarding
- fully custom enterprise contract flows with offline legal approval

---

# 3. Target onboarding outcomes

At the end of onboarding, the system should have:
- a valid tenant record
- a chosen base package and optional add-on features
- initial tenant branding/theme configuration
- a primary admin user account tied to the tenant
- tenant commercial entitlements provisioned
- optional default feature assignment policies initialized
- portal status marked active or pending-payment/pending-verification

---

# 4. High-level user journey

## Step O1 - start registration
Prospect enters basic information:
- tenant/company name
- email
- phone (optional)
- country/region
- desired portal/workspace slug if applicable

## Step O2 - choose commercial package
Prospect selects:
- base package
- optional add-ons
- individual priced features where applicable

System shows:
- monthly/annual price
- included features
- assignment-limited features if any
- total amount

## Step O3 - choose portal look and feel
Prospect may choose:
- starter theme
- brand colors
- logo upload (optional at this stage)
- display name/support contact defaults

## Step O4 - create admin account
Prospect creates primary tenant admin account:
- full name
- email
- password or auth bootstrap method
- optional MFA bootstrap later

## Step O5 - provision tenant
System orchestrates:
- tenant creation
- subscription/package linkage
- feature entitlements
- branding defaults
- admin membership/role grant
- default portal configuration

## Step O6 - initial portal activation
Tenant lands in portal and can:
- confirm setup
- invite users/agents
- assign features to internal users
- start using enabled modules

---

# 5. Business rules

## 5.1 Tenant should not become active by accident
A tenant should have explicit lifecycle states.

Suggested states:
- `Draft`
- `PendingVerification`
- `PendingPayment`
- `Provisioning`
- `Active`
- `Suspended`
- `Cancelled`

## 5.2 Onboarding must be idempotent
Retrying the flow should not create duplicate tenants/subscriptions/admins.
Use onboarding session/correlation IDs.

## 5.3 Pricing shown at signup must match provisioned features
Do not let UI quote one package while backend provisions something else.

## 5.4 Admin creation and tenant provisioning should be transactional-orchestrated
If perfect distributed transaction is not possible, use saga/outbox compensation.
Do not leave silent partial setup.

## 5.5 Feature assignment defaults must be explicit
Some purchased features may be tenant-wide by default.
Others may require explicit admin assignment after onboarding.

---

# 6. Recommended ownership by service

## identity-service owns
- tenant registration identity aspects
- admin user creation
- tenant membership
- roles/permissions
- branding/settings ownership if current architecture keeps it there

## billing-service owns
- package catalog
- feature pricing
- commercial entitlements
- selected package/add-on provisioning
- subscription state

## api-gateway / frontend orchestration layer
- self-serve onboarding UX aggregation
- route/public onboarding APIs
- request composition and normalized response shape if needed

## optional orchestrator/application layer
For clean multi-step provisioning, consider dedicated onboarding orchestration in gateway/backend-for-frontend or a dedicated onboarding workflow module.

---

# 7. Domain additions

## 7.1 `OnboardingSession`
Represents a draft onboarding attempt.

Suggested fields:
- `Id`
- `SessionToken`
- `Email`
- `CompanyName`
- `CountryCode`
- `DesiredSlug`
- `Status`
- `SelectedBasePackageCode`
- `SelectedAddonCodesJson`
- `SelectedFeatureKeysJson`
- `ThemePreset`
- `BrandingDraftJson`
- `AdminDraftJson`
- `PricingSnapshotJson`
- `CorrelationId`
- `ExpiresAt`
- `CreatedAt`
- `UpdatedAt`

Purpose:
- draft signup safely
- support resume/retry
- prevent duplicate accidental provisioning

## 7.2 `TenantProvisioningJob` or saga state
Suggested fields:
- `Id`
- `OnboardingSessionId`
- `TenantId`
- `Status`
- `CurrentStep`
- `ErrorCode`
- `ErrorMessage`
- `RetryCount`
- `CreatedAt`
- `UpdatedAt`

---

# 8. API design

These APIs can be implemented directly in gateway/BFF or split between identity/billing with an orchestrated public facade.

## 8.1 Public onboarding start and draft APIs

### POST `/public/onboarding/sessions`
Start self-serve onboarding session.

Request example:
```json
{
  "companyName": "Voyara Holidays",
  "email": "founder@example.com",
  "countryCode": "IN",
  "desiredSlug": "voyara-holidays"
}
```

Response example:
```json
{
  "sessionId": "onb_123",
  "status": "Draft",
  "nextStep": "PackageSelection"
}
```

### GET `/public/onboarding/sessions/{sessionId}`
Return current draft state.

### PUT `/public/onboarding/sessions/{sessionId}/company-profile`
Update basic company details.

---

## 8.2 Package/feature selection and pricing APIs

### GET `/public/onboarding/packages`
Return packages, features, prices, theme eligibility, and assignment-limited notes.

### PUT `/public/onboarding/sessions/{sessionId}/commercial-selection`
Save selected package/add-ons/individual features.

Request example:
```json
{
  "basePackageCode": "base.operations",
  "addonCodes": ["addon.branding-pro"],
  "featureKeys": ["communication.notification.send"]
}
```

### POST `/public/onboarding/sessions/{sessionId}/pricing-preview`
Return pricing snapshot for current selection.

Response example:
```json
{
  "currency": "USD",
  "lineItems": [
    { "code": "base.operations", "amount": 49.00 },
    { "code": "addon.branding-pro", "amount": 15.00 },
    { "code": "communication.notification.send", "amount": 10.00 }
  ],
  "total": 74.00,
  "billingCycle": "Monthly"
}
```

---

## 8.3 Theme/branding selection APIs

### GET `/public/onboarding/themes`
Return available theme presets.

### PUT `/public/onboarding/sessions/{sessionId}/branding`
Save onboarding-time theme/branding choices.

Request example:
```json
{
  "themePreset": "ModernBlue",
  "displayName": "Voyara Holidays",
  "primaryColor": "#1D4ED8",
  "accentColor": "#F59E0B",
  "supportEmail": "support@example.com"
}
```

Optional logo upload can be:
- deferred until admin portal access
- or supported via staged upload API tied to onboarding session

Recommendation:
keep onboarding branding basic first, then allow richer branding post-login.

---

## 8.4 Admin account creation APIs

### PUT `/public/onboarding/sessions/{sessionId}/admin-account`
Save admin draft account.

Request example:
```json
{
  "fullName": "Mumin Farooq",
  "email": "admin@example.com",
  "password": "<redacted>"
}
```

### POST `/public/onboarding/sessions/{sessionId}/submit`
Finalize onboarding and trigger provisioning.

Response example:
```json
{
  "status": "Provisioning",
  "provisioningId": "prov_123",
  "nextStep": "AwaitProvisioning"
}
```

### GET `/public/onboarding/provisioning/{provisioningId}`
Return provisioning status.

---

## 8.5 Portal bootstrap APIs after onboarding

### GET `/portal/bootstrap`
Return:
- tenant summary
- branding
- current user info
- purchased features
- assigned user features
- portal modules enabled

This is important so the fresh admin lands in a coherent portal instead of a pile of disconnected calls.

---

# 9. Provisioning workflow

Suggested saga/orchestrated sequence:

## Step P1 - validate onboarding session
- session exists and not expired
- selection valid
- admin email valid
- pricing snapshot still current

## Step P2 - create tenant shell
- create tenant record with status `Provisioning` or `PendingPayment`

## Step P3 - create commercial records
- create subscription/customer package linkage
- assign selected packages/add-ons/features
- initialize tenant feature allocations

## Step P4 - create branding defaults
- persist selected theme/colors/support data

## Step P5 - create admin identity
- create user
- create tenant membership
- grant tenant admin role

## Step P6 - initialize portal defaults
- seed settings
- seed initial dashboard/module visibility defaults

## Step P7 - finalize tenant state
- set status to `Active` or appropriate gated state
- issue auth/login continuation

If any step fails:
- mark provisioning failed clearly
- store reason
- support safe retry or compensation

---

# 10. Portal activation and first-run expectations

After first login, tenant admin should be able to:
- view chosen package and enabled features
- upload logo / finalize branding
- invite agents/users
- assign purchased features to users/agents
- configure portal basics
- begin using enabled modules immediately

The first-run checklist should be explicit, not implied.

Suggested first-run tasks:
- confirm branding
- invite team
- assign features
- configure communication templates
- create first quotation/booking workflow entity

---

# 11. Feature assignment integration

This onboarding spec depends on the separate assignment spec.

Important rule:
if a selected feature requires explicit assignment, onboarding should provision the feature at tenant level but not silently grant it to all users.

Recommended default:
- primary admin gets all purchased assignable features initially
- other users get nothing until invited and assigned

That is the least surprising behavior.

---

# 12. Branding/theme integration

This spec aligns with tenant branding spec.

Onboarding should support:
- theme preset selection
- basic brand colors
- display name
- support contact info

Richer actions can happen after onboarding:
- logo upload variants
- advanced templates
- email branding
- portal banners
- white-label assets

Do not overload first signup with every advanced branding knob.

---

# 13. Security and validation rules

## Public onboarding endpoints must:
- rate limit abuse
- validate slug/company/email uniqueness rules
- prevent account enumeration where possible
- use expiring onboarding session tokens
- sanitize branding inputs
- never trust pricing from client

## Password/account creation must:
- use proper password policy
- support later MFA enrollment
- avoid creating orphan user if tenant provisioning fails silently

## Tenant slug/domain rules must be clear
If portal routing depends on slug/subdomain, validate and reserve it safely.

---

# 14. Failure and recovery cases

## Case A - pricing changed before submit
Response should require re-confirmation.
Do not silently provision old selection at new price.

## Case B - tenant created but admin user creation failed
Provisioning status should show failed step and support retry.

## Case C - payment required but incomplete
Tenant may remain `PendingPayment` with limited access.

## Case D - branding saved but session expired
Draft can be discarded safely.

## Case E - duplicate email/company attempt
System should respond predictably instead of creating near-duplicate tenant records.

---

# 15. Postman coverage needed

Need end-to-end collection flows for:
- start onboarding session
- select package/add-ons/features
- preview pricing
- set branding/theme
- create admin draft
- submit provisioning
- poll provisioning result
- login/bootstrap portal
- assign purchased features to tenant users

Also need negative flows:
- invalid package code
- expired onboarding session
- duplicate slug
- provisioning conflict
- selected feature not compatible with base package if such rule exists

These are detailed in the separate Postman spec.

---

# 16. Suggested implementation phases

## Phase ONS1 - onboarding draft/session APIs
- onboarding session entity
- public draft endpoints
- pricing preview integration

## Phase ONS2 - submit + provisioning saga
- orchestration workflow
- tenant + subscription + admin bootstrap
- failure visibility/retry handling

## Phase ONS3 - portal bootstrap + first-run experience
- bootstrap endpoint
- initial checklist and state
- tenant summary + feature access view

## Phase ONS4 - advanced onboarding enhancements
- staged logo upload
- payment processor hookup
- email verification
- saved-progress resume

---

# 17. Tests required

## Public onboarding
- start session succeeds
- invalid session rejected
- package selection persisted
- pricing preview consistent with selected features

## Provisioning
- submit creates tenant/subscription/admin coherently
- failed step surfaced clearly
- retries idempotent

## Portal activation
- fresh admin bootstrap reflects branding + features + assignments
- admin can immediately access allowed setup actions

## Security
- rate limits apply
- invalid inputs rejected
- tenant isolation maintained

---

# 18. Definition of done

This work is done only when:
- a new tenant can self-register through public APIs
- package/features/theme/admin setup can be captured coherently
- provisioning creates tenant, commercial entitlements, branding, and admin identity safely
- portal bootstrap reflects the real provisioned state
- primary admin can continue setup by assigning features to users/agents
- postman collections/scripts cover the real onboarding flow end-to-end

---

# 19. Final blunt recommendation

Do not treat self-onboarding as a couple of random create-tenant endpoints and call it a day.
That is how you get zombie tenants, broken subscriptions, and support pain.

Build one sane flow:
- draft onboarding
- commercial selection
- branding choice
- admin creation
- orchestrated provisioning
- portal bootstrap

Then the product actually behaves like a real SaaS platform.