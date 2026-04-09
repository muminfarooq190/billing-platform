# Frontend Stepwise Implementation Plan

## 1. Objective

This implementation plan breaks frontend delivery into practical steps for:
- **Admin portal**
- **Customer-facing web portal**
- shared frontend infrastructure

The plan is aligned to the backend schema, service boundaries, and currently implemented routes in the repository.

---

## 2. Guiding principles

- Build shared foundations once, reuse everywhere.
- Prioritize admin workflows first because backend maturity is strongest there.
- Keep customer-facing features focused on exposed and stable workflows.
- Mirror backend bounded contexts in frontend feature modules.
- Avoid overbuilding custom frontend infrastructure before real screens exist.

---

## 3. Delivery phases overview

### Phase 0 - Discovery and alignment
### Phase 1 - Frontend monorepo foundation
### Phase 2 - Auth and shell architecture
### Phase 3 - Admin portal MVP core modules
### Phase 4 - Admin portal advanced operational modules
### Phase 5 - Customer web portal MVP
### Phase 6 - Customer web portal advanced capabilities
### Phase 7 - Production hardening and scale readiness

---

## 4. Phase 0 - Discovery and alignment

## Goals
- Finalize frontend stack and monorepo decision
- Normalize route conventions from gateway to frontend client
- Confirm user roles/personas
- Confirm customer-auth assumptions vs public-token flows

## Tasks
1. Review backend endpoints and create route matrix
2. Define frontend apps and shared packages
3. Define page inventory for admin and customer experiences
4. Define role matrix:
   - Owner
   - Admin
   - Member
   - Customer/Traveler (future or token-based initially)
5. Define core design principles and token names
6. Confirm environments:
   - local
   - staging
   - production

## Deliverables
- route mapping sheet
- page inventory
- role access matrix
- frontend architecture approval

---

## 5. Phase 1 - Frontend monorepo foundation

## Goals
Create a scalable workspace for web and mobile without building domain screens yet.

## Recommended structure

```text
frontend/
  apps/
    admin-portal/
    customer-portal/
    customer-mobile/
  packages/
    ui/
    design-tokens/
    api-client/
    auth/
    types/
    utils/
```

## Tasks

### 5.1 Workspace setup
1. Initialize monorepo with Turborepo or pnpm workspace
2. Create shared TypeScript config
3. Configure ESLint + Prettier + import rules
4. Configure environment variable patterns
5. Add CI checks for lint/typecheck/test/build

### 5.2 Shared packages
1. Create `packages/design-tokens`
2. Create `packages/ui`
3. Create `packages/types`
4. Create `packages/utils`
5. Create `packages/api-client`
6. Create `packages/auth`

### 5.3 Tooling
1. Add Storybook for UI package
2. Add Playwright for web E2E
3. Add MSW for local API mocking
4. Add testing library + Vitest/Jest

## Deliverables
- working monorepo
- shared CI pipeline
- base UI package
- base API client package

---

## 6. Phase 2 - Auth and application shell

## Goals
Establish navigation, protected routes, session handling, and global layouts.

## Tasks

### 6.1 Shared auth implementation
1. Build login flow using `POST /api/auth/login`
2. Build registration flow using `POST /api/auth/register`
3. Build refresh flow using `POST /api/auth/refresh`
4. Build logout flow using `POST /api/auth/logout`
5. Implement token refresh interceptor logic
6. Implement role-aware route guard

### 6.2 Admin portal shell
1. Create app layout:
   - sidebar
   - topbar
   - breadcrumb
   - profile menu
2. Add role-aware menu rendering
3. Add global command/search placeholder
4. Add notification bell placeholder

### 6.3 Customer portal shell
1. Create responsive layout:
   - top nav / bottom nav mobile strategy
   - account menu
   - notification icon
2. Add public layout for quotation share links
3. Add protected customer area layout for future authenticated users

