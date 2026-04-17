# Travel CRM Canonical Lifecycle and Entity Ownership

_Last updated: 2026-04-17_

This document defines the **canonical workflow** for Voyara so backend implementation does not drift into contradictory or hallucinated process design.

Blunt rule:
if future implementation conflicts with this lifecycle, this document wins unless explicitly superseded.

---

# 1. Canonical lifecycle

Voyara should model the customer/trip lifecycle in this order:

1. Inquiry
2. Sales notes / optional draft itinerary concept
3. Quotation
4. Quotation revisions
5. Customer acceptance
6. Booking creation
7. Detailed itinerary / confirmed trip plan
8. Supplier fulfillment
9. Payment tracking
10. Travel docs / vouchers
11. Trip completion / changes / cancellations if needed

This order is intentional.
It separates:
- lead capture
- commercial proposal
- operational execution
- finance visibility
- post-booking lifecycle management

Do not collapse these stages into one blob.

---

# 2. Core product principles

## 2.1 Inquiry is not quotation
An inquiry is inbound demand.
It is not a priced commercial proposal.

## 2.2 Quotation is not booking
A quotation is what we are offering and for how much.
A booking is an operational commitment created after acceptance.

## 2.3 Itinerary is not the quotation itself
The quote may include a trip concept.
But the **confirmed itinerary** should represent the trip plan in operational reality.

## 2.4 Booking is the operational root after acceptance
Once the customer accepts the commercial offer, the operational truth should move under booking lifecycle.

## 2.5 Payment truth belongs to billing-service, but booking should expose financial visibility
Travel-service should not own invoice/payment truth.
It should project finance state into booking context.

---

# 3. Stage-by-stage ownership

## Stage 1 - Inquiry

### Purpose
Capture inbound travel demand from a prospect/customer.

### Owning entity
- `TravelInquiry`

### Allowed concepts
- source
- customer intent
- dates/budget/traveler count
- assignment
- qualification/disqualification
- notes

### Must not happen here
- booking creation
- supplier fulfillment
- payment state
- vouchers

### Exit from stage
A qualified inquiry can be converted into contact + quotation workflow.

---

## Stage 2 - Sales notes / optional draft itinerary concept

### Purpose
Let internal team shape the trip idea before pricing.

### Recommended ownership
For MVP:
- use inquiry notes / timeline entries

Later optional:
- lightweight pre-sales itinerary concept or planning artifact

### Important distinction
This is **not** the confirmed itinerary.
It is a draft planning aid for sales.

### Why this exists
Sales often needs rough trip structure before final pricing:
- destinations
- trip flow
- hotel/tour ideas
- route concept

### Must not happen here
- final confirmed itinerary status
- fulfillment state
- vouchers/doc issuance

---

## Stage 3 - Quotation

### Purpose
Create a formal commercial offer.

### Owning entity
- `Quotation`

### Quotation should contain
- customer linkage
- title / destination / dates / traveler count
- currency
- commercial notes
- line items / totals
- validity

### Meaning
Quotation answers:
> what are we offering, and what does it cost?

### Must not become
- final itinerary truth
- supplier execution root
- payment ledger

---

## Stage 4 - Quotation revisions

### Purpose
Safely iterate the offer.

### Owning entities
- `QuotationRevision`
- `QuotationStatusHistory`
- `QuotationAttachment`
- `QuotationShareLink`

### Meaning
This stage handles:
- customer objections
- changes in dates/destination/hotels
- pricing updates
- revised commercial snapshots

### Rule
Revision history is the commercial memory.
Do not overwrite accepted commercial state blindly.

---

## Stage 5 - Customer acceptance

### Purpose
Mark the commercial point of commitment.

### Owning concept
Accepted quotation revision

### Meaning
This is the boundary between:
- proposal
and
- operational commitment

### Required effects
- accepted revision must be frozen and traceable
- downstream booking creation must use accepted revision

### Rule
Nothing should become operationally committed without this stage unless manual exception workflows are explicitly designed.

