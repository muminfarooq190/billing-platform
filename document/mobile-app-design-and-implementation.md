# Mobile App Design & Implementation Document

## 1. Purpose

This document defines the recommended mobile app design and implementation approach for the **end-user facing portal** of the billing-platform / Voyara system.

The mobile app is intended for travelers/customers, not internal operations staff.

It is based on the current backend capabilities in the repository, especially:
- travel quotations
- itineraries
- bookings
- booking documents
- notifications
- recipient preferences
- billing invoice visibility
- public quotation token flows

---

## 2. Mobile app product goal

The mobile app should become the traveler’s companion app.

Its primary jobs are to help users:
- see their upcoming trips
- review quotations
- access itinerary details quickly
- retrieve travel documents on the go
- receive notifications
- manage preferences
- view invoice/payment status

It should **not** try to replicate the full admin portal.

---

## 3. Recommended mobile technology stack

Use:
- **React Native with Expo**
- **TypeScript**
- **Expo Router**
- **TanStack Query**
- **React Hook Form + Zod**
- **NativeWind** or Tamagui
- **Expo SecureStore**
- **Expo Notifications**
- **Expo FileSystem** for downloads/caching
- **Expo DocumentPicker** if upload later becomes required

## Why this stack fits
- fast cross-platform delivery
- shared TypeScript types with web frontends
- strong support for notifications and secure storage
- good reuse of API client and validation logic
- ideal for trip-centric UX with offline-friendly behavior

---

## 4. Mobile app scope

## 4.1 In scope for v1
- onboarding / sign-in shell or token-entry flow
- home dashboard
- quotations view
- trip list
- itinerary detail
- travel documents center
- notifications inbox
- preferences management
- invoice summary/list

## 4.2 Out of scope for v1
- internal CRM workflows
- contact management
- quotation editing by internal users
- booking item administration
- tenant/user administration
- webhook administration
- advanced finance operations

---

## 5. User personas

### Primary persona: traveler/customer
Needs quick access to:
- where am I going?
- what is confirmed?
- what documents do I need?
- what changed?
- what do I owe?

### Secondary persona: client decision-maker
Needs to:
- review quotations
- inspect trip breakdown
- see invoice/payment state
- access communications and documents

---

## 6. Mobile information architecture

## Bottom navigation recommendation
- Home
- Trips
- Documents
- Notifications
- Profile

## Nested screens

### Home
- overview dashboard
- next trip card
- pending quotation card
- unpaid invoice card
- unread notifications summary

### Trips
- trip list
- trip detail
- itinerary day view
- traveler details
- booking item summaries

### Documents
- all documents
- documents by trip
- documents by traveler
- preview/download

### Notifications
- inbox list
- detail view
- mark read

### Profile
- preferences
- timezone
- quiet hours
- communication channels
- billing summary
- logout

---

## 7. Backend alignment

## 7.1 Relevant current backend domains

### Travel
- `Quotation`
- `QuotationRevision`
- `QuotationAttachment`
- `QuotationShareLink`
- `Booking`
- `Traveler`
- `BookingDocument`
- `Itinerary`
- `ActivityEntry`

### Billing
- `Invoice`
- `Subscription`

### Communication
- `Notification`
- `RecipientPreferences`

## 7.2 Relevant backend routes

### Quotations
- `GET /travel/quotations/public/{token}`
- `POST /travel/quotations/public/{token}/viewed`
- internal quotation APIs for future authenticated customer access

### Bookings
- `GET /api/travel/...bookings`
- `GET /api/travel/...bookings/{id}`
- `GET /api/travel/...bookings/{id}/travelers`
- `GET /api/travel/...bookings/{id}/items`
- `GET /api/travel/...bookings/{id}/documents`

### Itinerary
- `GET /api/travel/...itineraries`
- `GET /api/travel/...itineraries/{id}`

### Notifications
- `GET /api/communication/...notifications/recipient/{recipientId}`
- `GET /api/communication/...notifications/recipient/{recipientId}/unread-count`
- `PATCH /api/communication/...notifications/{id}/read`

