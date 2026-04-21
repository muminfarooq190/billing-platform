# Travel CRM Frontend/Backend Alignment Spec

_Last updated: 2026-04-21_

This document defines the **production-ready MVP alignment contract** between:
- the Voyara backend implemented in `billing-platform`
- the current frontend prototype at `C:\Users\uzayr\source\repos\voyara-admin-prototype-real`
- the future real frontend application that will replace or absorb the prototype

Blunt rule:
we do **not** want a pretty frontend that teaches the wrong workflow.
We also do **not** want backend capability with no coherent product surface.
This spec exists so both sides converge on one product truth.

---

## 1. What product we are building

Voyara is a **multi-tenant travel CRM / sales / operations / billing platform** for businesses like:
- travel agencies
- tour operators
- DMCs
- hotels/resort groups with trip-selling operations
- destination specialists
- concierge/luxury travel teams

The tenant-facing frontend is **not** a platform super-admin console.
It is the day-to-day business workspace used by tenant admins and their team members.

That means the frontend must support:
- tenant branding and theming
- tenant-specific modules and feature visibility
- role/permission-aware actions
- subscription-aware feature access
- user-assigned feature access within a tenant
- workflow-first travel case operations

---

## 2. Current repo reality

### Backend
Backend source of truth currently lives in:
- `C:\Users\uzayr\source\repos\billing-platform`

Important backend docs already defining truth include:
- `docs/travel-crm-canonical-lifecycle-and-entity-ownership.md`
- `docs/write-read-consistency-rules-for-voyara.md`
- `docs/feature-based-authorization-and-entitlement-audit.md`
- `docs/implementation_spec_tenant_branding_and_theming.md`
- `docs/implementation_spec_tenant_scoped_user_feature_assignments.md`
- `docs/implementation_spec_subscription_entitlements.md`
- `docs/phase-5b-inquiry-and-lead-intake-implementation-spec.md`
- `docs/phase-5c-sales-shaping-and-draft-trip-concept-spec.md`
- `docs/phase-7-booking-and-fulfillment-implementation-spec.md`

### Frontend prototype
Current prototype lives in:
- `C:\Users\uzayr\source\repos\voyara-admin-prototype-real`

Current prototype is:
- visually useful
- workflow-improving versus older drafts
- still plain HTML/CSS/JS
- still partly teaching outdated flow in some places
- not yet the production app

### Key frontend/backend mismatch today
The prototype still contains remnants of the old story:
- quote -> itinerary -> booking

But backend truth is now:
- inquiry -> concept -> quotation -> accepted revision -> booking -> confirmed itinerary -> fulfillment -> payment visibility -> documents -> completion/change/cancellation

That mismatch must die.

---

## 3. Canonical product workflow

The real product workflow for both frontend and backend is:

1. Inquiry
2. Sales shaping / draft concept
3. Quotation
4. Quotation revisions
5. Customer acceptance
6. Booking creation
7. Confirmed itinerary / trip plan
8. Supplier fulfillment
9. Payment visibility
10. Documents / vouchers
11. Completion / changes / cancellations

### Non-negotiable rules
1. **Inquiry is the top-of-funnel root.**
2. **Draft concept is pre-sales shaping, not confirmed itinerary.**
3. **Quotation is commercial truth, not operational truth.**
4. **Accepted quotation revision creates booking.**
5. **Booking becomes the operational root after acceptance.**
6. **Confirmed itinerary belongs to booking lifecycle.**
7. **Billing/payment truth remains billing-owned, but frontend must project it in booking/case context.**
8. **Documents and vouchers are post-booking lifecycle artifacts.**

If frontend design, route names, page copy, or CTAs violate these rules, frontend is wrong.

---

## 4. Product surface definition

The real frontend should be the **tenant business operating console**.

### Primary user types
1. **Tenant Admin**
   - manages workspace
   - manages branding
   - manages users and feature allocation
   - sees subscription and billing visibility
   - configures templates/integrations/preferences

2. **Sales / Consultant User**
   - manages inquiries, concepts, quotations, follow-ups, case progression

3. **Operations User**
   - manages bookings, fulfillment, docs, readiness, traveler coordination

4. **Finance-aware User**
   - sees invoice/payment status and overdue warnings based on permissions/features

5. **Limited / assigned user**
   - only sees and uses features purchased by tenant and assigned by tenant admin

### UX principle
The product should feel like:
- a tenant travel operating system
not
- a random bundle of admin pages

