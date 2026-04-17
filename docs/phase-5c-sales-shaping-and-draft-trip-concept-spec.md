# Phase 5C Implementation Spec - Sales Shaping and Draft Trip Concept

_Last updated: 2026-04-17_

This document defines the missing pre-quotation shaping layer in Voyara.

Blunt summary:
Inquiry intake is now in place.
Quotation, booking, itinerary, and fulfillment foundations also exist.
What still needs a clearer product shape is the fuzzy middle zone between:
- inquiry received
- quotation sent

That zone is where sales people think, compare, shape, and refine the trip idea before putting a commercial offer in front of the customer.

If that layer is missing, teams fall back to:
- messy notes
- WhatsApp memory
- random docs
- quote notes abused as planning scratchpad

That is weak.

This phase exists to fix that without confusing draft trip concepts with confirmed booking-stage itineraries.

---

# 1. Goal

Add a first-class but lightweight **sales shaping layer** for travel inquiries.

This phase should let teams:
- capture pre-quotation discovery notes
- track customer preferences and sales context
- sketch rough trip concepts before pricing
- preserve alternative ideas/options
- keep these artifacts clearly separated from confirmed booking itineraries

---

# 2. Canonical lifecycle position

This phase belongs here:

1. Inquiry
2. Sales notes / draft trip concept
3. Quotation
4. Quotation revisions
5. Customer acceptance
6. Booking creation
7. Detailed itinerary / confirmed trip plan
8. Supplier fulfillment
9. Payment tracking
10. Travel docs / vouchers
11. Trip completion / changes / cancellations

Important rule:
**draft trip concept is sales-stage planning, not booking-stage operational truth.**

---

# 3. Product decision

## Recommendation
Implement this as a **lightweight inquiry-linked concept layer**, not a giant itinerary clone.

That means:
- inquiry notes remain supported
- add a structured draft trip concept entity for clearer planning
- do not overload booking itinerary with pre-sales meaning
- do not overload quotation notes with discovery clutter

---

# 4. Scope

Phase 5C covers:
1. inquiry-linked sales notes improvements
2. draft trip concept aggregate
3. concept option list/read APIs
4. concept status + primary selection model
5. concept -> quotation seeding support
6. timeline/audit integration

Phase 5C does **not** cover:
- AI itinerary generation
- supplier pricing engine
- full package builder
- auto-conversion to booking itinerary
- customer-facing concept sharing UX

---

# 5. Proposed domain model

## 5.1 New aggregate: `DraftTripConcept`

Purpose:
store structured pre-quotation travel plan ideas linked to an inquiry.

### Table: `draft_trip_concepts`

## Columns
- `id` UUID PK
- `tenant_id` UUID
- `travel_inquiry_id` UUID FK
- `title` TEXT
- `destination` TEXT
- `summary` TEXT NULL
- `start_date` TIMESTAMPTZ NULL
- `end_date` TIMESTAMPTZ NULL
- `travellers` INT NULL
- `currency` TEXT NULL
- `budget_amount` NUMERIC(18,2) NULL
- `concept_status` TEXT
- `is_primary` BOOLEAN
- `option_label` TEXT NULL
- `notes` TEXT NULL
- `created_by_user_id` UUID NULL
- `created_at` TIMESTAMPTZ
- `updated_at` TIMESTAMPTZ
- `deleted_at` TIMESTAMPTZ NULL

## Suggested statuses
- `Draft`
- `ReadyForQuote`
- `Archived`

## Notes
This is intentionally lighter than a full itinerary.
It is a sales concept, not an execution object.

---

## 5.2 New entity: `DraftTripConceptDay`

Purpose:
allow rough day-wise concepting without claiming operational precision.

### Table: `draft_trip_concept_days`

## Columns
- `id` UUID PK
- `draft_trip_concept_id` UUID FK
- `day_number` INT
- `title` TEXT
- `description` TEXT NULL
- `location` TEXT NULL
- `overnight_location` TEXT NULL
- `created_at` TIMESTAMPTZ

## Meaning
This is rough plan structure like:
- Day 1 arrival + transfer
- Day 2 city tour
- Day 3 leisure

Not supplier-confirmed segments.

---

## 5.3 Inquiry note categorization improvement

If existing generic notes can support typed categories, add categories such as:
- `SalesDiscovery`
- `CustomerPreference`
- `BudgetRisk`
- `TripConcept`
- `InternalSalesNote`

If generic notes cannot support categories cleanly yet, keep it simple and defer the category layer.

---

# 6. Core behavior rules

## Rule 1
An inquiry can have multiple draft trip concepts.

## Rule 2
One concept may be marked `is_primary = true`.