### Preferences
- `GET /api/communication/...recipient-preferences/{tenantId}/{recipientId}`
- `PUT /api/communication/...recipient-preferences`

### Billing
- `GET /api/billing/...invoices`
- `GET /api/billing/...invoices/{id}`

> Exact mobile API paths should be normalized via a shared API client package because gateway and controller route composition currently creates nested path quirks.

---

## 8. Core mobile UX principles

- Make the next important thing obvious.
- Optimize for one-handed use.
- Support intermittent connectivity.
- Keep critical travel info reachable in 2 taps or less.
- Prioritize readability over admin-style data density.
- Surface time, status, and documents clearly.

---

## 9. Detailed screen design

## 9.1 Home screen

### Purpose
Immediate overview of current travel state.

### Content blocks
1. Greeting + traveler name
2. Next trip card
   - destination
   - date range
   - booking/trip status
3. Pending quotation card
   - title
   - destination
   - valid until
   - CTA: review now
4. Invoice/payment card
   - total due
   - due date
   - status
5. Notifications summary
   - unread count
6. Quick actions
   - open itinerary
   - open documents
   - view latest invoice

### Design notes
- card-first layout
- strong status indicators
- compact but premium feel

## 9.2 Quotations screens

### Quotation list
- active/pending quotations
- status badge
- destination
- travel dates
- validity

### Quotation detail
- trip overview
- line item breakdown
- totals
- visible notes
- attachments
- revision information
- viewed timestamp if relevant

### Public token flow
If customer auth is not ready, support a special token-based quotation review screen inside app via deep link.

Example deep link:
- `voyara://quote/{token}`

## 9.3 Trips screen

### Trip list
- upcoming
- active
- completed
- cancelled

### Trip card fields
- trip name
- destination
- start/end dates
- traveler count
- status

### Trip detail
- summary header
- itinerary preview
- travelers
- booking items snapshot
- document shortcuts
- invoice shortcut if tied to trip context

## 9.4 Itinerary screen

### Recommended presentation
- day tabs or vertical timeline
- each item as a card
- show:
  - type
  - title
  - description
  - location
  - start/end time
  - cost if relevant

### Why this matters
Itinerary is likely the highest-value mobile feature during travel.

## 9.5 Documents screen

### Use cases
- show voucher at hotel desk
- open flight confirmation quickly
- download insurance PDF
- retrieve traveler-specific document

### Presentation
- grouped by trip
- grouped by traveler optionally
- file type icon
- customer-visible label not shown; only already-filtered allowed docs should reach app
- preview/download/share where allowed

### Important metadata to show
- document type
- traveler name if applicable
- upload/created date
- file name

## 9.6 Notifications screen

### List screen
- grouped by date
- unread emphasis
- status chip if useful
- compact preview text

### Detail screen
- subject
- body
- sent date
- related entity shortcuts if available

## 9.7 Preferences screen

### Fields from current schema
- email
- phone
- deviceToken
- timezone
- channel preferences
- quiet hours start/end

### UX
- toggle per channel
- quiet hours switch + time picker
- timezone selector

## 9.8 Billing screens

### List screen
- invoice number/id
- status
- amount
- due date
- paid date

### Detail screen
- subtotal
- tax
- total
- due date
- paid date
- status

This can remain simple in v1.

---

## 10. Mobile state management

## 10.1 Server state
Use TanStack Query for:
- trips
- itinerary
- quotations
- notifications
- unread count
- documents
- invoices
- preferences

## 10.2 Local/UI state
Use Zustand or local component state for:
- selected trip
- filters
- temporary draft form state
- bottom sheet visibility
- local sort options

## 10.3 Persistence
Persist only what improves mobile usability:
- session metadata
- selected tenant/profile if needed
- cached recent trips/documents metadata
- notification settings cache

Avoid persisting sensitive business payloads unnecessarily.

---

## 11. Authentication strategy for mobile

## 11.1 Recommended approach
- access token in memory
- refresh token in SecureStore
- biometric gate optional for opening app
- deep-link support for invitation/public quotation flows