That means list pages are important, but the center of gravity should become a **travel case workspace** with lifecycle guidance and next actions.

---

## 5. Decision: where the frontend project should live

We need to decide whether to:
1. keep frontend prototype separate forever
2. move the prototype into backend repo
3. create a real frontend app in its own dedicated repo
4. create a frontend app inside a monorepo rooted at `billing-platform`

## Recommended decision
**Create the real production frontend as its own dedicated repo:**
- recommended repo name: `voyara-portal` or `voyara-frontend`

Keep `voyara-admin-prototype-real` as:
- prototype/design reference
- UX audit source
- temporary visual guide

Do **not** treat the prototype repo as the final production frontend codebase.

### Why this is the right call
The current prototype is useful for:
- workflow discovery
- page inventory
- stakeholder demos
- visual reference

But it is not a strong long-term production foundation because:
- plain HTML/CSS/JS will become brittle for real auth, routing, state, entitlement-aware navigation, and feature composition
- many pages will need restructuring around backend workflow truth anyway
- production-grade theming, role-aware routing, query caching, forms, and shared page modules are much easier in a modern app architecture

### What should happen practically
- keep `voyara-admin-prototype-real` unchanged as a design/prototype repo for now
- create a new repo for the real app
- systematically port prototype patterns/screens into the real app only after they are aligned to backend truth

### Alternative acceptable option
If operationally you strongly prefer a single repo, the second-best option is:
- add a `/frontend` app inside `billing-platform`
- effectively treat `billing-platform` as a monorepo

That is acceptable if you want tighter co-evolution and shared CI.

### Not recommended
- trying to evolve `voyara-admin-prototype-real` directly into the long-term production app with no architecture reset

That will get messy fast.

---

## 6. Recommended frontend tech stack

## Recommended stack for production-ready MVP

### Core
- **React**
- **TypeScript**
- **Next.js** (App Router) or **Vite + React Router**

## Recommendation between those two
For this product, I recommend:
- **Next.js + TypeScript**

### Why Next.js is the better fit
Even if the initial app is mostly authenticated dashboard UI, Next gives us:
- strong routing conventions
- layout nesting
- good long-term app structure
- server components / hybrid rendering options when useful
- easier docs/marketing/admin shell expansion later
- strong ecosystem and production maturity

This is not because we need marketing-site hype.
It is because the product will likely benefit from:
- authenticated layouts
- nested route groups
- dynamic tenant/brand-aware shells
- optional SSR/edge handling later
- serious long-term maintainability

### Styling/UI
- **Tailwind CSS** for implementation speed and design token discipline
- **CSS variables** for tenant theme tokens
- **shadcn/ui** or a lightweight headless component approach for composable primitives
- selective use of **Radix UI** primitives where accessibility/state complexity matters

### Data/state
- **TanStack Query** for server state
- **React Hook Form** for forms
- **Zod** for schema validation and parsing

### Charts / data viz
- **Recharts** for MVP dashboards
  - practical
  - fast to ship
  - enough for KPI and business charts

### Tables / dense data views
- **TanStack Table**

### Icons / polish
- **Lucide** icons

### Auth/session
Depends on backend auth strategy, but frontend must support:
- tenant-aware auth session
- permission claims
- tenant entitlement/feature payload
- user assignment payload or derived capability payload

### Why not keep pure HTML/CSS/JS for the real app
Because we need all of this in a durable way:
- route guarding
- entitlement-aware navigation
- permission-aware actions
- reusable case workspace composition
- design system tokens
- complex forms
- query invalidation and caching
- module-level lazy loading
- robust shared layouts

Plain files can prototype this.
They are a bad long-term home for it.

---

## 7. Frontend architecture principles

## 7.1 Frontend must be capability-aware
Frontend should render based on a layered access model:
- permission layer
- tenant entitlement layer
- user assignment layer

### Frontend rule
A user can only use a feature if:
1. user permission allows the action
2. tenant subscription/package/override includes the feature
3. user is assigned the feature where assignment is required

Expressed simply:

```text
visible/usable = permission && tenantFeatureEnabled && userFeatureAssignedIfApplicable
```

### Important product distinction
- **Permissions** answer: can this user perform this action?
- **Entitlements** answer: has this tenant purchased/unlocked this capability?
- **Assignments** answer: within this tenant, has the admin allocated this capability to this user?

Frontend must not collapse these into one muddy boolean.

