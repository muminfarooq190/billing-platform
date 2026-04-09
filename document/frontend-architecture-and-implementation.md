# Frontend Architecture & Design Implementation Document

## 1. Purpose

This document proposes a scalable frontend architecture for the `billing-platform` / `Voyara` repository based on the current backend microservices, routes, domain aggregates, and database-oriented entities already implemented in the repo.

It covers:
- Admin portal architecture
- Customer-facing web portal architecture
- Shared frontend foundations
- Domain-driven information architecture
- API integration strategy
- UI design system direction
- Scalability, maintainability, and rollout considerations

This is grounded in the current backend services:
- Identity Service
- Travel Service
- Billing Service
- Communication Service
- Webhook Service
- API Gateway

---

## 2. Current backend domain model summary

### 2.1 Identity domain
Based on `IdentityDbContext` and aggregates:
- `Tenant`
  - id
  - name
  - email
  - plan
  - status
  - createdAt / updatedAt / deletedAt
- `User`
  - id
  - tenantId
  - email
  - passwordHash
  - role
  - lastLoginAt
  - createdAt / updatedAt / deletedAt

### 2.2 Billing domain
Based on `BillingDbContext` and aggregates:
- `Subscription`
  - id
  - tenantId
  - planType
  - billingCycle
  - status
  - startDate
  - nextBillingDate
  - cancelledAt
- `Invoice`
  - id
  - subscriptionId
  - tenantId
  - subtotal
  - taxAmount
  - total
  - status
  - dueDate
  - paidAt
  - issuedAt

### 2.3 Travel domain
Based on `TravelDbContext` and aggregates:
- `Contact`
- `Quotation`
- `QuotationRevision`
- `QuotationRevisionLineItem`
- `QuotationAttachment`
- `QuotationStatusHistory`
- `QuotationShareLink`
- `Booking`
- `BookingStatusHistory`
- `Traveler`
- `BookingItem`
- `BookingDocument`
- `Itinerary`
- `FollowUp`
- `ActivityEntry`

This is the richest domain in the system and should drive most of the portal UX.

### 2.4 Communication domain
Based on `CommunicationDbContext` and aggregates:
- `Notification`
- `NotificationTemplate`
- `RecipientPreferences`

### 2.5 Webhook domain
Based on NestJS + TypeORM migration:
- `webhook_subscriptions`
- `webhook_delivery_logs`

---

## 3. Frontend product surfaces to build

The repo supports three frontend products naturally:

1. **Admin Portal**
   - For internal staff, tenant owners, operators, travel consultants, finance/admin users
   - Main operational product

2. **Customer Web Portal**
   - For end customers/travelers
   - View quotations, itineraries, booking documents, notifications, preferences, invoices/payments

3. **Customer Mobile App**
   - End-user companion experience
   - Optimized for trip access, live itinerary, notifications, travel documents, and self-service

---

## 4. Recommended frontend technology stack

## 4.1 Web apps
Use a monorepo frontend with:
- **Next.js 15+** (App Router)
- **TypeScript**
- **React 19**
- **Tailwind CSS**
- **shadcn/ui** or custom headless design system primitives
- **TanStack Query** for server-state management
- **Zustand** for lightweight client-side UI state
- **React Hook Form + Zod** for typed forms and validation
- **TanStack Table** for data-heavy admin grids
- **Recharts** or **Apache ECharts** for dashboard analytics
- **MSW** for API mocking in development
- **Playwright** for E2E testing
- **Storybook** for design system and component documentation

## 4.2 Mobile app
Use:
- **React Native with Expo**
- **TypeScript**
- **Expo Router**
- **TanStack Query**
- **NativeWind** or Tamagui for scalable styling
- **Expo Notifications**
- **SecureStore** for token handling

## 4.3 Why this stack fits the repo
- Strong TypeScript support aligns with growing API complexity.
- Next.js supports both authenticated admin apps and customer public/share routes.
- React Native/Expo allows reuse of domain types, API clients, validation schemas, and design tokens.
- TanStack Query matches a microservice-backed API gateway architecture well.
- Zod enables schema-safe contracts against backend DTOs.

---

## 5. Recommended frontend repository structure

Since the backend is already a multi-service repo, add a frontend monorepo under a top-level `frontend/` folder.

### Proposed structure

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
    config/