## Rule 3
Draft trip concepts do not represent confirmed itinerary truth.

## Rule 4
Draft trip concepts may seed quotation creation but must not be mistaken for booking itinerary.

## Rule 5
When a concept is archived, it remains historically visible but not active.

---

# 7. API design

## 7.1 Inquiry concept APIs

### `POST /travel/inquiries/{id}/concepts`
Create draft trip concept.

#### Example request
```json
{
  "title": "Bali Honeymoon Option A",
  "destination": "Bali",
  "summary": "Romantic beach + Ubud split with private transfers",
  "startDate": "2026-06-10T00:00:00Z",
  "endDate": "2026-06-16T00:00:00Z",
  "travellers": 2,
  "currency": "INR",
  "budgetAmount": 150000,
  "optionLabel": "Option A",
  "notes": "Target premium honeymoon positioning",
  "days": [
    {
      "dayNumber": 1,
      "title": "Arrival in Bali",
      "description": "Airport pickup and beachfront resort check-in",
      "location": "Bali",
      "overnightLocation": "Seminyak"
    }
  ]
}
```

### `GET /travel/inquiries/{id}/concepts`
List draft trip concepts for inquiry.

### `GET /travel/inquiries/{id}/concepts/{conceptId}`
Get concept detail.

### `PUT /travel/inquiries/{id}/concepts/{conceptId}`
Update concept.

### `POST /travel/inquiries/{id}/concepts/{conceptId}/mark-primary`
Mark one concept as primary.

### `POST /travel/inquiries/{id}/concepts/{conceptId}/archive`
Archive concept.

---

## 7.2 Quotation seeding

### `POST /travel/inquiries/{id}/concepts/{conceptId}/convert-to-quotation`
Optional convenience endpoint.

Alternative:
allow existing `convert-to-quotation` endpoint to take a `conceptId`.

### Recommended approach
Avoid duplicating conversion logic.
Prefer extending inquiry conversion with optional concept linkage:

`POST /travel/inquiries/{id}/convert-to-quotation`
```json
{
  "contactId": null,
  "quotationTitle": "Bali Honeymoon",
  "currency": "INR",
  "notes": "Created from primary concept",
  "assignedToUserId": null,
  "createContactIfMissing": true,
  "conceptId": "uuid"
}
```

### Behavior
- concept remains inquiry-stage artifact
- quotation gets seeded from concept summary/dates/destination/travellers
- quotation notes may mention source concept
- concept is not converted into itinerary automatically

---

# 8. Read model needs

## `DraftTripConceptListItemReadModel`
- `Id`
- `TravelInquiryId`
- `Title`
- `Destination`
- `ConceptStatus`
- `IsPrimary`
- `OptionLabel`
- `StartDate`
- `EndDate`
- `Travellers`
- `BudgetAmount`
- `Currency`
- `UpdatedAt`

## `DraftTripConceptDetailReadModel`
- all list fields
- `Summary`
- `Notes`
- concept days

---

# 9. Timeline / audit integration

Record these actions:
- concept created
- concept updated
- concept marked primary
- concept archived
- quotation seeded from concept

This should appear in inquiry timeline/history where appropriate.

---

# 10. UI / workflow expectations

A sales user should be able to:
- open inquiry
- add notes and customer preferences
- create Option A / Option B concepts
- mark one concept primary
- create quotation from the chosen concept

This should feel like pre-sales shaping, not operations.

---

# 11. Why not just use itinerary?

Because itinerary now has a clearer canonical role:
- confirmed itinerary belongs to booking lifecycle

If you reuse itinerary for sales-stage rough planning, you blur ownership again and reintroduce the same architecture confusion we just cleaned up.

That would be dumb.

---

# 12. Implementation sequence

## PR 5C.1
Draft trip concept aggregate + tables + repositories

## PR 5C.2
Create/list/get/update/archive/mark-primary APIs

## PR 5C.3
Quotation seeding from concept

## PR 5C.4
Timeline/audit/read model/docs/postman polish

---

# 13. Definition of done

Phase 5C is done when:
- inquiries support structured pre-quote shaping
- multiple rough concepts can exist per inquiry
- one concept can be primary
- quotation can be seeded from concept
- concepts are clearly distinct from booking itinerary
- timeline/audit shows concept lifecycle
- docs/tests/builds pass

---

# 14. Final blunt conclusion

Inquiry intake is now in place.
Booking-owned itinerary direction is also in place.

The next missing layer is not operations.
It is **sales-stage travel thinking**.

Phase 5C should give Voyara a place for:
- rough travel ideas
- optioning
- customer preference shaping
- pre-quote structure

without pretending those ideas are already operational truth.