---

## Stage 6 - Booking creation

### Purpose
Create the operational root record for the sold trip.

### Owning entity
- `Booking`

### Source of truth
Booking should be created from:
- accepted quotation revision

### Meaning
Booking answers:
> this trip is now a committed operational record

### Booking should own or anchor
- travelers
- booking items
- documents
- payment visibility
- confirmed itinerary linkage/state
- change/cancellation lifecycle

### Rule
Standard path should be:
- inquiry -> quotation -> accepted quotation -> booking

Do not let booking become a shortcut around the commercial workflow.

---

## Stage 7 - Detailed itinerary / confirmed trip plan

### Purpose
Represent the actual trip structure after commitment.

### Recommended ownership
**Confirmed itinerary should belong to booking context.**

### Important architecture decision
There may be an early draft trip concept during inquiry/sales.
That is fine.
But the confirmed itinerary should not remain just a quotation-side artifact.

### Meaning
Confirmed itinerary includes things like:
- day plan
- travel segments
- hotel sequence
- confirmed services/timings
- customer-facing trip structure

### Required rule
If itinerary reflects actual post-sale trip execution, it should be linked to booking.

### Blunt conclusion
- pre-sales itinerary concept: optional, lightweight, sales-side
- confirmed itinerary: booking-side operational truth

---

## Stage 8 - Supplier fulfillment

### Purpose
Track execution against suppliers/vendors.

### Owning entities
- `BookingItem`
- supplier confirmation and issuance workflow around booking items

### Meaning
This stage tracks:
- requested
- pending supplier
- confirmed
- issued/ticketed
- failed
- cancelled

### Rule
Supplier fulfillment belongs after booking creation.
Do not run fulfillment against unaccepted quotes as the normal workflow.

---

## Stage 9 - Payment tracking

### Purpose
Expose financial state in travel context.

### Ownership split
- billing-service owns invoice/payment/refund truth
- travel-service projects booking-level financial visibility

### Booking-side visibility should include
- total sell amount
- paid amount
- outstanding amount
- payment status
- next due date
- invoice count
- refund visibility later

### Rule
Payment state should be attached to booking lifecycle, not inquiry lifecycle.

---

## Stage 10 - Travel docs / vouchers

### Purpose
Store and expose post-booking customer/ops documents.

### Owning entity
- `BookingDocument`

### Typical artifacts
- voucher
- ticket
- supplier confirmation
- insurance
- passport copy
- travel doc bundle

### Rule
Documents/vouchers belong after booking and typically after fulfillment progress.

---

## Stage 11 - Trip completion / changes / cancellations

### Purpose
Handle real-world lifecycle after booking exists.

### Owning concepts
- booking status progression
- booking change requests
- cancellations
- refunds/credits visibility later

### Meaning
This is where the ugly reality lives:
- date changes
- traveler changes
- cancellations
- penalties
- refunds
- completion

---

# 4. Audit of current implementation against canonical lifecycle

This section audits current travel-service design as of 2026-04-17.

## 4.1 Inquiry stage
### Status
Partially implemented now on branch `feat/travel-inquiry-intake`.

### Notes
- `TravelInquiry` foundation and internal management APIs were added.
- public intake endpoint not added yet.
- inquiry -> contact + quotation conversion not added yet.

### Gap
Need conversion workflow and later public intake.

---

## 4.2 Sales notes / draft itinerary concept
### Status
Partially supported through generic notes/timeline patterns.

### Gap
No explicit lightweight draft itinerary concept linked to inquiry.
This is acceptable for now if inquiry notes cover the need.

---

## 4.3 Quotation + revisions
### Status
Reasonably strong.

### Already present
- quotations
- revisions
- attachments
- send/share/public view
- accept/reject/expire
- approval requests

### Conclusion
This stage is in decent shape.

---

## 4.4 Acceptance -> booking creation
### Status
Implemented.

### Already present
- accepted revision required
- booking creation from quotation endpoint
- booking root and booking status history

