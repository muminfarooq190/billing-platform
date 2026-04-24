# Frontend/Backend Integration Contract and Route Mapping

_Last updated: 2026-04-21_

This document translates the approved frontend alignment into a concrete integration contract for `voyara-portal`.

It answers two practical questions:
1. **Which frontend routes/pages map to which backend APIs?**
2. **What core payloads/contracts does the frontend need to bootstrap auth, tenancy, branding, entitlements, assignments, and domain workflows?**

Blunt rule:
if the frontend does not know which backend endpoints feed which page, the implementation will drift into fake UI again.

---

## 1. Scope

This contract covers the main MVP surfaces for:
- auth/session
- tenant identity/branding/settings
- entitlements and user feature assignments
- users/team management
- inquiries and draft concepts
- quotations and revisions
- bookings and confirmed itineraries
- contacts
- follow-ups
- communications/templates
- billing visibility
- route mapping from prototype-inspired page inventory to real frontend routes

This document is meant to sit beside:
- `docs/travel-crm-frontend-backend-alignment-spec.md`
- `docs/travel-crm-canonical-lifecycle-and-entity-ownership.md`
- `docs/feature-based-authorization-and-entitlement-audit.md`

---

## 2. Core integration principles

## 2.1 Frontend must compose a tenant-aware bootstrap payload
The frontend cannot render correctly from auth token alone.
It needs enough information to decide:
- who the user is
- which tenant they are in
- what permissions they have
- which features the tenant has purchased/unlocked
- which features the user is assigned
- what branding/theme to apply

## 2.2 Frontend should prefer explicit API composition over hidden guesswork
For MVP, the frontend can compose its startup state from a small set of backend endpoints instead of waiting for one perfect mega-endpoint.

## 2.3 Backend truth wins over prototype teaching
If the prototype suggests a route/page/workflow that conflicts with backend lifecycle truth, the route and page behavior must be corrected.

## 2.4 Legacy itinerary endpoints must not drive the primary frontend story
Legacy compatibility endpoints may continue to exist, but the real frontend should follow:
- accepted quotation -> booking creation
- booking -> confirmed itinerary creation/read

---

## 3. Required frontend bootstrap contract

The frontend should load a bootstrap model immediately after login (or on app refresh if a valid session already exists).

## 3.1 Recommended bootstrap composition
The frontend should compose bootstrap state from:
1. `POST /auth/login` or `POST /auth/refresh`
2. branding/settings endpoints
3. entitlements endpoint
4. user feature access endpoint
5. optionally users/team endpoint when admin shell needs it

## 3.2 Minimum bootstrap shape used by frontend

```ts
export type PortalBootstrap = {
  auth: {
    accessToken: string;
    refreshToken?: string;
    mustChangePassword?: boolean;
    mfaVerified?: boolean;
  };
  session: {
    userId: string | null;
    tenantId: string | null;
    email?: string | null;
    permissions: string[];
  };
  branding: {
    displayName?: string | null;
    legalName?: string | null;
    primaryColor?: string | null;
    secondaryColor?: string | null;
    accentColor?: string | null;
    textColor?: string | null;
    backgroundColor?: string | null;
    themeMode?: string | null;
    defaultFontFamily?: string | null;
    supportEmail?: string | null;
    supportPhone?: string | null;
    websiteUrl?: string | null;
    tagline?: string | null;
    assets?: Array<{
      id: string;
      assetType: string;
      storageKey: string;
      originalFileName: string;
      contentType: string;
      isActive: boolean;
    }>;
  } | null;
  tenantSettings: {
    timezone?: string | null;
    locale?: string | null;
    dateFormat?: string | null;
    currency?: string | null;
    numberFormat?: string | null;
    defaultCountry?: string | null;
    settingsJson?: string | null;
  } | null;
  entitlements: Array<{
    featureKey: string;
    granted: boolean;
    limitValue?: number | null;
    source?: string | null;
    effectiveFrom?: string | null;
    effectiveTo?: string | null;
  }>;
  myFeatureAccess: {
    assignedFeatures: string[];
    effectiveFeatures?: string[];
    limits?: Record<string, number | null>;
  } | null;
};
```

