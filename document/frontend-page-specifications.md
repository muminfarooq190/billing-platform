# Frontend Page Specifications

## 1. Purpose

This document turns the architecture and implementation plan into page-level specs for:
- Admin portal
- Customer web portal
- Shared/public quotation pages

---

## 2. Admin portal page list

## 2.1 Authentication
- Login page
- Register tenant page
- Logout flow

## 2.2 Dashboard
### Page: Admin Dashboard
**Purpose:** high-level operating view
**Blocks:**
- KPI cards
- recent quotations
- bookings queue
- due follow-ups
- overdue invoices
- recent activity
**Actions:**
- create quotation
- add contact
- open due follow-up
- view invoice

## 2.3 CRM - Contacts
### Page: Contacts List
**Columns:** name, email, phone, company, tags, createdAt
**Filters:** search, tag, pagination
**Actions:** create, edit, delete, open detail

### Page: Contact Detail
**Sections:** profile, notes, tags, linked quotations, linked follow-ups, activity summary

## 2.4 Follow-ups
### Page: Follow-up List
**Columns:** subject, customer, priority, dueDate, status, assignedTo
**Filters:** status, due range, customer
**Actions:** create, edit, complete/cancel transitions

## 2.5 Quotations
### Page: Quotations List
**Columns:** title, customerName, destination, travelDate, returnDate, status, validUntil, total
**Filters:** status, customerName, date range
**Actions:** create, open, send, expire

### Page: Quotation Detail
**Tabs:**
- overview
- revisions
- attachments
- history
- timeline
**Header fields:** title, customer, status, destination, dates, travellers, currency, validUntil
**Primary actions:** edit, create revision, send, expire, reject, convert to itinerary

### Page: Create/Edit Quotation
**Sections:**
- customer selection
- trip details
- line items
- notes
- validity

### Page: Quotation Revision Detail
**Sections:** snapshot details, visible notes, internal notes, line items, totals

## 2.6 Itineraries
### Page: Itineraries List
**Columns:** title, customer, destination, startDate, endDate, status, totalCost
**Actions:** create, open, update

### Page: Itinerary Detail
**Sections:** summary, day-wise itinerary, status actions, linked quotation

## 2.7 Bookings
### Page: Bookings List
**Columns:** bookingNumber, tripName, destination, startDate, endDate, status, assignedTo
**Filters:** status, destination, date range, assignee

### Page: Booking Detail
**Tabs:** summary, travelers, items, documents, timeline, financials
**Header fields:** bookingNumber, tripName, destination, dates, status, totalSellAmount, assignedTo

### Page: Traveler Management
**Fields:** firstName, lastName, DOB, passport, nationality, meal preference, emergency contact, lead traveler

### Page: Booking Items
**Fields:** type, title, supplierName, supplierReference, location, timings, sell/cost amounts, status, voucher, confirmation

### Page: Booking Documents
**Fields:** documentType, traveler, file, description, createdAt, customerVisible

## 2.8 Billing
### Page: Billing Dashboard
**Blocks:** subscription summary, invoices overview, overdue counts

### Page: Invoices List
**Columns:** invoiceId, status, subtotal, tax, total, issuedAt, dueDate, paidAt
**Actions:** view, generate, pay

### Page: Invoice Detail
**Sections:** financial breakdown, line items if exposed, payment state, timestamps

## 2.9 Communication
### Page: Notifications
**Columns:** subject, recipient, channel, status, priority, sentAt, readAt

### Page: Templates
**Columns:** name, channel, status, updatedAt
**Actions:** create, edit, archive/activate

### Page: Recipient Preferences
**Sections:** contact info, timezone, per-channel toggles, quiet hours

## 2.10 Webhooks
### Page: Webhook Subscriptions
**Columns:** targetUrl, events, active, createdAt
**Actions:** create, deactivate

### Page: Delivery Logs
**Columns:** eventType, status, attemptCount, responseStatusCode, createdAt
**Actions:** open detail, replay

## 2.11 Identity & Settings
### Page: Users
**Columns:** email, role, lastLoginAt, createdAt
**Actions:** create, edit, delete

### Page: Tenant Settings
**Sections:** tenant profile, plan, status, suspend action

---

## 3. Customer web portal page list

## 3.1 Home
### Page: Customer Overview
**Blocks:**
- next trip
- pending quotation
- invoice due card
- unread notifications
- quick links

## 3.2 Quotations
### Page: Quotations List
**Columns/cards:** title, destination, dates, validity, status

### Page: Quotation Detail
**Sections:** summary, price breakdown, notes, attachments, action area

## 3.3 Trips
### Page: Trips List
**Cards:** destination, dates, status, traveler count

### Page: Trip Detail
**Sections:** summary, itinerary preview, traveler list, document shortcuts, support/help slot

## 3.4 Itinerary
### Page: Itinerary Detail
**Sections:** day groups, itinerary items, timings, locations

## 3.5 Documents
### Page: Documents Center
**Views:** by trip, by traveler, all documents

## 3.6 Billing
### Page: Invoices List
**Fields:** amount, status, due date, paid date

### Page: Invoice Detail
**Fields:** subtotal, tax, total, timestamps

## 3.7 Notifications
### Page: Notification Center
**Fields:** subject, body preview, sentAt, read state

## 3.8 Preferences
### Page: Preferences
**Fields:** email, phone, timezone, channel preferences, quiet hours

---

## 4. Public quotation page spec

### Route
- `/quote/[token]`

### Blocks
- hero summary
- destination and dates
- quotation status / validity
- line item breakdown
- visible notes
- media and attachments
- trust / contact area

### UX requirements
- mobile-first
- premium visual feel
- strong empty/error states
- viewed tracking on load

---

## 5. Shared UX states required everywhere
- loading
- empty
- error
- permission denied
- archived/cancelled/inactive state
- destructive action confirmation
- file upload progress
