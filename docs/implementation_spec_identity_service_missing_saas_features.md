# Implementation Spec - Identity Service Missing SaaS Features

_Last updated: 2026-04-09_

This document critically assesses what `identity-service` is still missing if Voyara wants to behave like a serious SaaS platform and not just a minimal auth/user table with tenant labels.

Blunt summary:
identity-service now covers the basics reasonably:
- tenant registration
- login / JWT / refresh token flow
- user CRUD
- tenant suspension
- tenant branding / brand assets / template theme overrides

That is a decent foundation.
But for a real multi-tenant SaaS, identity-service is still missing several important platform capabilities.

---

# 1. Current reality check

## What identity-service already has
- tenant registration
- login
- JWT/JWKS support
- password change
- user create/update/delete
- tenant read by id/email
- tenant suspension
- tenant branding + assets + template themes

## What identity-service still lacks
The biggest SaaS-grade gaps are now around:
- organization/role maturity
- invitations and onboarding
- access policy depth
- tenant settings / preferences foundation
- security controls and auditability
- lifecycle and governance features
- admin/owner self-service controls
- support/impersonation controls

---

# 2. Most critical SaaS gaps

## Tier A - core SaaS maturity
1. invitation-based user onboarding
2. proper role/permission model beyond simple role strings
3. tenant settings / workspace configuration
4. identity audit trail and security events
5. user lifecycle states (active, invited, locked, disabled)
6. session/token management visibility and revocation

## Tier B - enterprise / platform trust features
7. MFA / stronger auth controls
8. support impersonation / break-glass admin model
9. domain ownership / email domain policies
10. SSO / external identity readiness

## Tier C - admin ergonomics / scale
11. organization/team structures
12. delegated admin controls
13. tenant-level policy engine
14. admin exports / compliance support

---

# 3. Proposed next phases

---

# Phase I1 - Invitation and User Lifecycle Management

## Goal
Stop forcing all user creation through raw admin-created passwords and direct CRUD.

## Why this matters
A real SaaS platform needs a clean lifecycle for users:
- invited
- accepted
- active
- suspended
- locked
- deleted

Right now the service can create users, but there is no proper invite/accept path.
That is clunky and weak for both security and UX.

## Key features

### 1.1 `UserInvitation`
Suggested fields:
- `Id`
- `TenantId`
- `Email`
- `Role`
- `InvitedByUserId`
- `TokenHash`
- `ExpiresAt`
- `AcceptedAt`
- `RevokedAt`
- `CreatedAt`

### 1.2 User status model
Extend user with:
- `Status` (`Invited`, `Active`, `Suspended`, `Locked`, `Deleted`)
- `LastLoginAt`
- `PasswordChangedAt`
- `MustChangePassword`

### 1.3 Invitation flow
- admin invites user by email
- invite token generated
- recipient accepts invitation and sets password
- user becomes active

## APIs
- `POST /identity/users/invitations`
- `POST /identity/users/invitations/{id}/resend`
- `POST /identity/users/invitations/accept`
- `POST /identity/users/{id}/suspend`
- `POST /identity/users/{id}/reactivate`

## Exit criteria
- new users can be onboarded through invites
- user lifecycle states are explicit and queryable
- admins do not need to manually assign passwords for every new user

---

# Phase I2 - Role and Permission System

## Goal
Replace shallow role-string logic with real SaaS-grade authorization primitives.

## Why this matters
Simple `Admin`, `Owner`, `Agent` role strings are okay for a prototype.
They are not enough for a growing SaaS product where different tenants want:
- finance-only users
- ops-only users
- read-only users
- custom internal admin boundaries

## Key features

### 2.1 `Permission`
Static registry of capabilities such as:
- `travel.quotation.read`
- `travel.quotation.write`
- `billing.invoices.read`
- `identity.users.manage`
- `branding.theme.manage`

### 2.2 `RoleDefinition`
Tenant-aware or system-default roles with permission bundles.

### 2.3 `UserRoleAssignment`
Map users to roles with auditability.

### 2.4 optional direct `UserPermissionOverride`
For edge cases.

## APIs
- `GET /identity/roles`
- `POST /identity/roles`
- `PUT /identity/roles/{id}`
- `GET /identity/permissions`
- `PUT /identity/users/{id}/roles`

## Exit criteria
- permission checks can become feature-aware and action-aware
- roles are not just hardcoded enums in random handlers

---

# Phase I3 - Tenant Settings and Workspace Preferences

## Goal
Turn identity-service into the home of tenant-level workspace configuration, not just branding.

## Why this matters
A SaaS tenant needs more than colors and logos.
They need controlled workspace settings such as:
- timezone
- locale
- date/number formatting
- default currency
- notification defaults
- booking/quote numbering preferences
- feature toggles that are tenant-local (not commercial entitlements)

## Key features

### 3.1 `TenantSettings`
Suggested fields:
- `TenantId`
- `Timezone`
- `Locale`
- `DateFormat`
- `Currency`
- `NumberFormat`
- `DefaultCountry`
- `SettingsJson`
- `CreatedAt`
- `UpdatedAt`

### 3.2 settings API
- `GET /identity/tenant-settings`
- `PUT /identity/tenant-settings`

## Exit criteria
- tenant config stops leaking into random services
- UI and downstream services can resolve consistent tenant defaults

---

# Phase I4 - Security Events and Identity Audit

## Goal
Make identity behavior explainable and supportable.

## Why this matters
Identity without audit is a support and compliance nightmare.
You need to know:
- who invited whom
- who changed roles
- who reset password
- who suspended a user
- failed login spikes
- refresh token abuse / session anomalies