## 11.2 If customer auth is not yet ready
Support staged rollout:
1. public quotation deep links first
2. invite-based sign-in later
3. full customer account model later

This keeps mobile delivery unblocked by identity evolution.

---

## 12. Offline and low-connectivity design

Mobile travel apps must assume poor connectivity.

## Recommended offline support
- cache latest itinerary detail
- cache recent documents metadata
- cache basic trip summaries
- queue mark-as-read actions if offline
- display offline banner and last synced time

## Do not rely on always-online UX
Trip info should remain partially accessible without network.

---

## 13. Notifications and deep linking

## 13.1 Push notifications
Use Expo Notifications for:
- trip reminders
- quotation ready/review reminders
- invoice due reminders
- booking updates
- document availability notices

## 13.2 Deep-link targets
- quotation detail
- trip detail
- itinerary day
- documents for trip
- invoice detail
- notification detail

---

## 14. Design system for mobile

Create shared design tokens with web, but mobile-specific components.

## Components to build
- `AppShell`
- `TripCard`
- `QuoteCard`
- `InvoiceCard`
- `NotificationRow`
- `DocumentRow`
- `StatusPill`
- `TimelineItemCard`
- `EmptyState`
- `OfflineBanner`
- `SectionHeader`
- `ActionSheet`
- `InfoListItem`

## Visual direction
- premium travel companion feel
- generous spacing
- subtle gradients for destination/trip surfaces
- clear hierarchy for dates and statuses
- strong readability outdoors and in transit

---

## 15. Security considerations

- use secure token storage
- avoid exposing internal-only fields in mobile DTOs
- protect document URLs
- consider signed URL or token-bound file access strategy
- sanitize attachment/document previews
- never trust cached auth blindly after refresh failure

---

## 16. Performance considerations

- lazy-load heavy screens
- paginate notifications and documents
- prefetch trip detail after home load
- cache image/document thumbnails where safe
- compress asset bundles and icons

---

## 17. Testing strategy

## Unit tests
- mappers
- hooks
- query helpers
- form validation schemas

## Integration tests
- auth flow
- quotations fetch
- notifications mark-read
- preferences save

## E2E/device tests
- login/session restore
- open trip and itinerary
- open/download document
- open push notification deep link

---

## 18. Stepwise implementation plan for mobile

## Phase M1 - Mobile foundation
1. Initialize Expo app in `frontend/apps/customer-mobile`
2. Add shared packages integration
3. Add navigation shell
4. Add theme and tokens
5. Add API client + auth plumbing

## Phase M2 - Public/customer quote experience
1. Build quote deep link handling
2. Build quotation detail screen
3. Add viewed tracking call

## Phase M3 - Trip experience MVP
1. Build home screen
2. Build trips list
3. Build trip detail
4. Build itinerary detail

## Phase M4 - Documents and notifications
1. Build documents list/detail flow
2. Add local download/open flow
3. Build notifications list
4. Add unread count and mark-read

## Phase M5 - Preferences and billing
1. Build preferences screen
2. Build invoice list/detail screens
3. Add timezone and quiet-hours controls

## Phase M6 - Hardening
1. Offline support
2. push notification integration
3. deep link polish
4. analytics and crash reporting
5. accessibility review

---

## 19. Recommended mobile MVP

If only a narrow first mobile release is desired, ship:
- quotation deep link / public quote screen
- trip list
- itinerary detail
- documents access
- notifications inbox

That gives the strongest traveler value with the least dependency on unfinished auth expansion.

---

## 20. Final recommendation

### Best-fit mobile product strategy
Build a **React Native Expo customer companion app** focused on:
- trips
- itineraries
- documents
- notifications
- quotation review
- preferences
- simple billing visibility

### Why this approach works
- aligned with current schema and APIs
- avoids trying to force admin workflows into mobile
- creates a practical traveler-first experience
- allows staged rollout even if customer auth evolves later
- reuses shared packages from the proposed frontend monorepo

This is the right mobile direction for the end-user facing side of the platform.
