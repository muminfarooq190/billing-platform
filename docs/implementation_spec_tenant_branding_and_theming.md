# Implementation Spec - Tenant Branding, Themes, Logos, Images, and Template Customization

_Last updated: 2026-04-09_

This document defines how Voyara should support tenant-specific branding across portal UI, templates, emails, customer-facing quotation views, and related assets.

Blunt goal:
let each tenant customize their own visual identity without breaking multi-tenant isolation or fighting the current microservice architecture.

---

# 1. Problem statement

Different tenants will want their own:
- logo
- brand colors
- favicon
- portal theme
- quote/public-share styling
- email header/footer branding
- document/template imagery

Right now the repo architecture appears capable of tenant scoping, but branding is not yet modeled as a first-class cross-service capability.

If we do this casually, we’ll get:
- duplicated logo storage across services
- frontend hardcoding
- templates with inconsistent branding
- broken cache invalidation
- cross-service leaks of tenant assets

We need a proper architecture-aligned design.

---

# 2. Scope

This spec covers:
- tenant themes
- logos and custom images
- template-level branding settings
- customer portal and public quotation branding
- storage and delivery strategy
- alignment with current services and gateway

This spec does not cover:
- full CMS/page-builder
- arbitrary white-label code forks per tenant
- customer-specific theme overrides inside one tenant

---

# 3. Architecture alignment

Current repo architecture uses:
- per-service databases
- API gateway
- CQRS + MediatR
- service-owned schemas
- tenant context from gateway/JWT
- file metadata pattern already present in travel-service for attachments/documents

Therefore branding should follow the same principles:
- metadata in DB
- binaries in file/object storage
- no cross-service DB sharing
- one service owns branding truth
- other services consume branding via API or cached projection

---

# 4. Recommended ownership model

## Preferred: identity-service or a dedicated tenant-config domain

Since branding is tenant-level configuration, not travel-specific or billing-specific, it should not live inside travel-service.

Recommended options:

### Option A - extend identity-service
Best if identity-service already owns tenant/admin profile concerns.

### Option B - create a dedicated tenant-config/branding-service later
Best if you expect branding, settings, preferences, feature flags, and localization to grow heavily.

## Recommendation for now
Use identity-service as first owner.
That is the smallest architecture-respecting change.

---

# 5. New domain concepts

## 5.1 `TenantBranding`

Purpose:
canonical tenant brand/theme settings.

### Suggested fields
- `TenantId`
- `DisplayName`
- `LegalName` (nullable)
- `PrimaryColor`
- `SecondaryColor`
- `AccentColor`
- `TextColor`
- `BackgroundColor`
- `ThemeMode` (`Light`, `Dark`, `System`, `Custom`)
- `DefaultFontFamily`
- `SupportEmail`
- `SupportPhone`
- `WebsiteUrl`
- `Tagline`
- `CreatedAt`
- `UpdatedAt`

## 5.2 `TenantBrandAsset`

Purpose:
store logos/images/icons/banners as metadata.

### Suggested fields
- `Id`
- `TenantId`
- `AssetType` (`LogoPrimary`, `LogoLight`, `LogoDark`, `Favicon`, `PortalBanner`, `EmailHeader`, `EmailFooter`, `QuotationCover`, `TemplateImage`, `Other`)
- `StorageKey`
- `OriginalFileName`
- `ContentType`
- `SizeBytes`
- `Width`
- `Height`
- `AltText`
- `IsActive`
- `CreatedAt`
- `UpdatedAt`
- `DeletedAt`

## 5.3 `TenantTemplateTheme`

Purpose:
branding overrides for specific template surfaces.

### Suggested fields
- `Id`
- `TenantId`
- `TemplateScope` (`Portal`, `QuotationPublicView`, `QuotationPdf`, `Email`, `Invoice`, `Notification`, `LoginPage`)
- `HeaderHtml` (nullable)
- `FooterHtml` (nullable)
- `CustomCss` (nullable)
- `LogoAssetId` (nullable)
- `BackgroundAssetId` (nullable)
- `SettingsJson`
- `CreatedAt`
- `UpdatedAt`