## 3.3 Frontend boot sequence
1. call login or refresh
2. store tokens securely according to frontend auth policy
3. derive `tenantId` and `userId` from token claims where possible
4. fetch branding/settings/entitlements/feature access
5. build runtime capability map
6. render shell/navigation based on capabilities and tenant branding

---

## 4. Auth and session integration

## 4.1 Backend endpoints
### Login
- `POST /auth/login`

Returns:
- `accessToken`
- `refreshToken`
- `mustChangePassword`
- `permissions`
- `mfaVerified`

### Register
- `POST /auth/register`

Returns:
- `tenantId`
- `userId`
- `accessToken`
- `refreshToken`
- `permissions`

### Refresh
- `POST /auth/refresh`

Returns:
- `accessToken`
- `refreshToken`
- `permissions`

### Logout
- `POST /auth/logout`
- `POST /identity/logout`

### Session listing
- `GET /identity/me/sessions`
- `DELETE /identity/me/sessions/{sessionId}`
- `DELETE /identity/users/{userId}/sessions`

## 4.2 Frontend auth routes
| Frontend route | Purpose | Backend endpoint(s) |
|---|---|---|
| `/login` | user sign-in | `POST /auth/login` |
| `/forgot-password` | placeholder recovery flow for MVP if available later | no confirmed endpoint yet in inspected controllers |
| `/reset-password` | password reset completion if implemented later | no confirmed endpoint yet in inspected controllers |
| session management UI under settings/security | list/revoke sessions | `GET /identity/me/sessions`, `DELETE /identity/me/sessions/{sessionId}` |

## 4.3 Immediate implementation note
From current controller inspection, login/refresh/logout are confirmed.
Forgot/reset password endpoints were **not** confirmed in the inspected controller set, so those frontend routes should remain UI placeholders until backend support is confirmed/added.

---

## 5. Tenant branding and settings integration

## 5.1 Confirmed backend endpoints
### Branding
- `GET /tenant-branding`
- `PUT /tenant-branding`
- `GET /tenant-branding/assets`
- `POST /tenant-branding/assets`
- `DELETE /tenant-branding/assets/{assetId}`

### Tenant settings
- `GET /identity/tenant-settings`
- `PUT /identity/tenant-settings`

## 5.2 Frontend route mapping
| Frontend route | Purpose | Backend endpoint(s) |
|---|---|---|
| `/settings` | settings overview shell | compose child settings endpoints |
| `/settings` branding section | tenant display/theme editor | `GET /tenant-branding`, `PUT /tenant-branding` |
| `/settings` branding assets | logo/background asset management | `GET /tenant-branding/assets`, `POST /tenant-branding/assets`, `DELETE /tenant-branding/assets/{assetId}` |
| `/settings` tenant preferences/profile | timezone/locale/currency/etc | `GET /identity/tenant-settings`, `PUT /identity/tenant-settings` |

## 5.3 Frontend theme contract
Frontend theme provider should map branding payload to CSS variables / theme tokens.
The frontend should tolerate:
- no branding record yet -> apply default Voyara brand preset
- no assets yet -> show default placeholders
- partial branding -> merge custom values over default tokens

---

## 6. Entitlements and feature assignment integration

## 6.1 Confirmed backend endpoints
### Tenant entitlements
- `GET /billing/entitlements/me`
- `GET /billing/entitlements/{tenantId}`
- `POST /billing/entitlements/{tenantId}/grants`
- `POST /billing/entitlements/{tenantId}/packages`
- `POST /billing/entitlements/{tenantId}/overrides`

### Tenant billing package and override management
- `GET /billing/tenants/{tenantId}/packages`
- `POST /billing/tenants/{tenantId}/packages`
- `PUT /billing/tenants/{tenantId}/packages/{assignmentId}`
- `DELETE /billing/tenants/{tenantId}/packages/{assignmentId}`
- `GET /billing/tenants/{tenantId}/feature-overrides`
- `POST /billing/tenants/{tenantId}/feature-overrides`
- `PUT /billing/tenants/{tenantId}/feature-overrides/{overrideId}`
- `DELETE /billing/tenants/{tenantId}/feature-overrides/{overrideId}`
- `GET /billing/tenants/{tenantId}/entitlements`
- `GET /billing/tenants/{tenantId}/entitlements/{featureKey}`

