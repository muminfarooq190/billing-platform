# Write / Read Consistency Rules for Voyara

_Last updated: 2026-04-17_

This document defines how Voyara should maintain consistency between write models and read models.

Blunt rule:
read models are allowed to be optimized.
They are **not** allowed to invent business truth.

---

# 1. Why this document exists

Voyara uses a split where:
- write side is mostly EF Core + aggregates + command handlers
- read side is mostly Dapper + read models + list/detail queries

That split is useful, but it creates a risk:
if read models drift away from write-model truth, the system starts lying.

This document is the guardrail.

---

# 2. Core principle

## Write model owns business truth
The write model decides:
- whether an action is valid
- what the new canonical state is
- which records must change together

## Read model owns query shape, not business truth
The read model decides:
- how data is listed
- how data is filtered
- how data is combined for UI/reporting

It must not silently redefine lifecycle meaning.

---

# 3. Ownership rules

Every important business concept must have one clear canonical owner.

## Travel examples
- inquiry status -> `TravelInquiry`
- quotation commercial state -> `Quotation`
- accepted revision -> `Quotation.AcceptedRevisionId`
- booking operational state -> `Booking`
- confirmed itinerary ownership -> `Itinerary.BookingId`
- invoice/payment truth -> billing-service

If two models both appear to own the same truth, redesign it.

---

# 4. Same-service consistency rule

Inside a single service/database:
- command handlers should update canonical aggregate state + required related records in one unit of work
- `SaveChangesAsync()` should commit the whole state transition before returning success
- read models should query the same committed canonical tables

This provides strong-enough consistency for operational UI after writes.

---

# 5. Cross-service consistency rule

Across services, consistency is eventual.

Examples:
- booking lives in travel-service
- payment truth lives in billing-service
- travel-side financial summary is a derived view, not the financial source of truth

Rules:
- use outbox/events/API composition for cross-service visibility
- do not pretend these integrated views are strongly consistent if they are not
- UI should tolerate short projection lag where needed

---

# 6. Command design rules

A command should write all data required for a coherent post-commit read.

## Example: inquiry -> quotation conversion
One command should:
- create or link contact
- create quotation
- update inquiry status
- persist conversion foreign keys
- insert inquiry status history
- write activity/audit records
- commit once

Do not spread one business transition across loosely related partial writes unless there is a deliberate asynchronous design.

---

# 7. History and audit are part of consistency

The following are not optional fluff:
- status history tables
- activity entries
- audit logs

They preserve how state changed, not just what state is now.

This matters because a read model that only shows the final state without trustworthy transition history becomes hard to debug and hard to trust.

---

# 8. Denormalization rules

Read models may denormalize for speed and usability.
That is fine.

But denormalized values must be:
- derived from canonical persisted truth
- limited to values that are safe to copy/project
- documented when they are derived rather than owned

Avoid duplicating mutable business truth across many tables unless there is a strong reason.

More duplication = more drift risk.

---

# 9. Immediate UI consistency rule

For operational screens that users expect to update immediately after a command:
- prefer querying canonical write tables directly through Dapper read models
- avoid depending on async projections for the first post-write read

Examples:
- inquiry detail after assign/qualify/convert
- booking detail after create-from-quotation
- itinerary detail after booking-owned itinerary creation

---

# 10. Eventual consistency rule for non-critical reads

For dashboards, analytics, external integrations, and cross-service summaries:
- eventual consistency is acceptable
- stale-by-seconds is often fine
- but derived views must still point back to canonical ownership

Examples:
- booking financial summary
- webhook delivery dashboards
- aggregate KPI reporting

---

# 11. Anti-patterns to avoid

## 11.1 Dual ownership
Do not let two models both appear authoritative for the same business truth.

## 11.2 Read-side business hallucination
Do not let list/detail queries infer lifecycle meaning that the write model does not actually own.

## 11.3 Partial transition writes
Do not complete half a business transition and leave the rest for some unrelated sync path unless explicitly designed as async.

## 11.4 Silent legacy ambiguity
If old and new lifecycle paths coexist, read models must clearly distinguish them.
They must not flatten them into one misleading “same thing.”

---

# 12. Specific rule for itinerary ownership

Voyara currently has legacy quote-linked itinerary behavior and newer booking-owned confirmed itinerary behavior.

Canonical rule:
- confirmed itinerary belongs to booking lifecycle
- `Itinerary.BookingId` is the operational ownership signal
- quote-linked itinerary paths are compatibility/legacy behavior, not the preferred operational truth

Read-model implication:
- booking-linked itineraries should be preferred in operational queries
- legacy quote-linked itineraries should not be mistaken for confirmed booking-stage trip truth

---

# 13. Specific rule for inquiry lifecycle

Canonical rule:
- inquiry is the top-of-funnel write model
- contact/quotation are downstream artifacts

Read-model implication:
- inquiry status, assignment, conversion refs, and history should come from inquiry-owned tables
- do not infer inquiry truth only from contact or quotation existence

---

# 14. Specific rule for billing integration

Canonical rule:
- billing-service owns invoice/payment/refund truth

Travel read-model implication:
- booking financial summary is a derived integration read
- it should be documented and treated as a projection/view, not financial source of truth

---

# 15. Practical engineering checklist

Before adding or changing a read model, ask:

1. What write model owns this truth?
2. Am I reading canonical persisted state or inventing logic in the query?
3. Does this need immediate post-write accuracy or is eventual consistency okay?
4. Am I duplicating mutable data unnecessarily?
5. If old/new workflows coexist, does the query distinguish them clearly?

If the answers are muddy, stop and redesign before shipping.

---

# 16. Final blunt conclusion

Voyara can absolutely use EF for writes and Dapper for reads without becoming inconsistent chaos.
But that only works if:
- ownership is explicit
- commands commit complete state transitions
- read models stay downstream of canonical truth
- cross-service views are treated honestly as eventual/derived

That is the contract.