## Deliverables
- working login/register/logout
- route protection
- base admin shell
- base customer shell

---

## 7. Phase 3 - Admin portal MVP core modules

This phase should produce a usable internal operations product.

## Priority order
1. Dashboard
2. Contacts
3. Quotations
4. Follow-ups
5. Itineraries
6. Bookings
7. Billing summary
8. Users

---

## 8. Phase 3A - Admin Dashboard

## Goals
Provide a useful operational entry point.

## Tasks
1. Build KPI cards using available billing/travel/communication aggregates
2. Build recent activity widget using timeline/activity where possible
3. Build due follow-ups widget
4. Build invoices due/overdue widget
5. Build recent quotations/bookings summary widgets

## UI components needed
- MetricCard
- StatusBadge
- ActivityList
- FilterChip
- ChartCard

## Deliverable
- dashboard landing page for admin users

---

## 9. Phase 3B - Contacts module

## Goals
Deliver CRM basics.

## Backend routes used
- `POST /api/travel/travel/contacts`
- `GET /api/travel/travel/contacts`
- `GET /api/travel/travel/contacts/{id}`
- `GET /api/travel/travel/contacts/search`
- `PUT /api/travel/travel/contacts/{id}`
- `DELETE /api/travel/travel/contacts/{id}`

## Tasks
1. Build contacts list page with pagination
2. Add search and tag filter
3. Build create contact form
4. Build edit contact form
5. Build delete confirmation flow
6. Build contact detail drawer/page
7. Link contacts to quotations/follow-ups if present in UI

## Deliverable
- fully usable contacts CRM module

---

## 10. Phase 3C - Quotations module

## Why this is critical
Quotation is the center of the travel sales workflow and already has deep backend support.

## Backend capabilities already available
- create quotation
- update quotation
- list quotation
- get quotation detail
- create revisions
- list revisions
- get revision detail
- send quotation
- accept/reject/expire quotation
- upload/list/delete attachments
- public token access
- viewed tracking
- convert to itinerary
- timeline support

## Tasks

### Quotations list
1. Build quotations table
2. Add filters:
   - status
   - customer name
   - travel date range
3. Add status badges and row actions

### Quotation detail
4. Build detail page header with key fields
5. Add tabs:
   - overview
   - revisions
   - attachments
   - history
   - timeline
6. Show validity, share status, last viewed state

### Quotation creation/editing
7. Build quotation create form
8. Build draft edit form
9. Build line-item editor
10. Add currency validation and total calculations

### Revision workflow
11. Build create revision dialog/page
12. Show revision comparison summary
13. Build revision detail view

### Customer delivery workflow
14. Build send quotation dialog
15. Add copy public link action
16. Add expire/reject/accept controls where appropriate
17. Add convert-to-itinerary action

### Attachments
18. Build file uploader
19. Add customer-visible toggle
20. Add attachment gallery/list view

## Deliverable
- production-usable quotation workflow in admin portal

---

## 11. Phase 3D - Follow-ups module

## Backend routes used
- create
- list
- detail
- update

## Tasks
1. Build follow-up list with filters:
   - status
   - customer name
   - due date range
2. Highlight overdue and due-today items
3. Build create/edit form
4. Add assignee selector
5. Add status transition controls

## Deliverable
- operations queue for follow-up management

---

## 12. Phase 3E - Itineraries module

## Backend routes used
- create itinerary
- get itinerary
- list itineraries
- update itinerary

## Tasks
1. Build itineraries list page
2. Build itinerary creation form
3. Build itinerary detail page
4. Build day-wise item rendering UX
5. Build draft/confirmed/in-progress/completed/cancelled status UI
6. Support quotation-linked itinerary display

## Deliverable
- itinerary management feature for operations team

---

## 13. Phase 3F - Bookings module

## Why this matters
Bookings convert accepted commercial intent into operational fulfillment.

## Backend routes used
- create from quotation
- list bookings
- get booking detail
- traveler CRUD
- booking item CRUD
- item status patch
- booking document upload/list/delete
- timeline