---

# 6. Storage strategy

Follow the same pattern already used for travel attachments/documents.

## Rules
- binaries are not stored in Postgres
- metadata is stored in DB
- use storage abstraction (`IFileStorage`-style)
- local filesystem in dev, object storage later in prod

## Suggested storage paths
- `tenant/{tenantId}/branding/logo-primary/...`
- `tenant/{tenantId}/branding/favicon/...`
- `tenant/{tenantId}/branding/email/...`
- `tenant/{tenantId}/branding/portal/...`
- `tenant/{tenantId}/branding/templates/...`

---

# 7. API design

## 7.1 Branding settings

### GET `/identity/tenant-branding`
Returns current tenant branding profile.

### PUT `/identity/tenant-branding`
Updates colors/text/theme settings.

Request example:
```json
{
  "displayName": "Voyara Holidays",
  "primaryColor": "#0F172A",
  "secondaryColor": "#1D4ED8",
  "accentColor": "#F59E0B",
  "themeMode": "Light",
  "defaultFontFamily": "Inter",
  "supportEmail": "support@voyara.example"
}
```

## 7.2 Asset APIs

### POST `/identity/tenant-branding/assets`
Upload logo/image asset.

### GET `/identity/tenant-branding/assets`
List brand assets.

### DELETE `/identity/tenant-branding/assets/{assetId}`
Soft delete branding asset.

## 7.3 Template theme APIs

### GET `/identity/tenant-branding/templates/{scope}`
Read template theme settings.

### PUT `/identity/tenant-branding/templates/{scope}`
Upsert per-surface theme config.

---

# 8. Consumption model across services

Branding is tenant-level shared config, so multiple services need it.

## travel-service needs branding for
- public quotation view
- quotation PDF generation
- customer-visible notes/portal surfaces later

## communication-service needs branding for
- email templates
- notification templates
- branded headers/footers and logos

## billing-service may need branding for
- invoices
- receipts
- billing portal views

## frontend needs branding for
- tenant portal shell
- login/landing/dashboard appearance

---

# 9. Delivery strategy for consuming branding

## Phase B1 - synchronous lookup
Other services fetch branding via internal API when needed.

Good for:
- first implementation
- lower complexity

Needs:
- caching
- timeout/fallback behavior

## Phase B2 - projection/event-driven cache
Identity publishes branding-changed event.
Consumers update local cache/projection.

Recommended event names:
- `identity.tenant-branding.updated`
- `identity.tenant-brand-asset.updated`
- `identity.tenant-brand-asset.deleted`

This aligns with outbox/event architecture already present in billing-service.

---

# 10. Frontend alignment

## Portal theme model
Frontend should not hardcode tenant themes in build artifacts.

Instead:
- fetch branding at bootstrap after tenant resolution
- apply CSS variables dynamically
- resolve active logo URLs from branding API

### Suggested CSS variables
- `--tenant-color-primary`
- `--tenant-color-secondary`
- `--tenant-color-accent`
- `--tenant-color-text`
- `--tenant-color-bg`
- `--tenant-font-family`

## React/frontend recommendation
Create a `TenantBrandingProvider` that:
- loads branding once
- exposes theme + asset URLs
- injects CSS custom properties into document root

---

# 11. Public quote/share and template theming

For public quotation pages in travel-service:
- fetch tenant branding by `TenantId`
- render tenant logo and colors
- use quotation/public-view specific template settings if present
- never expose internal branding asset metadata that is irrelevant to public view

## Template priority order
1. surface-specific override (`QuotationPublicView`)
2. tenant global branding defaults
3. system default theme

---

# 12. Email and notification branding

communication-service should resolve:
- active email header logo
- footer/support text
- tenant colors for email buttons/highlights
- optional per-notification template styling

Recommended first pass:
- global tenant branding + one email template surface
- avoid arbitrary free-form HTML everywhere initially

---

# 13. Subscription interaction

Branding itself may be partially gated by plan.

Example:
- `branding.theme.manage`
- `branding.assets.manage`
- `branding.templates.advanced`
- `branding.white_label.custom_domain`