## Key features

### 4.1 `IdentityAuditLog`
Fields:
- `Id`
- `TenantId`
- `ActorUserId`
- `TargetUserId`
- `EventType`
- `BeforeJson`
- `AfterJson`
- `IpAddress`
- `UserAgent`
- `OccurredAt`

### 4.2 `SecurityEvent`
Fields:
- `Id`
- `TenantId`
- `UserId`
- `EventType` (`LoginSucceeded`, `LoginFailed`, `PasswordChanged`, `TokenRevoked`, `InvitationAccepted`, `RoleChanged`, etc.)
- `IpAddress`
- `UserAgent`
- `MetadataJson`
- `OccurredAt`

## APIs
- `GET /identity/audit/users/{id}`
- `GET /identity/security-events`

## Exit criteria
- support/admin can explain identity changes
- suspicious auth behavior is queryable

---

# Phase I5 - Session and Token Management

## Goal
Give users/admins control over sessions and refresh tokens.

## Why this matters
A modern SaaS platform should support:
- viewing active sessions
- revoking sessions
- forcing logout after password reset or admin action
- detecting stale tokens

## Key features

### 5.1 `UserSession`
Fields:
- `Id`
- `TenantId`
- `UserId`
- `RefreshTokenId`
- `DeviceName`
- `IpAddress`
- `UserAgent`
- `LastSeenAt`
- `CreatedAt`
- `RevokedAt`

### 5.2 APIs
- `GET /identity/me/sessions`
- `DELETE /identity/me/sessions/{sessionId}`
- `DELETE /identity/users/{id}/sessions` (admin)

## Exit criteria
- session revocation is explicit and visible
- refresh token behavior is governable

---

# Phase I6 - MFA and Security Hardening

## Goal
Add real account protection options.

## Why this matters
If Voyara handles business data, customer data, billing, and branded customer access, MFA becomes important fast.

## Key features
- TOTP-based MFA enrollment
- recovery codes
- MFA challenge on login
- optional tenant policy: require MFA for admins/owners

## APIs
- `POST /identity/me/mfa/enroll`
- `POST /identity/me/mfa/verify`
- `POST /identity/me/mfa/disable`

## Exit criteria
- admins can secure accounts beyond passwords
- tenant-level MFA policy can be enforced

---

# Phase I7 - Domain Verification and SSO Readiness

## Goal
Prepare identity-service for enterprise SaaS growth.

## Why this matters
Sooner or later enterprise customers ask for:
- domain-based user controls
- SSO / SAML / OIDC enterprise login
- automatic user provisioning / just-in-time access

## Key features

### 7.1 `TenantDomain`
Fields:
- `Id`
- `TenantId`
- `Domain`
- `VerificationToken`
- `VerifiedAt`
- `IsPrimary`

### 7.2 `TenantIdentityProvider`
Fields:
- `Id`
- `TenantId`
- `ProviderType`
- `ConfigJson`
- `Enabled`
- `CreatedAt`
- `UpdatedAt`

## Exit criteria
- domain ownership can be proven
- SSO path is architecturally possible without another identity rewrite

---

# Phase I8 - Support Impersonation and Admin Governance

## Goal
Help support teams troubleshoot safely without becoming a security disaster.

## Why this matters
B2B SaaS eventually needs support tooling like:
- impersonate tenant admin
- access a tenant safely for debugging
- emergency break-glass admin path

But this must be tightly audited.

## Key features
- support impersonation requests with reason
- time-limited impersonation tokens
- explicit audit trail for impersonation start/stop
- policy restrictions by environment/role

## APIs
- `POST /identity/support/impersonation/start`
- `POST /identity/support/impersonation/stop`
- `GET /identity/support/impersonation/history`

## Exit criteria
- support access is possible without silent abuse
- every impersonation action is audited

---

# 4. Cross-cutting architecture guidance

## Identity-service should remain the source of truth for
- users
- tenant membership
- auth/session state
- roles/permissions
- branding
- tenant settings
- security events

## Do not push tenant settings/role logic randomly into other services
Other services should consume identity outputs via:
- JWT claims
- internal APIs
- cached projections
- events where needed

## Keep the separation between
- commercial entitlements (billing-service)
- workspace config and access policy (identity-service)

That distinction matters.

---

# 5. Recommended execution order

If you want the highest SaaS maturity fastest, do this:

1. Phase I1 - invitation + user lifecycle
2. Phase I2 - role/permission system
3. Phase I4 - audit/security events
4. Phase I5 - session/token management
5. Phase I3 - tenant settings
6. Phase I6 - MFA
7. Phase I7 - domain verification / SSO readiness
8. Phase I8 - support impersonation / governance

Why this order:
- first fix onboarding and access control
- then make it observable and safe
- then improve settings and account security
- then add enterprise-grade identity controls

---

# 6. Definition of done for “real SaaS identity”

Identity-service starts to feel like a real SaaS platform layer when:
- users are invited/onboarded cleanly
- roles and permissions are explicit and flexible
- tenant settings live in one place
- auth/security events are auditable
- sessions can be inspected/revoked
- MFA exists
- enterprise identity path is possible
- support access is governed and logged

---

# 7. Final blunt conclusion

The current identity-service is good enough for:
- basic tenant registration
- basic login
- basic user management
- branding ownership

It is not yet good enough for a serious SaaS platform if you care about:
- access governance
- secure onboarding
- auditable admin behavior
- strong auth controls
- enterprise readiness

That is the real next frontier for identity-service.