### User feature assignments
- `GET /billing/feature-access/me`
- `GET /billing/tenants/{tenantId}/feature-allocations`
- `GET /billing/tenants/{tenantId}/users/{userId}/features`
- `POST /billing/tenants/{tenantId}/users/{userId}/feature-assignments`
- `DELETE /billing/tenants/{tenantId}/users/{userId}/feature-assignments/{featureKey}`

## 6.2 Frontend route mapping
| Frontend route | Purpose | Backend endpoint(s) |
|---|---|---|
| app bootstrap | build tenant feature map | `GET /billing/entitlements/me`, `GET /billing/feature-access/me` |
| `/settings` subscription/plan | plan/package visibility | `GET /billing/tenants/{tenantId}/packages`, `GET /billing/tenants/{tenantId}/entitlements` |
| `/settings` feature allocations | tenant admin feature allocation UI | `GET /billing/tenants/{tenantId}/feature-allocations` |
| `/users` detail drawer or allocation panel | see a user’s assigned features | `GET /billing/tenants/{tenantId}/users/{userId}/features` |
| `/users` allocation actions | assign/revoke user features | `POST /billing/tenants/{tenantId}/users/{userId}/feature-assignments`, `DELETE /billing/tenants/{tenantId}/users/{userId}/feature-assignments/{featureKey}` |

## 6.3 Frontend capability resolver
Frontend should normalize backend data into a single capability resolver:

```ts
canAccess(featureKey, permissionKey, assignmentMode?)
```

The resolver should evaluate:
- permission list from auth token/login response
- entitlement list from billing
- assignment list from user feature access

---

## 7. Users/team integration

## 7.1 Confirmed backend endpoints
- `GET /identity/users`
- `POST /identity/users`
- `GET /identity/users/{userId}`
- `PUT /identity/users/{userId}`
- `DELETE /identity/users/{userId}`
- `POST /identity/users/{userId}/suspend`
- `POST /identity/users/{userId}/reactivate`

## 7.2 Frontend route mapping
| Frontend route | Purpose | Backend endpoint(s) |
|---|---|---|
| `/users` | team list | `GET /identity/users` |
| `/users` create modal | add user | `POST /identity/users` |
| `/users/[userId]` or drawer | read user detail | `GET /identity/users/{userId}` |
| `/users` edit action | update role/password | `PUT /identity/users/{userId}` |
| `/users` suspend/reactivate actions | user status lifecycle | `POST /identity/users/{userId}/suspend`, `POST /identity/users/{userId}/reactivate` |
| `/users` feature allocation panel | combine user with billing assignment data | identity user endpoints + billing assignment endpoints |

## 7.3 Frontend note
The users surface should not stop at CRUD.
It should visibly connect:
- user identity/status
- role/permission role summary
- assigned product features
- subscription allocation visibility

---

## 8. Inquiries and draft concepts integration

## 8.1 Confirmed backend endpoints
### Inquiry root
- `GET /travel/inquiries`
- `GET /travel/inquiries/{id}`
- `GET /travel/inquiries/{id}/history`
- `POST /travel/inquiries/{id}/assign`
- `POST /travel/inquiries/{id}/qualify`
- `POST /travel/inquiries/{id}/disqualify?status=...`
- `POST /travel/inquiries/{id}/mark-contacted`
- `POST /travel/inquiries/{id}/archive`
- `POST /travel/inquiries/{id}/convert-to-quotation`

### Draft concepts under inquiry
- `GET /travel/inquiries/{id}/concepts`
- `GET /travel/inquiries/{id}/concepts/{conceptId}`
- `POST /travel/inquiries/{id}/concepts`
- `POST /travel/inquiries/{id}/concepts/{conceptId}/mark-primary`
- `POST /travel/inquiries/{id}/concepts/{conceptId}/archive`

### Public intake
- `PublicInquiriesController.cs` exists in repo, but exact endpoints were not inspected in this pass.