## Tasks

### Booking list
1. Build bookings list with filters:
   - status
   - destination
   - date range
   - assigned user
   - primary contact

### Booking detail
2. Build booking summary page
3. Add tabs:
   - summary
   - travelers
   - items
   - documents
   - timeline

### Travelers
4. Build traveler list
5. Build add/edit traveler form
6. Validate passport expiry and travel details UX

### Booking items
7. Build item list/board
8. Build add/edit item form
9. Build item status quick-update interactions
10. Show supplier, voucher, confirmation, amount, currency fields clearly

### Documents
11. Build document upload flow
12. Show file metadata and traveler linkage
13. Show customer-visible indicator
14. Enable download/delete actions

## Deliverable
- booking operations module

---

## 14. Phase 3G - Billing MVP in admin portal

## Backend routes used
- dashboard
- subscription detail/create/cancel
- invoice list/detail/generate/pay

## Tasks
1. Build billing dashboard summary page
2. Build subscription card section
3. Build invoice list with status filter
4. Build invoice detail page
5. Add generate invoice action
6. Add pay invoice action

## Deliverable
- basic internal billing operations support

---

## 15. Phase 3H - Users and tenant settings

## Tasks
1. Build tenant detail/settings page
2. Build users list
3. Build create/edit user forms
4. Add role badges
5. Add tenant plan change flow
6. Add suspend tenant confirmation flow

## Deliverable
- internal tenant and user management

---

## 16. Phase 4 - Admin portal advanced operational modules

## Goals
Complete secondary modules and production niceties.

## 16.1 Communication module

### Tasks
1. Build notifications list by recipient
2. Build unread count indicators
3. Build templates list and editor
4. Build recipient preference editor
5. Add timezone and quiet-hours UX

## 16.2 Webhooks module

### Tasks
1. Build webhook subscriptions list
2. Build create subscription form
3. Build delivery logs list
4. Build delivery detail drawer
5. Add replay action

## 16.3 Advanced dashboard analytics

### Tasks
1. Add conversion charts
2. Add destination analytics
3. Add booking margin trends when enough data exists
4. Add webhook failure monitoring widget

## Deliverables
- complete admin portal v1

---

## 17. Phase 5 - Customer web portal MVP

## Goal
Build a focused customer-facing web experience using existing backend capabilities.

## MVP scope
- quotation public view
- trip overview
- itinerary view
- documents view
- invoices view
- notifications center
- preferences

---

## 18. Phase 5A - Public quotation experience

## Why first
The backend already supports tokenized quotation sharing and viewed tracking.

## Tasks
1. Build `/quote/[token]` public route
2. Fetch quotation using `GET /travel/quotations/public/{token}`
3. Mark viewed using `POST /travel/quotations/public/{token}/viewed`
4. Create premium quotation page design:
   - trip summary
   - destination/travel dates
   - line-item breakdown
   - revision snapshot
   - attachments/media gallery
   - expiry information
5. Add mobile-first layout and polished empty/error states

## Deliverable
- strong public customer experience for quotations

---

## 19. Phase 5B - Customer authenticated shell

## Assumption
If customer auth is not ready, this phase can still be scaffolded behind feature flags.

## Tasks
1. Build customer dashboard shell
2. Add nav items:
   - home
   - trips
   - itinerary
   - documents
   - billing
   - notifications
   - preferences
3. Integrate auth/session strategy once customer identity is finalized

## Deliverable
- ready customer shell

---

## 20. Phase 5C - Trips and itinerary

## Tasks
1. Build trip list page
2. Build trip detail summary
3. Build itinerary timeline/day-by-day layout
4. Add booking item presentation suited for end users
5. Support responsive and print-friendly itinerary views

## Deliverable
- customer trip management experience

---

## 21. Phase 5D - Documents and billing

