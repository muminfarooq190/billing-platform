# Identity Permission Catalog

_Last updated: 2026-04-10_

This is the small boring map of **permission constants → intended usage** for `identity-service`.

The goal is simple:
- keep permission meaning stable
- stop random drift where the same permission gets reused for unrelated admin actions
- make future controller/service changes less guessy

Source of truth for constant names:
- `services/identity-service/src/Infrastructure/Auth/Permissions.cs`

---

## Identity permissions

### `Permissions.Identity.UsersManage`
Constant:
- `identity.users.manage`

Intended use:
- create/update/delete users
- invite/resend/accept-managed onboarding flows
- suspend/reactivate users
- revoke sessions for another user
- assign operational user lifecycle actions

Current controller usage:
- `UsersController`
- `UserInvitationsController`
- admin endpoint in `SessionsController`

Should generally NOT be used for:
- role catalog management
- audit/security event browsing
- tenant plan/suspension controls
- branding/theme changes

---

### `Permissions.Identity.RolesManage`
Constant:
- `identity.roles.manage`

Intended use:
- create/update role definitions
- assign roles to users
- future permission-bundle administration

Current controller usage:
- `RolesController`

Should generally NOT be used for:
- basic user CRUD/lifecycle
- tenant settings
- branding
- tenant suspension/plan changes

---

### `Permissions.Identity.AuditRead`
Constant:
- `identity.audit.read`

Intended use:
- view identity audit logs
- view security events
- support/admin investigation flows

Current controller usage:
- `IdentityAuditController`

Should generally NOT be used for:
- mutating users/roles/settings

---

### `Permissions.Identity.SettingsManage`
Constant:
- `identity.settings.manage`

Intended use:
- manage tenant workspace defaults/settings
- timezone/locale/currency/format preferences
- future tenant-local workspace policy knobs that are not commercial entitlements

Current controller usage:
- `TenantSettingsController`

Should generally NOT be used for:
- branding/theme assets
- billing entitlements
- role assignment

---

### `Permissions.Identity.TenantManage`
Constant:
- `identity.tenant.manage`

Intended use:
- tenant plan changes
- tenant suspension/reactivation/governance actions
- top-level tenant admin controls

Current controller usage:
- `TenantsController`

Should generally NOT be used for:
- ordinary user management
- audit browsing
- branding/theme editing

---

## Branding permissions

### `Permissions.Branding.ThemeManage`
Constant:
- `branding.theme.manage`

Intended use:
- tenant branding profile changes
- brand asset upload/delete/read management
- template theme customization

Current controller usage:
- `TenantBrandingController`
- `TenantBrandingAssetsController`
- `TenantTemplateThemesController`

Should generally NOT be used for:
- tenant settings like timezone/currency
- user lifecycle/admin roles

---

## Cross-service / future-facing permissions

These are defined in the identity catalog and seeded for role bundles, but not fully enforced by `identity-service` controllers yet.
They exist so JWT permission claims can become useful to downstream services.

### `Permissions.Travel.QuotationRead`
Constant:
- `travel.quotation.read`

Intended use:
- downstream travel-service read access for quotation views/search/listing

### `Permissions.Travel.QuotationWrite`
Constant:
- `travel.quotation.write`

Intended use:
- downstream travel-service quotation creation/update/send/edit actions

### `Permissions.Billing.InvoicesRead`
Constant:
- `billing.invoices.read`

Intended use:
- downstream billing/invoice read access

---

## Practical rules for future changes

### 1. Prefer reusing an existing permission only when the action is genuinely the same kind of authority
Bad:
- reusing `identity.users.manage` for tenant suspension just because “it’s admin stuff”

Good:
- use `identity.tenant.manage` for tenant governance

### 2. If a new permission is needed
Do all of these together:
1. add constant to `Permissions.cs`
2. add policy registration in `PermissionAuthorization.cs` (or ensure `Permissions.All` covers it)
3. add seed definition in `IdentitySeed.cs`
4. update default system role bundles intentionally
5. update this doc

### 3. Keep identity permissions separate from commercial entitlements
Identity permissions answer:
- “what is this user allowed to do?”

Billing entitlements answer:
- “what has this tenant paid to unlock?”

Do not mash those together.

### 4. Prefer narrow permissions over vague god-permissions
Bad:
- `identity.admin.all`

Better:
- `identity.users.manage`
- `identity.roles.manage`
- `identity.audit.read`
- `identity.tenant.manage`

---

## Current default system role bundles

### Owner
Gets:
- `identity.users.manage`
- `identity.roles.manage`
- `identity.audit.read`
- `identity.settings.manage`
- `identity.tenant.manage`
- `branding.theme.manage`
- `travel.quotation.read`
- `travel.quotation.write`
- `billing.invoices.read`

### Admin
Gets:
- `identity.users.manage`
- `identity.audit.read`
- `identity.settings.manage`
- `identity.tenant.manage`
- `branding.theme.manage`
- `travel.quotation.read`
- `travel.quotation.write`
- `billing.invoices.read`

### Member
Gets:
- `travel.quotation.read`

---

## Blunt guidance

If you’re unsure whether to add a permission or reuse one, ask:

> Would granting this permission to a finance-only admin accidentally give them unrelated power?

If yes, the permission is probably too broad or being reused in the wrong place.