---

## 7.2 Frontend must be tenant-branded by design, not by afterthought
Branding/theming is not cosmetic fluff.
It is a product feature.

Each tenant should be able to define or consume:
- tenant name
- logo
- brand colors
- typography pairing or allowed font family set
- preferred dashboard hero treatment
- light/dark defaults
- email/document template theme linkage where applicable

### Frontend implementation rule
All major visual design should hang off a **theme token contract**, not hardcoded colors.

Example conceptual token groups:
- `--color-primary`
- `--color-secondary`
- `--color-accent`
- `--color-surface`
- `--color-surface-alt`
- `--color-text`
- `--color-text-muted`
- `--font-display`
- `--font-body`
- `--radius-card`
- `--shadow-elevation-1`

### Important constraint
Allow theming freedom without allowing tenants to create unreadable clown UIs.
So MVP should support:
- curated token ranges
- validated brand presets or constrained custom values
- theme preview before publish

---

## 7.3 Frontend must be workflow-first
The app should not just be a sidebar full of unrelated pages.

The frontend should expose two complementary modes:

### Mode A: Queue/list views
Used to answer:
- what needs attention?
- what is overdue?
- what should I work on next?

Examples:
- inquiries needing qualification
- quotations expiring soon
- bookings blocked by missing traveler docs
- failed communications
- overdue invoices or payment alerts

### Mode B: Case/workspace views
Used to answer:
- where is this travel case now?
- what happened before?
- what happens next?
- what is blocked?
- who owns it?

The future real frontend must prioritize **case/workspace views** much more strongly than the prototype currently does.

---

## 8. MVP route and module inventory

## 8.1 Auth / session
- Login
- Forgot password
- Reset password
- Optional workspace/tenant selection if required by auth flow

## 8.2 Main tenant app shell
- Dashboard
- Inquiries
- Travel Cases / Workspace
- Contacts
- Quotations
- Bookings
- Itineraries (booking-linked confirmed itinerary view, not quote-rooted)
- Follow-Ups
- Communications
- Billing
- Team / Users
- Settings
- Integrations / Webhooks

## 8.3 Settings sections
- Tenant Profile
- Branding
- Subscription & Plan
- Feature Access / Allocations
- Roles & Access
- Notification Preferences
- Template Center
- API / Webhooks
- Audit / Activity where appropriate

---

## 9. Production-ready MVP feature contract

This section defines what must be covered so the frontend and backend align on an MVP that is serious, not fake-complete.

## 9.1 Tenant onboarding / workspace identity
MVP should support:
- tenant identity display
- tenant profile settings
- tenant brand preview and save
- subscription visibility
- current plan visibility
- enabled feature visibility

## 9.2 Inquiry-first demand capture
MVP should support:
- inquiry list/queue
- inquiry statuses
- assignment
- qualification/disqualification
- inquiry detail
- notes/activity
- conversion path into concept/quotation flow

## 9.3 Sales shaping / draft concept
MVP should support:
- concept creation under inquiry
- multiple concepts/options where relevant
- concept summary and rough structure
- mark primary concept
- create quotation from concept

## 9.4 Quotation workflow
MVP should support:
- quotation list and detail
- status pipeline
- revision history
- accepted revision visibility
- send/share state
- expiration visibility
- conversion into booking only after accepted revision

## 9.5 Booking / operations workflow
MVP should support:
- booking list and detail
- booking creation from accepted quotation
- operational statuses
- readiness/checklist concepts
- traveler summary
- supplier fulfillment visibility
- change/cancellation surface at least in basic form

## 9.6 Confirmed itinerary workflow
MVP should support:
- booking-linked itinerary detail
- itinerary readiness state
- daily/section structure if available
- operational/customer-facing trip structure visibility

### Critical rule
Frontend must not teach users that itinerary is the root object after quote acceptance.
Booking is.

## 9.7 Follow-ups / task execution
MVP should support:
- due today / overdue / upcoming queue
- priority and assignee filters
- task status changes
- linkage to related case/entity

## 9.8 Communications
MVP should support:
- communication/delivery visibility
- status feed
- failed delivery visibility
- template linkage where supported
- case-level communication context when relevant

## 9.9 Billing visibility
MVP should support:
- subscription/plan display
- tenant billing overview
- invoice/payment visibility
- overdue warnings
- booking/case-level financial summary projection where available

