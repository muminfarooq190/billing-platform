# Phase 5B Implementation Spec - Inquiry and Lead Intake Foundation

_Last updated: 2026-04-17_

This document defines the missing pre-quotation workflow in Voyara.

Blunt summary:
Voyara has decent contact, quotation, booking, timeline, and fulfillment foundations.
What it still lacked was a proper first-class path for unknown website visitors / inbound travel inquiries to enter the system and become actionable sales work.

This spec now also reflects the current implementation progress on branch `feat/travel-inquiry-intake`.

---

# 1. Goal

Introduce a proper inquiry / lead intake pipeline for travel-service so Voyara supports the full path:

- inbound public inquiry captured
- inquiry reviewed internally
- inquiry qualified/disqualified
- inquiry assigned to an agent
- inquiry converted into contact + quotation draft
- quote continues into booking workflow

---

# 2. Current implementation status

## Implemented
- `TravelInquiry` aggregate
- `TravelInquiryStatusHistory`
- internal inquiry management APIs
- inquiry assignment / qualify / disqualify / contacted / archive actions
- inquiry -> contact + quotation conversion
- public inquiry intake endpoint: `POST /travel/public/inquiries`
- basic honeypot + required field validation
- initial activity/audit/history writes

## Not yet fully mature
- branded host / signed site token tenant resolution
- first-class rate limiting strategy in travel-service
- CAPTCHA integration
- richer source attribution / UTM capture
- public inquiry status lookup surface

---

# 3. Public intake design notes

## Current safe rule
Public inquiry writes must not trust `tenantId` in request body.

Current implementation resolves tenant through a separate public resolver path and currently expects:
- `x-public-tenant-id`

This is acceptable as an implementation step for controlled environments.
It should later evolve toward:
- branded hostname mapping
- signed portal/site token
- gateway-injected trusted tenant context

## Basic protection currently implemented
- honeypot field support
- required field validation
- at least one contact method required

## Protection still recommended later
- request throttling / rate limiting
- CAPTCHA / Turnstile style bot protection
- richer source/referrer/utm capture

---

# 4. Key endpoints

## Public
### `POST /travel/public/inquiries`
Create inbound inquiry.

Example body:
```json
{
  "fullName": "Jane Doe",
  "email": "jane@example.com",
  "phone": "+919999999999",
  "whatsappNumber": "+919999999999",
  "departureCity": "Mumbai",
  "destination": "Bali",
  "travelDate": "2026-06-10T00:00:00Z",
  "returnDate": "2026-06-16T00:00:00Z",
  "isDateFlexible": true,
  "travellers": 2,
  "budgetAmount": 150000,
  "budgetCurrency": "INR",
  "message": "We want a honeymoon package with private transfers.",
  "source": "Website",
  "honeypot": null
}
```

Trusted public tenant resolution header currently expected:
- `x-public-tenant-id: <tenant-guid>`

## Internal
- `GET /travel/inquiries`
- `GET /travel/inquiries/{id}`
- `GET /travel/inquiries/{id}/history`
- `POST /travel/inquiries/{id}/assign`
- `POST /travel/inquiries/{id}/qualify`
- `POST /travel/inquiries/{id}/disqualify?status=Lost|Spam`
- `POST /travel/inquiries/{id}/mark-contacted`
- `POST /travel/inquiries/{id}/archive`
- `POST /travel/inquiries/{id}/convert-to-quotation`

---

# 5. Canonical lifecycle alignment

Inquiry is now the intended top-of-funnel entry point before quotation.

Canonical path:
- inquiry
- sales shaping / notes
- quotation
- quotation revisions
- customer acceptance
- booking
- confirmed itinerary
- fulfillment
- payment visibility
- docs / vouchers

Do not bypass inquiry by pretending the CRM begins only at contact or quotation level.

---

# 6. Final blunt note

Phase 5B is no longer hypothetical.
It is now partially implemented and usable.

What remains is mostly hardening and product polish, not total invention from scratch.