```

### Package responsibilities

#### `apps/admin-portal`
Internal operations UI for CRM, billing, communication, and tenant management.

#### `apps/customer-portal`
Customer-facing responsive web portal for quotations, trips, documents, invoices, and preferences.

#### `apps/customer-mobile`
Mobile app for travelers.

#### `packages/ui`
Shared design system components:
- buttons
- cards
- tables
- form controls
- drawers
- modals
- timeline components
- status badges
- empty states
- KPI widgets

#### `packages/design-tokens`
Shared colors, spacing, typography, shadows, border radii, motion tokens, and semantic role colors.

#### `packages/api-client`
Typed API wrappers around gateway routes:
- `/api/auth/*`
- `/api/identity/*`
- `/api/travel/*`
- `/api/billing/*`
- `/api/communication/*`
- `/api/webhooks/*`

#### `packages/auth`
JWT handling, refresh workflow, route protection, role mapping, session hooks.

#### `packages/types`
Shared DTO types, enums, domain view models, filters, pagination models.

#### `packages/utils`
Date formatting, currency formatting, query helpers, file helpers, notification mappers.

---

## 6. Information architecture

## 6.1 Admin portal information architecture

### Primary navigation
- Dashboard
- CRM
  - Contacts
  - Follow-ups
- Quotations
- Itineraries
- Bookings
- Billing
  - Subscription
  - Invoices
  - Payments status view
- Communications
  - Notifications
  - Templates
  - Recipient preferences
- Webhooks
  - Subscriptions
  - Delivery logs
- Identity & Settings
  - Tenant profile
  - Users & roles
  - Plan management

### Reason this structure fits the schema
The main operational workflow in the database is:
`Contact -> Quotation -> Revision -> Acceptance -> Booking -> Travelers / Items / Documents -> Itinerary / Notifications / Billing`

The admin UX should mirror that lifecycle.

## 6.2 Customer portal information architecture

### Primary navigation
- Home / Overview
- Quotations
- Trips
- Itinerary
- Documents
- Billing
- Notifications
- Preferences / Profile

### Public/shared routes
The repo already supports tokenized public quotation access:
- `GET /travel/quotations/public/{token}`
- `POST /travel/quotations/public/{token}/viewed`

That implies a public customer-safe route such as:
- `/quote/[token]`

This should be SSR-friendly and highly polished because it is often the first customer touchpoint.

---

## 7. UX and design direction

## 7.1 Design principles
- Data-dense but calm for admin users
- Trust-building and premium for travelers/customers
- Strong status visibility for workflow-heavy records
- Mobile-first for customer portal, desktop-first for admin portal
- Clear state models: draft, sent, accepted, rejected, overdue, pending, confirmed, cancelled, etc.

## 7.2 Visual language

### Admin portal
- Clean enterprise CRM style
- Neutral base palette with role/status accents
- High-density tables with strong filtering
- Side panel drill-in for quick edits
- Timeline and activity feed as first-class UI patterns

### Customer portal
- Travel-inspired premium aesthetic
- Larger cards, visual destination emphasis, trip summaries
- Friendly progress states
- Strong document and itinerary usability

## 7.3 Status color mapping
Create semantic tokens that are reused across web and mobile.

Example:
- Draft = slate
- Sent = blue
- Accepted = green
- Rejected = red
- Expired = amber
- Pending = yellow
- Confirmed = emerald
- InProgress = indigo
- Completed = green-dark
- Cancelled = gray/red-muted
- Paid = green
- Overdue = orange/red
- Failed = red
- Delivered = teal

---

## 8. Domain-to-screen mapping

## 8.1 Identity screens

### Admin portal
- Login
- Register tenant
- Tenant overview
- Users list
- User create/edit modal
- Change tenant plan
- Suspend tenant confirmation flow

### Relevant backend routes
- `POST /api/auth/register`
- `POST /api/auth/login`
- `POST /api/auth/refresh`
- `POST /api/auth/logout`
- `GET /api/identity/tenants/{id}`
- `PATCH /api/identity/tenants/{id}/plan`
- `POST /api/identity/tenants/{id}/suspend`
- `GET /api/identity/identity/users`
- `POST /api/identity/identity/users`
- `PUT /api/identity/identity/users/{userId}`
- `DELETE /api/identity/identity/users/{userId}`

> Note: because of gateway path transforms and controller route attributes, frontend API client normalization is important. Do not scatter raw URLs across components.

## 8.2 Billing screens

### Admin portal
- Billing dashboard
- Subscription detail card
- Invoice list with filters
- Invoice detail
- Trigger invoice generation
- Mark/pay invoice action

### Customer portal
- My subscription/plan summary if exposed
- My invoices
- Payment history / status

### Relevant data points
From domain aggregates:
- planType
- billingCycle
- status
- nextBillingDate
- subtotal
- taxAmount
- total
- dueDate
- paidAt
- issuedAt

## 8.3 Travel CRM screens

### Contacts
- Contact list
- Search contacts
- Contact detail
- Contact create/edit
- Contact-linked activity summaries

### Follow-ups
- Follow-up list
- Due today / overdue / upcoming views
- Follow-up create/edit
- Assigned-to filtering

### Quotations
- Quotation list
- Quotation detail
- Quotation editor
- Revision history
- Attachment manager
- Send quotation dialog
- Accept/reject/expire history
- Public quotation preview

### Itineraries
- Itinerary list
- Itinerary builder
- Itinerary day-based detail view

### Bookings
- Booking list
- Booking detail
- Traveler management
- Booking items board/list
- Document center
- Booking timeline

### Timeline views
The repo already supports:
- `GET /travel/timeline/{entityType}/{entityId}`
- quotation timeline shortcut
- booking timeline shortcut

This should be surfaced as a reusable timeline widget across quotation and booking detail pages.

## 8.4 Communication screens

### Admin portal
- Notification queue/history by recipient
- Unread count widgets
- Template list/detail/editor
- Recipient preferences editor

### Customer portal
- Notification center
- Read/unread state
- Preferences management

## 8.5 Webhook screens

### Admin portal
- Webhook subscriptions list
- Create subscription form
- Delivery logs table
- Delivery log detail drawer
- Replay action

This is primarily an integration/admin feature, not a customer-facing one.

---

## 9. Frontend architecture patterns

## 9.1 BFF vs direct gateway integration
Recommended initial approach:
- Frontends call the **API Gateway** directly for authenticated business APIs.
- Use lightweight Next.js server actions / route handlers only for:
  - token bootstrap
  - cookie/session mediation
  - public quotation SSR rendering if needed
  - secure file proxying when browser constraints demand it

This avoids unnecessary duplication while retaining flexibility.

## 9.2 App architecture per web app

### Layering
- `app/` routes and layouts
- `features/` domain modules
- `components/` reusable page-level compositions
- `lib/` app-specific config/hooks
- shared packages for UI, API, auth, and types

### Feature-oriented module example
```text
features/
  quotations/
    api/
    components/
    hooks/
    schemas/
    mappers/
    types/
```

This scales better than organizing by technical type alone.

## 9.3 Data fetching strategy
Use TanStack Query for all authenticated, cacheable server state.

Recommended patterns:
- Queries per domain list/detail
- Mutations with optimistic updates only for safe UX moments
- Invalidate by domain keys
- Cursor/page query keys standardized

Examples:
- `['contacts', filters]`
- `['quotation', quotationId]`
- `['quotation-revisions', quotationId]`
- `['bookings', filters]`
- `['invoice-list', filters]`
- `['notifications', recipientId, page]`

## 9.4 Forms strategy
Use React Hook Form + Zod:
- aligned with DTO request contracts
- reusable schemas across web and mobile where relevant
- precise error mapping from backend validation/domain errors

---

## 10. Authentication and authorization model

## 10.1 Backend auth facts from repo
- JWT access token issued by identity service
- refresh token rotation supported
- role claim exists (`Admin`, `Owner`, `Member`, etc.)
- gateway validates JWT

## 10.2 Frontend auth recommendations

### Web
- Store access token in memory where possible
- Store refresh token in secure HTTP-only cookie via Next.js auth boundary or carefully managed secure storage strategy
- Auto-refresh on 401 once
- Force logout on refresh failure

### Mobile
- Store refresh token in `SecureStore`
- Short-lived access token in memory
- Background refresh on app resume

## 10.3 Route protection

### Admin portal roles
- `Owner`, `Admin` full access
- `Member` reduced operational access depending on feature gating

### Customer portal roles
Customer-facing identity likely needs either:
1. separate future customer auth model, or
2. mapped recipient/traveler identity using current identity service extensions.

For now, design the portal so it can start with:
- public token quotation flow
- invite-based access later
- eventual traveler/customer auth

---

## 11. API client design

Create a shared typed gateway client.

## 11.1 Client responsibilities
- base URL handling
- auth header injection
- tenant context header if needed in future
- retry/refresh logic
- file upload support
- pagination helpers
- error normalization

## 11.2 Example domain client modules
- `authClient`
- `identityClient`
- `billingClient`
- `travelClient`
- `communicationClient`
- `webhookClient`

## 11.3 Error normalization contract
Normalize backend responses to:
- validation error
- unauthorized
- forbidden
- not found
- domain conflict / invalid transition
- server error

This is especially important because workflow-rich aggregates can throw domain exceptions for invalid state transitions.

---

## 12. Admin portal detailed module design

## 12.1 Dashboard

### Widgets
- active quotations
- accepted quotations this period
- upcoming follow-ups
- active bookings
- unpaid / overdue invoices
- unread notifications count
- webhook failures count

### Charts
- quotation conversion funnel
- bookings by destination
- invoices by status
- follow-up completion trend

### Recommended layout
- top KPI row
- middle charts row
- bottom action queue / recent activity row

## 12.2 Contacts module

### Views
- table with pagination
- quick search
- tag filters
- detail side panel

### Schema-driven fields
- firstName
- lastName
- email
- phone
- company
- notes
- tags
- createdAt

## 12.3 Quotations module

### Key screens
- quotation list
- quotation detail header with status badge
- revisions tab
- attachments tab
- timeline tab
- send dialog
- customer preview dialog

### Important UX patterns
- draft quotation editing
- revision locking mental model
- visible notes vs internal notes separation
- attachment visibility toggle (`isCustomerVisible`)
- validity window and expiration warning

### Core fields surfaced from schema
- customerName
- destination
- travelDate / returnDate
- travellers
- currency
- validUntil
- status
- currentRevisionNumber
- acceptedRevisionId
- shareToken / shareTokenExpiresAt
- totalAmount

## 12.4 Bookings module

### Detail page tabs
- Summary
- Travelers
- Booking Items
- Documents
- Timeline
- Financials

### Important UX requirements
- display progression from quotation to booking
- separate operational status from item-level status
- support multiple travelers cleanly
- support customer-visible document filtering

### Core schema fields
- bookingNumber
- tripName
- destination
- startDate / endDate
- travellersCount
- totalSellAmount
- totalCostAmount
- marginAmount
- assignedToUserId
- status

## 12.5 Billing module

### Screens
- billing dashboard
- subscription card
- invoices list
- invoice detail

### Important fields
- planType
- billingCycle
- nextBillingDate
- invoice totals
- dueDate
- paidAt
- status

## 12.6 Communication module

### Screens
- notifications list by recipient
- template list/editor
- recipient preferences editor

### Important fields
- channel
- subject
- body
- priority
- status
- retryCount
- sentAt / deliveredAt / readAt
- timezone
- quiet hours per channel

## 12.7 Webhook module

### Screens
- subscriptions list
- new subscription form
- delivery logs table
- delivery detail and replay action

### Important fields
- targetUrl
- events[]
- isActive
- attemptCount
- responseStatusCode
- responseBody
- status

---

## 13. Customer portal detailed module design

## 13.1 Home dashboard
A concise traveler dashboard should show:
- upcoming trip summary
- latest quotation requiring action
- latest invoice/payment status
- unread notifications
- quick links to documents and itinerary

## 13.2 Quotations

### Customer views
- quotation summary
- detailed revision breakdown
- attachments/media gallery
- expiry countdown
- accept/reject CTA

### Public quotation route
A polished route based on token:
- hero summary
- trip details
- line item breakdown
- attachment gallery
- trust indicators
- accept/reject flow if later enabled publicly

## 13.3 Trips and itinerary

### Key screens
- trip list
- trip detail
- day-wise itinerary view
- downloadable or visible booking documents

### UX style
- mobile-first timeline cards
- offline-friendly itinerary caching
- clear local-time rendering

## 13.4 Documents
- tickets
- vouchers
- confirmations
- passport/visa support if customer-visible
- grouped by trip and traveler

## 13.5 Billing
- invoice list
- invoice detail
- status badges
- due reminder UX
- future payment integration entry points

## 13.6 Notifications and preferences
- inbox/notification center
- mark as read
- unread count
- channel preferences
- quiet hours
- timezone

---

## 14. Mobile app product design alignment

The mobile app should not replicate every admin feature.
It should focus on high-frequency traveler use cases:
- trip overview
- itinerary access
- travel documents
- notifications
- quotation review
- preferences
- billing snapshots

Avoid heavy CRM or tenant administration in mobile.

---

## 15. Scalability recommendations

## 15.1 Frontend scalability
- monorepo with shared packages
- feature modules by bounded context
- shared query keys and API client
- reusable domain mappers
- Storybook-driven UI system
- design tokens reused across web and mobile

## 15.2 Team scalability
- separate ownership by domain package if needed
- one team can own admin app, another customer app, while sharing `packages/*`
- generated API types can be introduced later if OpenAPI specs are published

## 15.3 Performance scalability
- SSR/streaming for public quotation pages and SEO-sensitive pages
- client rendering for authenticated dashboards
- route-level code splitting
- virtualization for large tables
- image/document lazy loading
- optimistic list updates for minor mutations

---

## 16. Suggested design system primitives

Create reusable primitives for:
- `StatusBadge`
- `MetricCard`
- `FilterBar`
- `DataTable`
- `TimelineList`
- `ActivityFeed`
- `EmptyState`
- `SectionHeader`
- `DetailsDrawer`
- `FileUploader`
- `CurrencyAmount`
- `DateRangeDisplay`
- `EntityHeader`
- `ConfirmActionDialog`
- `EntityTabs`

These components will be heavily reused across quotations, bookings, invoices, notifications, and webhooks.

---

## 17. Accessibility and UX quality bar

All frontend surfaces should include:
- keyboard-accessible tables/forms/dialogs
- WCAG-compliant contrast
- status text not represented by color alone
- proper loading, empty, and error states
- file upload progress visibility
- timezone-aware date formatting
- mobile-safe touch targets

---

## 18. Recommended implementation approach

### Build order
1. Shared frontend foundation
2. Admin portal core auth and layout
3. Travel CRM modules
4. Billing and communication modules
5. Webhook/integration views
6. Customer web portal
7. Customer mobile app

This order matches current backend maturity and business value.

---

## 19. Risks and design considerations

### 19.1 Route consistency
There are some route naming quirks between gateway prefixes and controller-level route attributes. Centralize path mapping in the API client.

### 19.2 Customer identity model
Customer/traveler authentication is not yet as explicit as internal tenant-user auth. The customer portal should begin with token/public experiences and evolve toward customer auth.

### 19.3 File delivery strategy
Travel attachments and booking documents require a clear frontend-safe file delivery pattern, especially for secure mobile/document previews.

### 19.4 Role/permission granularity
Backend currently exposes role-driven auth, but fine-grained UI permissions may need to be formalized later.

---

## 20. Final recommendation

### Best-fit solution
Implement a **shared frontend monorepo** with:
- **Next.js admin portal**
- **Next.js customer portal**
- **Expo React Native mobile app**
- shared packages for UI, types, auth, tokens, and API clients

### Why this is the right architecture
- matches the microservice backend well
- scales across multiple frontend products
- keeps admin and customer experiences separate but consistent
- supports fast delivery now and extensibility later
- aligns with the backend schema and workflow state model already present in the repo

---

## 21. Appendix: backend routes that should drive frontend modules

### Identity
- `POST /api/auth/register`
- `POST /api/auth/login`
- `POST /api/auth/refresh`
- `POST /api/auth/logout`
- `GET /api/identity/tenants/{id}`
- `PATCH /api/identity/tenants/{id}/plan`
- `POST /api/identity/tenants/{id}/suspend`
- `GET /api/identity/identity/users`
- `POST /api/identity/identity/users`
- `GET /api/identity/identity/users/{userId}`
- `PUT /api/identity/identity/users/{userId}`
- `DELETE /api/identity/identity/users/{userId}`

### Billing
- `GET /api/billing/billing/dashboard`
- `GET /api/billing/billing/subscriptions`
- `POST /api/billing/billing/subscriptions`
- `DELETE /api/billing/billing/subscriptions/{id}`
- `GET /api/billing/billing/invoices`
- `GET /api/billing/billing/invoices/{id}`
- `POST /api/billing/billing/invoices/generate`
- `POST /api/billing/billing/invoices/{id}/pay`

### Travel
- contacts, quotations, revisions, attachments, public quotation views, itineraries, follow-ups, bookings, travelers, booking items, documents, timeline

### Communication
- notifications
- templates
- recipient preferences

### Webhooks
- subscriptions
- deliveries
- replay