## 9.10 Team / users / access
MVP should support:
- invite/manage users
- role status visibility
- seat summary
- feature assignment/allocations UI
- visibility into what subscription enables versus what a user is assigned

## 9.11 Settings / branding / templates / integrations
MVP should support:
- branding management
- notification preferences
- template center visibility
- webhooks/integration visibility
- tenant-level operational configuration needed by portal

---

## 10. Feature entitlement and assignment rules in the UI

This is one of the most important product rules.

The user specifically wants the product to ensure:
- frontend only exposes features that are in the tenant subscription
- if tenant admin adds users, those users only get the features allowed by subscription and allocated to them

That is correct.
That should be the contract.

## 10.1 Access model
For each frontend module/capability, define:
- required permissions
- required tenant feature key(s)
- whether explicit user assignment is required

### Example matrix pattern

| Capability | Permission example | Tenant feature required | User assignment required |
|---|---|---|---|
| View inquiries | `travel.inquiries.read` | `travel.inquiries` | No |
| Manage quotations | `travel.quotations.write` | `travel.quotations` | Optional/depends |
| Use premium itinerary workspace | `travel.itineraries.write` | `travel.itinerary_advanced` | Yes |
| Access billing overview | `billing.invoices.read` | `billing.portal` | No or Yes depending on product policy |
| Manage templates | `communication.templates.manage` | `communication.templates` | Yes if seat-limited/premium |
| Manage branding | `tenant.branding.manage` | `tenant.branding` | No, admin-only permission usually enough |

The exact matrix should be finalized from backend feature keys, but the frontend must be built to support this model cleanly.

## 10.2 UI behavior rules
If tenant lacks a feature entirely:
- hide the route from primary navigation or show locked state intentionally
- prevent deep-link use without feature access
- show clear upsell/locked copy only if product policy wants that

If tenant has feature but user is not assigned:
- do not allow usage
- show restricted/assigned-by-admin messaging where useful
- keep the distinction clear from “tenant did not buy this”

If tenant has feature and user is assigned but lacks permission:
- treat as authorization failure, not subscription failure

## 10.3 Seat/assignment aware admin UX
Tenant admins should be able to see:
- which features exist in their subscription
- which are limited by seat/user assignment
- which users currently hold assignments
- which features are unassigned or over-requested

That UI matters because otherwise subscription logic becomes invisible and impossible to manage.

---

## 11. Travel case workspace contract

The real frontend should add a first-class **Travel Case Workspace**.

## 11.1 Purpose
This is the detail surface for one inquiry/customer trip journey over time.

## 11.2 Required sections
### Header
- traveler/inquiry name
- destination
- travel dates
- source
- owner/assignee
- priority
- current stage
- blockers

### Lifecycle rail
- Inquiry
- Concept
- Quotation
- Accepted
- Booking
- Itinerary
- Fulfillment
- Payment
- Documents
- Completed / Changed / Cancelled

### Main stage panel
Shows active-stage details and actions.

### Activity timeline
Shows:
- status changes
- assignments
- revisions
- communication events
- major actions

### Side rail
Shows:
- next recommended actions
- overdue follow-ups
- warnings/blockers
- linked docs/files
- communication/payment readiness signals

## 11.3 Why this matters
Without this, the app remains just a decent admin shell.
With this, it becomes the actual tenant travel operating system.

---

## 12. Frontend information architecture corrections required from current prototype

The current prototype is useful but must be treated critically.

## Corrections that must happen before production porting
1. Add **Inquiries** as first-class workflow root.
2. Add **draft concept** workflow under inquiry.
3. Stop teaching **quote -> itinerary -> booking**.
4. Reframe itinerary as **booking-owned confirmed trip plan**.
5. Add or evolve toward **Travel Case Workspace**.
6. Connect billing/comms/docs to lifecycle context, not just isolated pages.
7. Ensure settings include real **branding**, **subscription**, and **feature allocation** surfaces.

If a prototype page conflicts with backend truth, backend truth wins.

---

## 13. Proposed production frontend app structure

If using Next.js, recommended broad structure:

```text
app/
  (auth)/
    login/
    forgot-password/
    reset-password/
  (portal)/
    layout.tsx
    dashboard/
    inquiries/
    cases/
      [caseId]/
    contacts/
    quotations/
    bookings/
    itineraries/
    follow-ups/
    communications/
    billing/
    team/
    settings/
      profile/
      branding/
      subscription/
      features/
      roles-access/
      notifications/
      templates/
      webhooks/
components/
  layout/
  navigation/
  cases/
  dashboard/
  tables/
  forms/
  charts/
  feedback/
lib/
  api/
  auth/
  permissions/
  entitlements/
  theme/
  schemas/
  utils/
features/
  inquiries/
  quotations/
  bookings/
  itineraries/
  communications/
  billing/
  users/
  settings/
```