## 8.2 Frontend route mapping
| Frontend route | Purpose | Backend endpoint(s) |
|---|---|---|
| `/inquiries` | inquiry queue/list | `GET /travel/inquiries` |
| `/inquiries/[inquiryId]` | inquiry detail root | `GET /travel/inquiries/{id}` |
| `/inquiries/[inquiryId]` history/timeline | lifecycle history | `GET /travel/inquiries/{id}/history` |
| `/inquiries/[inquiryId]` assignment/status actions | assign/qualify/contact/archive | relevant POST action endpoints |
| `/inquiries/[inquiryId]/concepts` or inquiry page concepts section | concept list | `GET /travel/inquiries/{id}/concepts` |
| concept create modal | create concept | `POST /travel/inquiries/{id}/concepts` |
| concept actions | mark primary/archive | concept action POST endpoints |
| inquiry-to-quote action | create quotation from inquiry/concept | `POST /travel/inquiries/{id}/convert-to-quotation` |

## 8.3 Query parameter contract for inquiry list
Current list supports:
- `page`
- `pageSize`
- `status`
- `source`
- `assignedToUserId`
- `destination`
- `q`

Frontend filter state should mirror those directly.

---

## 9. Contacts integration

## 9.1 Confirmed backend endpoints
- `POST /travel/contacts`
- `GET /travel/contacts/{id}`
- `GET /travel/contacts`
- `GET /travel/contacts/search?q=...`
- `PUT /travel/contacts/{id}`
- `DELETE /travel/contacts/{id}`

## 9.2 Frontend route mapping
| Frontend route | Purpose | Backend endpoint(s) |
|---|---|---|
| `/contacts` | contact list/search | `GET /travel/contacts`, `GET /travel/contacts/search` |
| contact detail drawer/page | get contact | `GET /travel/contacts/{id}` |
| contact create/edit | create/update | `POST /travel/contacts`, `PUT /travel/contacts/{id}` |
| delete action | remove contact | `DELETE /travel/contacts/{id}` |

---

## 10. Quotations integration

## 10.1 Confirmed backend endpoints
- `POST /travel/quotations`
- `GET /travel/quotations`
- `GET /travel/quotations/{id}`
- `PUT /travel/quotations/{id}`
- `GET /travel/quotations/{id}/history`
- `GET /travel/quotations/{id}/revisions`
- `GET /travel/quotations/{id}/revisions/{revisionId}`
- `POST /travel/quotations/{id}/revisions`
- `POST /travel/quotations/{id}/send`
- `POST /travel/quotations/{id}/approval-requests`
- `POST /travel/quotations/{id}/approval-requests/{approvalRequestId}/approve`
- `POST /travel/quotations/{id}/approval-requests/{approvalRequestId}/reject`
- `POST /travel/quotations/{id}/accept`
- `POST /travel/quotations/{id}/reject`
- `POST /travel/quotations/{id}/expire`
- `POST /travel/quotations/{id}/attachments`
- `GET /travel/quotations/{id}/attachments`
- `DELETE /travel/quotations/{id}/attachments/{attachmentId}`
- public endpoints:
  - `GET /travel/quotations/public/{token}`
  - `POST /travel/quotations/public/{token}/viewed`
  - `POST /travel/quotations/public/{token}/accept`
  - `POST /travel/quotations/public/{token}/reject`
- legacy compatibility only:
  - `POST /travel/quotations/{id}/convert`

## 10.2 Frontend route mapping
| Frontend route | Purpose | Backend endpoint(s) |
|---|---|---|
| `/quotations` | quotation list | `GET /travel/quotations` |
| `/quotations/[quotationId]` | quotation detail | `GET /travel/quotations/{id}` |
| quotation history tab | commercial audit trail | `GET /travel/quotations/{id}/history` |
| quotation revisions tab | revision list/read | `GET /travel/quotations/{id}/revisions`, `GET /travel/quotations/{id}/revisions/{revisionId}` |
| create quotation | create | `POST /travel/quotations` |
| create revision | new revision | `POST /travel/quotations/{id}/revisions` |
| send/share actions | send quotation | `POST /travel/quotations/{id}/send` |
| approval workflow | approval requests | approval request endpoints |
| accept/reject/expire actions | lifecycle actions | corresponding POST endpoints |
| attachments UI | upload/list/delete attachments | attachment endpoints |
| public quote page (if implemented in frontend or customer surface) | public quote read/decision | public token endpoints |

