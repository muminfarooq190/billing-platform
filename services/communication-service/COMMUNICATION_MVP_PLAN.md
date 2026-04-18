# Communication Service MVP Readiness Plan

Goal: make communication-service ready to send customer-facing communications for quotations, itineraries, invoices, and similar documents through email and WhatsApp.

## Priority 1 - Channel readiness
- Add `WhatsApp` as a first-class channel
- Implement Twilio WhatsApp provider and dispatcher
- Keep SMS/email existing paths intact

## Priority 2 - Document-aware customer delivery
- Extend email sending to render document links/attachments context clearly
- Ensure workflow notifications can carry quote PDF / itinerary PDF / invoice refs safely
- Do not introduce raw binary upload complexity for MVP

## Priority 3 - Customer-facing workflow coverage
- Support quotation sent
- Support itinerary sent
- Support booking confirmed
- Support invoice/payment communication

## Priority 4 - Validation
- Tests for WhatsApp dispatch surface
- Tests for document-aware email rendering / workflow defaults
- Build + targeted test pass

## Scope note
This MVP pass focuses on practical send-readiness, not every possible provider callback or omnichannel edge case.