### Important architecture rule
Do not build one giant `components` graveyard.
Prefer domain-oriented folders plus shared primitives.

---

## 14. Recommended frontend domain contracts

Frontend should consume backend data through explicit view-model contracts.
That means we should have stable shapes for things like:
- session/me payload
- tenant branding payload
- tenant feature access payload
- user-assigned feature payload
- inquiry summary/detail view
- case workspace view
- quotation detail view
- booking detail view
- billing summary view

### Session payload should ideally expose or derive
- user id
- tenant id
- roles/permissions
- enabled feature keys
- assigned feature keys or effective feature grants
- tenant branding summary

Without this, frontend capability rendering becomes chaotic.

---

## 15. MVP quality bar

This product should be treated as a **production-ready MVP**, not a fake demo MVP.

That means the frontend must be ready for:
- real authentication
- real routing
- failure/loading states
- no-access/locked states
- empty states
- audit-friendly lifecycle language
- tenant-safe navigation and guards
- basic accessibility sanity
- responsive desktop-first behavior
- deterministic UX for stakeholder demos and real pilot use

### Not required for MVP
- every imaginable analytics report
- total workflow automation for every corner case
- pixel-perfect white-label freedom with no constraints
- giant super-admin platform console

### Required for MVP
- workflow truth
- tenant theme support
- entitlement-aware module access
- user allocation support
- inquiry -> booking lifecycle correctness
- backend-aligned page language and data ownership

---

## 16. Phased execution plan

## Phase 1 - Decide architecture and freeze product truth
- approve this spec
- create real frontend repo (`voyara-portal` or `voyara-frontend`)
- lock canonical lifecycle language
- define route map and access model
- define theme token contract

## Phase 2 - Build core shell and auth/session model
- app shell
- auth pages
- tenant-aware layout
- navigation model
- permission/entitlement/assignment guards
- theme provider

## Phase 3 - Build workflow root correctly
- inquiries
- inquiry detail
- draft concept flow
- quotation list/detail/revisions
- booking creation path from accepted quotation

## Phase 4 - Build case-centric workspace
- travel case workspace
- lifecycle rail
- stage panels
- timeline
- side rail for blockers/next actions

## Phase 5 - Supporting domains
- bookings
- itineraries
- follow-ups
- communications
- billing visibility
- users/team
- settings/branding/subscription/feature assignments

## Phase 6 - Hardening / MVP polish
- error/loading/empty states
- route guards and unauthorized states
- chart/table polish
- responsive cleanup
- production audit pass against backend docs

---

## 17. Decision summary

## Repo location decision
**Recommended:** create a new production frontend repo.

- Prototype stays in `voyara-admin-prototype-real`
- Production app lives in a proper React/TypeScript codebase
- Backend remains in `billing-platform`
- Both are aligned through shared docs/contracts

## Tech stack decision
**Recommended:**
- Next.js
- React
- TypeScript
- Tailwind CSS
- CSS variables for tenant theming
- TanStack Query
- React Hook Form
- Zod
- TanStack Table
- Recharts
- Radix/shadcn-style primitives

## Product rules decision
Frontend must enforce and visually reflect:
- permissions
- tenant subscription entitlements
- tenant-scoped user feature assignments

## Workflow decision
Frontend must rigorously follow backend lifecycle truth:
- inquiry first
- concept before quote where needed
- accepted quote creates booking
- itinerary is booking-owned confirmed trip plan

---

## 18. Final blunt conclusion

The frontend should not be built as:
- a generic admin template
- a loose port of the current prototype
- a subscription-blind UI
- a permission-only UI
- a quote-centric travel workflow fantasy

It should be built as:
- a serious tenant-branded travel operating console
- aligned to backend entity ownership and lifecycle truth
- feature-aware at tenant and user-assignment level
- production-ready enough to support real admins and real teams

The correct next move is:
1. approve this alignment direction
2. create the real frontend repo
3. define exact access matrix per module/feature key
4. port the prototype selectively, not blindly

That is how you avoid building the wrong shiny thing.