## 10.3 Critical rule
Do not wire the main frontend action path to `POST /travel/quotations/{id}/convert`.
That endpoint is explicitly legacy.
Preferred path is:
- quotation accepted
- create booking via booking endpoint
- manage confirmed itinerary under booking

---

## 11. Booking and confirmed itinerary integration

## 11.1 Confirmed backend endpoints
### Booking root
- `POST /travel/bookings/from-quotation/{quotationId}`
- `GET /travel/bookings`
- `GET /travel/bookings/{id}`
- `GET /travel/bookings/{id}/financial-summary`
- `GET /travel/bookings/{id}/itinerary`
- `POST /travel/bookings/{id}/itinerary`

### Travelers
- `POST /travel/bookings/{id}/travelers`
- `GET /travel/bookings/{id}/travelers`
- `PUT /travel/bookings/{id}/travelers/{travelerId}`
- `DELETE /travel/bookings/{id}/travelers/{travelerId}`

### Booking items / fulfillment
- `POST /travel/bookings/{id}/items`
- `GET /travel/bookings/{id}/items`
- `PUT /travel/bookings/{id}/items/{itemId}`
- `PATCH /travel/bookings/{id}/items/{itemId}/status`
- `POST /travel/bookings/{id}/items/{itemId}/request-confirmation`
- `POST /travel/bookings/{id}/items/{itemId}/confirm`
- `POST /travel/bookings/{id}/items/{itemId}/issue`
- `DELETE /travel/bookings/{id}/items/{itemId}`

### Booking documents
- `POST /travel/bookings/{id}/documents`
- `GET /travel/bookings/{id}/documents`
- `DELETE /travel/bookings/{id}/documents/{documentId}`

## 11.2 Frontend route mapping
| Frontend route | Purpose | Backend endpoint(s) |
|---|---|---|
| `/bookings` | booking list | `GET /travel/bookings` |
| `/bookings/[bookingId]` | booking detail | `GET /travel/bookings/{id}` |
| booking financial widget | payment visibility in booking context | `GET /travel/bookings/{id}/financial-summary` |
| booking itinerary section | get/create confirmed itinerary | `GET /travel/bookings/{id}/itinerary`, `POST /travel/bookings/{id}/itinerary` |
| traveler management UI | list/create/update/delete travelers | traveler endpoints |
| fulfillment/items panel | list/create/update/status/confirm/issue item actions | booking item endpoints |
| documents/vouchers panel | upload/list/delete documents | booking document endpoints |
| quotation accepted -> create booking CTA | create operational root from quotation | `POST /travel/bookings/from-quotation/{quotationId}` |

## 11.3 Booking list query parameters
Current booking list supports:
- `page`
- `pageSize`
- `status`
- `destination`
- `startDateFrom`
- `startDateTo`
- `assignedToUserId`
- `primaryContactId`

Frontend filters should mirror these.

---

## 12. Itinerary read integration

## 12.1 Confirmed backend endpoints
### Preferred itinerary access
- `GET /travel/bookings/{id}/itinerary`
- `POST /travel/bookings/{id}/itinerary`

### Standalone itinerary controller
- `GET /travel/itineraries`
- `GET /travel/itineraries/{id}`
- `PUT /travel/itineraries/{id}`
- legacy create:
  - `POST /travel/itineraries` (obsolete)

## 12.2 Frontend route mapping
| Frontend route | Purpose | Backend endpoint(s) |
|---|---|---|
| `/itineraries` | itinerary list/reporting view | `GET /travel/itineraries` |
| booking detail itinerary tab | preferred confirmed itinerary view | `GET /travel/bookings/{id}/itinerary` |
| booking itinerary create action | create booking-owned itinerary | `POST /travel/bookings/{id}/itinerary` |
| `/itineraries/[id]` if needed later | standalone itinerary read/edit | `GET /travel/itineraries/{id}`, `PUT /travel/itineraries/{id}` |