### Conclusion
This stage aligns well with canonical lifecycle.

---

## 4.5 Itinerary stage
### Status
**This is where the current design is least aligned.**

### Current reality
- `Itinerary` currently links to `QuotationId`
- itinerary creation is customer/contact-centric and quote-linked
- `POST /travel/quotations/{id}/convert` converts accepted quotation directly into itinerary
- itinerary is currently treated as something that can live before/parallel to booking rather than clearly after booking in ops stage

### Why this is a problem
That makes itinerary feel like a quote-adjacent object instead of a booking-owned operational truth.

### Canonical recommendation
Refactor itinerary model toward:
- booking-linked confirmed itinerary
- optional earlier sales-side draft concept later if needed

### Verdict
This is the main architecture mismatch.

---

## 4.6 Supplier fulfillment
### Status
Implemented in booking context.

### Already present
- booking items
- confirmation requests
- confirm/issue actions

### Conclusion
This stage aligns well with the canonical model.

---

## 4.7 Payment tracking
### Status
Partially implemented.

### Current reality
- booking financial summary query exists
- billing-service integration provides travel-side finance visibility

### Conclusion
Direction is correct.
Needs continued refinement, but stage placement is right.

---

## 4.8 Travel docs / vouchers
### Status
Implemented in booking context.

### Conclusion
Good alignment.

---

## 4.9 Completion / changes / cancellations
### Status
Partially implemented.

### Current reality
- booking change request work exists
- booking status and cancellation basics exist

### Conclusion
Direction is correct.

---

# 5. Required implementation decisions from this audit

## Decision 1 - Keep inquiry first-class
Yes.
Continue implementation.

## Decision 2 - Inquiry should convert into quotation, not booking
Yes.
This must remain the normal path.

## Decision 3 - Booking should remain created from accepted quotation revision
Yes.
This is correct and should stay.

## Decision 4 - Confirmed itinerary should move toward booking ownership
Yes.
This is the biggest architecture correction needed.

## Decision 5 - Current quote -> itinerary conversion should be reconsidered
Yes.
It likely needs to evolve into one of these:
- accepted quote -> booking -> booking itinerary
- or accepted quote -> booking + itinerary creation together, but itinerary linked to booking

It should not remain permanently quote-owned if the itinerary is meant to represent post-sale operational truth.

---

# 6. Recommended implementation sequence from here

## Step 1
Finish inquiry workflow:
- inquiry -> contact + quotation conversion

## Step 2
Refactor itinerary architecture:
- introduce booking linkage for itinerary
- make confirmed itinerary belong to booking stage
- keep early pre-sales planning lightweight and separate if needed later

## Step 3
Align API surfaces:
- avoid quote-side endpoint implying itinerary is fundamentally a quotation artifact
- prefer booking-side itinerary creation/finalization flows

## Step 4
Continue payment visibility and late lifecycle maturity

---

# 7. Non-hallucination contract for future coding

Future backend work should follow these rules:

1. Inquiry is lead intake only.
2. Sales notes/draft planning may happen before quotation.
3. Quotation is the commercial offer.
4. Revisions preserve commercial negotiation history.
5. Acceptance freezes what customer approved.
6. Booking is created from accepted quotation revision.
7. Confirmed itinerary belongs to booking/ops stage.
8. Supplier fulfillment belongs to booking stage.
9. Payment visibility belongs to booking context, backed by billing-service truth.
10. Documents/vouchers belong to booking stage.
11. Changes/cancellations/completion are post-booking lifecycle concerns.

If a feature conflicts with those rules, stop and redesign it.

---

# 8. Final blunt conclusion

Most of Voyara’s current travel-service architecture is pointed in the right direction.
The biggest remaining workflow risk is not inquiry anymore.
It is **itinerary ownership confusion**.

The safe long-term answer is:
- lightweight trip concept can exist before acceptance
- confirmed itinerary must belong to booking lifecycle

That keeps commercial flow and operational flow connected without turning the backend into a confused soup.