This should integrate with the entitlement system from the subscription spec.

---

# 14. Security and validation rules

## Asset rules
- validate content type
- validate image dimensions/size
- virus/malware scan later if required
- soft delete metadata instead of blind hard delete where possible

## Data isolation
- every branding record must be tenant-scoped
- signed/private URLs for non-public assets when appropriate
- public assets should still resolve only from tenant-owned storage keys

## HTML/CSS customization rules
If allowing `HeaderHtml`, `FooterHtml`, or `CustomCss`:
- sanitize HTML
- restrict dangerous CSS/JS injection
- never allow arbitrary JS

---

# 15. Suggested implementation phases

## Phase B1 - branding core

### Add
- `TenantBranding`
- `TenantBrandAsset`
- branding APIs in identity-service
- file storage integration
- tests

### Outcome
A tenant can set colors and upload logos.

---

## Phase B2 - frontend portal theming

Status: in progress / partially implemented

### Add
- branding bootstrap endpoint usage in frontend
- CSS variable application
- logo rendering in shell/header/login surfaces

Implemented so far:
- admin and customer portal tracked layouts now use branding-aware shells
- portals pull existing tenant branding from identity-service B1 APIs
- shell/header surfaces now reflect tenant display/theme metadata instead of static app chrome

### Outcome
Portal visibly changes by tenant.

---

## Phase B3 - travel quotation/public-view branding

Status: implemented in tracked customer portal surface

### Add
- tenant branding lookup in travel-service or frontend public view
- quote/public page themed by tenant
- logo + colors on quotation exports/views

Implemented so far:
- customer portal public quote page now loads real public quotation data
- page also loads tenant branding and applies tenant-aware colors/support/footer details
- public quotation experience is no longer a dead scaffold page

### Outcome
Customer-facing quotation experience becomes white-labeled.

---

## Phase B4 - communication template branding

Status: initial implementation complete

### Add
- email header/footer branding
- notification template styling support
- branding resolution cache

Implemented so far:
- communication-service now enriches notification placeholders with branding placeholders
- default branding placeholders include `BrandDisplayName`, `BrandPrimaryColor`, and `BrandSupportEmail`
- template rendering can now consume tenant-brand-style placeholders without hardcoding them into each template

### Outcome
Emails and notifications feel tenant-owned.

---

## Phase B5 - advanced template theming

Status: initial docs/alignment pass complete, advanced surface still pending

### Add
- per-surface template overrides
- branded banners/backgrounds
- invoice/receipt styling
- optional custom domain prep hooks later

---

# 16. Suggested PR breakdown

## PR B1 - identity branding model + assets
- entities
- migrations
- repository/API

## PR B2 - frontend theme consumption
- provider
- CSS variables
- shell/logo support

## PR B3 - travel public quotation branding
- tenant branding integration
- template selection

## PR B4 - communication email branding
- email template hooks
- asset usage

## PR B5 - docs + entitlement hooks
- docs
- plan-gated branding management if needed

---

# 17. Minimum tests required

## Identity/branding
- tenant can update own branding only
- asset upload validates file type/size
- deleted asset no longer returned as active

## Frontend
- branding bootstrap applies CSS variables
- missing branding falls back to defaults

## Travel
- public quote view resolves correct tenant logo/colors
- tenant A branding never appears on tenant B quote

## Communication
- email rendering uses tenant-specific logo/colors

---

# 18. Definition of done

This work is done only when:
- tenant branding is owned in one service
- logos/images are stored as assets with metadata in DB
- frontend can apply tenant theme dynamically
- quotation/public-facing views can reflect tenant branding
- communication templates can use tenant branding
- tenant isolation is enforced for all branding reads/writes

---

# 19. Final blunt recommendation

Do not scatter `logoUrl`, `primaryColor`, and random template settings across travel-service, frontend configs, and communication-service separately.
That becomes a trash fire.

Create one tenant-branding source of truth, most likely in identity-service for now.
Then let other services consume it via API/cache/events.

That keeps the architecture clean and gives tenants the white-labeling they’ll expect.