## 12.3 Critical frontend behavior rule
When rendering itinerary navigation/copy, always frame it as:
- booking-linked confirmed itinerary
not:
- quote conversion target

---

## 13. Follow-ups integration

## 13.1 Confirmed backend endpoints
- `POST /travel/follow-ups`
- `GET /travel/follow-ups`
- `GET /travel/follow-ups/{id}`
- `PUT /travel/follow-ups/{id}`
- `POST /travel/follow-ups/{id}/complete`
- `POST /travel/follow-ups/{id}/reassign`

## 13.2 Frontend route mapping
| Frontend route | Purpose | Backend endpoint(s) |
|---|---|---|
| `/follow-ups` | task list | `GET /travel/follow-ups` |
| follow-up detail drawer | read detail | `GET /travel/follow-ups/{id}` |
| create/update task | create/update | `POST /travel/follow-ups`, `PUT /travel/follow-ups/{id}` |
| complete/reassign actions | workflow execution | `POST /travel/follow-ups/{id}/complete`, `POST /travel/follow-ups/{id}/reassign` |

## 13.3 Follow-up list query parameters
Current list supports:
- `page`
- `pageSize`
- `status`
- `customerName`
- `dueDateFrom`
- `dueDateTo`

---

## 14. Communications and templates integration

## 14.1 Confirmed backend endpoints
### Notifications
- `POST /communication/notifications`
- `POST /communication/notifications/workflows/{workflowType}`
- `GET /communication/notifications`
- `GET /communication/notifications/{id}`
- `GET /communication/notifications/recipient/{recipientId}`
- `GET /communication/notifications/recipient/{recipientId}/unread-count`
- `PATCH /communication/notifications/{id}/read`
- `POST /communication/notifications/{id}/replay`

### Templates
- `POST /communication/templates`
- `GET /communication/templates`
- `GET /communication/templates/{id}`
- `PUT /communication/templates/{id}`

## 14.2 Frontend route mapping
| Frontend route | Purpose | Backend endpoint(s) |
|---|---|---|
| `/communications` | communication activity list | `GET /communication/notifications` |
| communication detail drawer | notification detail | `GET /communication/notifications/{id}` |
| recipient-specific communication panel | by recipient | `GET /communication/notifications/recipient/{recipientId}` |
| unread count widgets | counts | `GET /communication/notifications/recipient/{recipientId}/unread-count` |
| replay action | replay failed message | `POST /communication/notifications/{id}/replay` |
| mark read action | update status | `PATCH /communication/notifications/{id}/read` |
| `/settings` template center or `/communications/templates` | list/edit templates | template endpoints |

## 14.3 List query contract
Notification list currently supports:
- `status`
- `channel`
- `referenceId`
- `correlationId`
- `workflowType`
- `recipientId`
- `page`
- `pageSize`

---

## 15. Billing visibility integration

## 15.1 Confirmed backend endpoints from inspected controllers
- `GET /billing/tenants/{tenantId}/packages`
- `GET /billing/tenants/{tenantId}/feature-overrides`
- `GET /billing/tenants/{tenantId}/entitlements`
- `GET /billing/tenants/{tenantId}/entitlements/{featureKey}`
- `GET /billing/entitlements/me`
- `GET /billing/entitlements/{tenantId}`
- `GET /travel/bookings/{id}/financial-summary`

Other billing controllers exist in repo (`InvoicesController`, `SubscriptionsController`, `TenantInvoiceReadController`, `DashboardController`) but were not fully inspected in this pass, so this contract only treats the confirmed endpoints above as hard-known.

## 15.2 Frontend route mapping
| Frontend route | Purpose | Backend endpoint(s) |
|---|---|---|
| `/billing` | tenant billing overview | start with tenant packages + entitlements + booking financial summaries where applicable |
| `/settings` subscription section | plan and entitlement administration | billing tenant package/entitlement endpoints |
| booking detail finance card | booking-level payment projection | `GET /travel/bookings/{id}/financial-summary` |

## 15.3 Immediate implementation guidance
The billing page should initially be driven by:
- tenant package assignments
- feature overrides
- effective entitlements
- booking financial summaries