## Tasks
1. Build trip documents page
2. Group documents by trip and traveler
3. Show visibility-safe files only
4. Build invoices list and invoice detail page
5. Add due/paid indicators and helpful copy

## Deliverable
- customer document center and billing visibility

---

## 22. Phase 5E - Notifications and preferences

## Tasks
1. Build customer notification center
2. Add mark-as-read flow
3. Build channel preferences form
4. Add timezone and quiet-hours controls

## Deliverable
- communication self-service

---

## 23. Phase 6 - Customer portal advanced capabilities

## Tasks
1. Add richer trip progress indicators
2. Add document previewers
3. Add saved/offline-ready itinerary caching
4. Add support/request-help entry points
5. Add deeper billing/payment integrations when backend expands
6. Add customer acceptance/rejection actions if formally supported in customer UI flows

## Deliverable
- customer portal v2

---

## 24. Phase 7 - Production hardening and scale readiness

## Tasks
1. Add performance budgets
2. Add route-level code splitting and bundle analysis
3. Add robust error boundaries
4. Add audit logging hooks for sensitive admin actions
5. Add analytics and product telemetry
6. Add accessibility audits
7. Add visual regression checks
8. Add E2E coverage for primary workflows
9. Add load-safe API retry behavior and cache policies
10. Add i18n readiness if expansion is expected

## Deliverable
- release-ready frontend platform

---

## 25. Detailed module sequencing recommendation

## Admin portal recommended order
1. monorepo foundation
2. auth/session
3. admin shell
4. dashboard
5. contacts
6. quotations
7. follow-ups
8. itineraries
9. bookings
10. billing
11. users/tenant settings
12. communications
13. webhooks
14. hardening and analytics

## Customer portal recommended order
1. public quotation route
2. customer shell
3. trip overview
4. itinerary view
5. documents
6. billing
7. notifications
8. preferences
9. hardening and polish

---

## 26. Implementation plan by team streams

## Stream A - Platform/UI foundation
- monorepo
- design system
- auth package
- API client
- Storybook
- testing foundation

## Stream B - Admin workflows
- dashboard
- CRM
- quotations
- itineraries
- bookings
- billing
- settings

## Stream C - Customer experience
- public quote experience
- customer dashboard
- trip/documents/notifications/preferences

This allows parallel delivery with shared package reuse.

---

## 27. Definition of done for each module

Each module should be considered complete only when it includes:
- typed API integration
- loading/empty/error states
- mobile/responsive support where relevant
- success/error toasts or inline feedback
- role-aware access control
- unit tests for hooks/helpers
- Playwright coverage for critical path
- Storybook entries for reusable components

---

## 28. Recommended first MVP milestone

If only one milestone is needed first, build this:

### Admin MVP
- auth
- dashboard
- contacts
- quotations
- follow-ups
- bookings
- basic billing

### Customer MVP
- public quotation page only

This gives the highest business value fastest and directly leverages the strongest backend modules already present.

---

## 29. Suggested backlog epics

### Epic 1: Frontend platform foundation
### Epic 2: Authentication and app shell
### Epic 3: CRM contacts and follow-ups
### Epic 4: Quotation management
### Epic 5: Booking operations
### Epic 6: Billing workspace
### Epic 7: Identity and tenant settings
### Epic 8: Communication center
### Epic 9: Webhook operations
### Epic 10: Customer quotation experience
### Epic 11: Customer trip workspace
### Epic 12: Customer notifications and preferences
### Epic 13: Production hardening

---

## 30. Final execution recommendation

### Best practical rollout
- Start with a shared monorepo and component system.
- Ship the admin portal first because the backend already models the operational domain deeply.
- Launch customer-facing web initially with the public quotation experience.
- Expand into authenticated customer portal after identity strategy is finalized.
- Build mobile after customer web flows stabilize and shared packages are mature.

This gives a realistic, scalable, schema-aligned path from zero frontend to a multi-product frontend platform.