If deeper invoice/subscription read models are required, inspect `InvoicesController`, `SubscriptionsController`, and `TenantInvoiceReadController` next and extend this contract.

---

## 16. Route mapping summary

## 16.1 Frontend route -> backend source map

| Frontend route | Primary backend source(s) |
|---|---|
| `/login` | `POST /auth/login` |
| `/forgot-password` | backend not confirmed yet |
| `/reset-password` | backend not confirmed yet |
| `/` dashboard | compose from billing/travel/communication summaries as implemented |
| `/inquiries` | `GET /travel/inquiries` |
| `/inquiries/[inquiryId]` | `GET /travel/inquiries/{id}`, `/history`, `/concepts` |
| `/cases` | composed list view from inquiries/bookings/quotations depending case model |
| `/cases/[caseId]` | composed workspace from inquiry + quotation + booking + follow-up + communications + financial summary |
| `/contacts` | `GET /travel/contacts`, `/search` |
| `/quotations` | `GET /travel/quotations` |
| `/quotations/[quotationId]` | quotation detail/history/revisions/attachments/actions |
| `/bookings` | `GET /travel/bookings` |
| `/bookings/[bookingId]` | booking detail + itinerary + travelers + items + documents + financial summary |
| `/itineraries` | `GET /travel/itineraries` plus booking itinerary reads |
| `/follow-ups` | `GET /travel/follow-ups` |
| `/communications` | `GET /communication/notifications` |
| `/billing` | billing tenant entitlements/packages + travel booking financial summary |
| `/users` | `GET /identity/users` + billing feature allocation endpoints |
| `/settings` | branding + tenant settings + entitlements + feature allocations + templates |
| `/webhooks` | controller exists in billing/stripe and other services, but webhook management read contract not fully inspected yet |

---

## 17. Frontend API module plan for `voyara-portal`

Recommended client modules:

```ts
lib/api/
  client.ts
  auth.ts
  bootstrap.ts
  branding.ts
  tenantSettings.ts
  entitlements.ts
  featureAssignments.ts
  users.ts
  inquiries.ts
  concepts.ts
  contacts.ts
  quotations.ts
  bookings.ts
  itineraries.ts
  followUps.ts
  communications.ts
  templates.ts
  billing.ts
```

### Recommended responsibilities
- `auth.ts` -> login/refresh/logout/session endpoints
- `bootstrap.ts` -> compose portal bootstrap payload
- `branding.ts` -> tenant branding + assets
- `tenantSettings.ts` -> locale/timezone/currency/settings
- `entitlements.ts` -> tenant entitlements + packages/overrides
- `featureAssignments.ts` -> current user feature access + admin user assignment endpoints
- `inquiries.ts` -> inquiry root actions and concept actions
- `quotations.ts` -> quotation lifecycle/revisions/attachments
- `bookings.ts` -> booking root, itinerary, travelers, items, documents, financial summary
- `communications.ts` -> notifications
- `templates.ts` -> communication templates

---

## 18. Gaps and next inspection targets

This pass confirmed a lot, but not everything.
Remaining useful inspections:
- `PublicInquiriesController.cs` for public lead capture contract
- `TenantInvoiceReadController.cs`, `InvoicesController.cs`, `SubscriptionsController.cs`, `DashboardController.cs` for deeper billing page contracts
- `RecipientPreferencesController.cs` for notification preferences UI
- `TenantTemplateThemesController.cs` for template theme linkage
- `TenantsController.cs`, `RolesController.cs`, `UserInvitationsController.cs` for richer admin/settings flows
- `ReportsController.cs`, `TimelineController.cs`, `NotesController.cs` for dashboard/case workspace enrichment

---

## 19. Final blunt conclusion

The backend already exposes enough real surface area to stop guessing.
The frontend can now be built against a concrete contract instead of vibes.

The immediate build order should be:
1. auth/bootstrap client
2. branding/settings/entitlements startup composition
3. inquiry routes
4. quotation routes
5. booking routes
6. users + feature allocation
7. communications/billing/settings expansion

That will keep the frontend honest, tenant-aware, and actually connected to backend truth.