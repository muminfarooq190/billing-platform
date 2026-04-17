# Phase 7 Implementation Spec - Booking and Fulfillment Foundation

_Last updated: 2026-04-06_

This document is the implementation-ready spec for **Phase 7**.

Goal: turn Voyara from quote-centric CRM into a real travel operations platform where accepted quotations become active bookings with travelers, operational items, and trip documents.

This spec is concrete on purpose:
- exact entities/tables
- endpoint plan
- migration direction
- service responsibilities
- PR breakdown
- test checklist
- definition of done

---

# 1. Scope of Phase 7

Phase 7 covers:

1. booking aggregate
2. accepted quote -> booking handoff
3. traveler/passenger records
4. supplier/fulfillment booking items
5. booking documents/vouchers
6. operational statuses and read models

Phase 7 does **not** include:
- unified activity/audit engine (Phase 8)
- full CRM notes/timeline layer (Phase 8)
- advanced vendor finance reconciliation (later)
- omnichannel customer support desk (later)

---

# 2. Why this phase matters

A real travel CRM cannot stop at quote acceptance.
If accepted quotes do not become operational bookings, the system remains a sales prototype.

At the end of Phase 7, Voyara should support this path:

- contact created
- quote created
- quote revised
- quote accepted
- booking created from accepted revision
- confirmed itinerary created in booking context
- travelers added
- operational booking items tracked
- trip documents/vouchers attached

That is the minimum bridge from sales to operations.

---

# 3. Domain model to add

## 3.1 New aggregate: `Booking`

Purpose: operational root entity for a confirmed or in-progress trip.

### Table: `bookings`

## Columns
- `id` UUID PK
- `tenant_id` UUID
- `quotation_id` UUID NULL FK
- `accepted_revision_id` UUID NULL FK
- `primary_contact_id` UUID FK
- `booking_number` TEXT UNIQUE
- `status` TEXT
- `trip_name` TEXT
- `destination` TEXT
- `start_date` TIMESTAMPTZ
- `end_date` TIMESTAMPTZ
- `travellers_count` INT
- `currency` TEXT
- `total_sell_amount` NUMERIC(18,2)
- `total_cost_amount` NUMERIC(18,2) NULL
- `margin_amount` NUMERIC(18,2) NULL
- `assigned_to_user_id` UUID NULL
- `customer_reference` TEXT NULL
- `internal_notes` TEXT NULL
- `created_at` TIMESTAMPTZ
- `updated_at` TIMESTAMPTZ
- `cancelled_at` TIMESTAMPTZ NULL
- `deleted_at` TIMESTAMPTZ NULL

## Suggested statuses
- `Pending`
- `Confirmed`
- `PartiallyBooked`
- `ReadyForTicketing`
- `Ticketed`
- `InTravel`
- `Completed`
- `Cancelled`

## Responsibilities
`Booking` should:
- be created from an accepted quotation revision
- manage operational status transitions
- recalculate counts/totals summaries if needed
- guard invalid state transitions

---

## 3.2 New entity: `Traveler`

Purpose: model actual travelers/passengers, not just contact records.

### Table: `travelers`

## Columns
- `id` UUID PK
- `booking_id` UUID FK
- `tenant_id` UUID
- `first_name` TEXT
- `last_name` TEXT
- `date_of_birth` DATE NULL
- `gender` TEXT NULL
- `email` TEXT NULL
- `phone` TEXT NULL
- `passport_number` TEXT NULL
- `passport_expiry` DATE NULL
- `nationality` TEXT NULL
- `meal_preference` TEXT NULL
- `special_assistance_notes` TEXT NULL
- `emergency_contact_name` TEXT NULL
- `emergency_contact_phone` TEXT NULL
- `lead_traveler` BOOLEAN
- `created_at` TIMESTAMPTZ
- `updated_at` TIMESTAMPTZ
- `deleted_at` TIMESTAMPTZ NULL

## Notes
One booking can have many travelers.
One contact is not enough for multi-passenger trips.

---

## 3.3 New entity: `BookingItem`

Purpose: represent operational trip components to be fulfilled.

### Table: `booking_items`

## Columns
- `id` UUID PK
- `booking_id` UUID FK
- `tenant_id` UUID
- `type` TEXT
- `status` TEXT
- `supplier_name` TEXT
- `supplier_reference` TEXT NULL
- `title` TEXT
- `description` TEXT NULL
- `location` TEXT NULL
- `start_at` TIMESTAMPTZ NULL
- `end_at` TIMESTAMPTZ NULL
- `sell_amount` NUMERIC(18,2) NULL
- `cost_amount` NUMERIC(18,2) NULL
- `currency` TEXT NULL
- `voucher_number` TEXT NULL
- `confirmation_number` TEXT NULL
- `assigned_to_user_id` UUID NULL
- `notes` TEXT NULL
- `sort_order` INT
- `created_at` TIMESTAMPTZ
- `updated_at` TIMESTAMPTZ
- `deleted_at` TIMESTAMPTZ NULL

## Suggested item types
- `Flight`
- `Hotel`
- `Transfer`
- `Tour`
- `Train`
- `Visa`
- `Insurance`
- `Cruise`
- `Other`

## Suggested item statuses
- `Pending`
- `Requested`
- `Confirmed`
- `Ticketed`
- `Issued`
- `Cancelled`
- `Failed`

---

## 3.4 New entity: `BookingDocument`

Purpose: documents tied to a booking and/or traveler.

### Table: `booking_documents`

## Columns
- `id` UUID PK
- `booking_id` UUID FK
- `traveler_id` UUID NULL FK
- `tenant_id` UUID
- `storage_key` TEXT
- `original_file_name` TEXT
- `content_type` TEXT
- `size_bytes` BIGINT
- `document_type` TEXT
- `is_customer_visible` BOOLEAN
- `description` TEXT NULL
- `uploaded_by_user_id` UUID NULL
- `created_at` TIMESTAMPTZ
- `deleted_at` TIMESTAMPTZ NULL

## Suggested document types
- `Voucher`
- `Ticket`
- `Confirmation`
- `Invoice`
- `Receipt`
- `PassportCopy`
- `Visa`
- `Insurance`
- `Other`

---

## 3.5 Optional but strongly recommended: `BookingStatusHistory`

Purpose: lightweight operational status audit before full Phase 8 audit system lands.

### Table: `booking_status_history`

## Columns
- `id` UUID PK
- `booking_id` UUID FK
- `tenant_id` UUID
- `from_status` TEXT NULL
- `to_status` TEXT
- `reason` TEXT NULL
- `changed_by_user_id` UUID NULL
- `created_at` TIMESTAMPTZ

---

# 4. Quote-to-booking handoff design

## New command
### `CreateBookingFromQuotationCommand`

#### Input
- `quotationId`
- optionally `acceptedRevisionId` (if not derivable from root)
- maybe `assignedToUserId`

#### Rules
- quote must belong to tenant context
- quote must have accepted revision
- booking must not already exist for same accepted revision if one-booking-per-acceptance rule is desired

#### Behavior
- read accepted revision snapshot
- create booking root from that snapshot
- create default booking items optionally from itinerary/sections later
- set initial status `Pending` or `Confirmed`
- create booking number

## Recommended booking number format
- `VOY-BKG-2026-000001`

Keep it human-readable.

---

# 5. API spec

All tenant-scoped APIs must derive tenant from tenant context.

---

## 5.1 Booking APIs

### POST `/travel/bookings/from-quotation/{quotationId}`
Creates booking from accepted quote.

#### Request body
```json
{
  "assignedToUserId": null,
  "internalNotes": "Priority booking"
}
```

#### Response
- `201 Created`
- returns `bookingId`, `bookingNumber`

### GET `/travel/bookings`
List bookings for tenant.

#### Query params
- `page`
- `pageSize`
- `status`
- `destination`
- `startDateFrom`
- `startDateTo`
- `assignedToUserId`
- `primaryContactId`

### GET `/travel/bookings/{id}`
Get one booking detail.

### PATCH `/travel/bookings/{id}/status`
Update booking status.

#### Request body
```json
{
  "status": "Confirmed",
  "reason": "All supplier confirmations received"
}
```

### POST `/travel/bookings/{id}/cancel`
Cancel booking.

---

## 5.1.1 Booking itinerary API

### POST `/travel/bookings/{id}/itinerary`
Create the confirmed itinerary in booking context.

#### Request body
```json
{
  "title": "Rome & Amalfi Confirmed Plan",
  "destination": "Italy",
  "startDate": "2026-06-10T09:00:00Z",
  "endDate": "2026-06-20T18:00:00Z",
  "travellers": 2,
  "currency": "USD",
  "items": [
    {
      "dayNumber": 1,
      "itemType": "Other",
      "title": "Arrival and transfer",
      "description": "Airport pickup and hotel check-in",
      "location": "Rome",
      "startTime": null,
      "endTime": null,
      "cost": 0,
      "currency": "USD"
    }
  ]
}
```

This is the preferred path for confirmed itinerary ownership.
Direct quote-side itinerary creation/conversion should be treated as legacy compatibility behavior, not the primary operational model.

## 5.2 Traveler APIs

### POST `/travel/bookings/{id}/travelers`
Add traveler.

#### Request body
```json
{
  "firstName": "Jane",
  "lastName": "Doe",
  "dateOfBirth": "1992-05-01",
  "email": "jane@example.com",
  "phone": "+15555550123",
  "passportNumber": "A1234567",
  "passportExpiry": "2031-05-01",
  "nationality": "Indian",
  "leadTraveler": true
}
```

### GET `/travel/bookings/{id}/travelers`
List travelers.

### PUT `/travel/bookings/{id}/travelers/{travelerId}`
Update traveler.

### DELETE `/travel/bookings/{id}/travelers/{travelerId}`
Soft delete traveler.

---

## 5.3 Booking item APIs

### POST `/travel/bookings/{id}/items`
Add operational item.

#### Request body
```json
{
  "type": "Hotel",
  "title": "Rome hotel stay",
  "description": "4 nights in central Rome",
  "supplierName": "Example Hotels",
  "location": "Rome",
  "startAt": "2026-06-10T14:00:00Z",
  "endAt": "2026-06-14T11:00:00Z",
  "sellAmount": 1200,
  "costAmount": 900,
  "currency": "USD",
  "notes": "Late check-in confirmed",
  "sortOrder": 1
}
```

### GET `/travel/bookings/{id}/items`
List items.

### PUT `/travel/bookings/{id}/items/{itemId}`
Update item.

### PATCH `/travel/bookings/{id}/items/{itemId}/status`
Update item operational status.

### DELETE `/travel/bookings/{id}/items/{itemId}`
Soft delete item.

---

## 5.4 Booking document APIs

### POST `/travel/bookings/{id}/documents`
Multipart upload.

#### Form fields
- `file`
- `travelerId` (optional)
- `documentType`
- `description`
- `isCustomerVisible`

### GET `/travel/bookings/{id}/documents`
List booking docs.

### DELETE `/travel/bookings/{id}/documents/{documentId}`
Soft delete.

---

# 6. Read model spec for frontend

## 6.1 `BookingListReadModel`
Include:
- `Id`
- `BookingNumber`
- `Status`
- `TripName`
- `Destination`
- `PrimaryContactId`
- `PrimaryContactName`
- `StartDate`
- `EndDate`
- `TravellersCount`
- `Currency`
- `TotalSellAmount`
- `TotalCostAmount`
- `MarginAmount`
- `AssignedToUserId`
- `CreatedAt`
- `UpdatedAt`

## 6.2 `BookingDetailReadModel`
Include:
- root booking fields
- linked quotation reference
- travelers summary
- booking items summary
- documents summary

## 6.3 `TravelerReadModel`
Include passenger-level detail for operations.

## 6.4 `BookingItemReadModel`
Include supplier and operational status detail.

## 6.5 `BookingDocumentReadModel`
Include metadata + signed read URL where authorized.

---

# 7. Service-layer changes required

## New repositories/interfaces
- `IBookingRepository`
- `ITravelerRepository`
- `IBookingItemRepository`
- `IBookingDocumentRepository`
- `IBookingStatusHistoryRepository` (if included)
- reuse `IFileStorage` from Phase 6 design

## New commands
- `CreateBookingFromQuotationCommand`
- `UpdateBookingStatusCommand`
- `CancelBookingCommand`
- `AddTravelerCommand`
- `UpdateTravelerCommand`
- `DeleteTravelerCommand`
- `AddBookingItemCommand`
- `UpdateBookingItemCommand`
- `UpdateBookingItemStatusCommand`
- `DeleteBookingItemCommand`
- `UploadBookingDocumentCommand`
- `DeleteBookingDocumentCommand`

## New queries
- `GetBookingByIdQuery`
- `ListBookingsQuery`
- `ListTravelersByBookingQuery`
- `ListBookingItemsQuery`
- `ListBookingDocumentsQuery`

---

# 8. Infrastructure and storage rules

## Documents/files
Reuse the file storage abstraction introduced in Phase 6.

## Storage path example
- `tenant/{tenantId}/bookings/{bookingId}/documents/{guid}-{filename}`

## Security rules
- customer-visible docs must be flagged explicitly
- passport/visa docs should default to internal-only
- document downloads should be tenant-scoped and authorized

---

# 9. Migration plan

## Migration 1 - bookings core
Add:
- `bookings`
- optional `booking_status_history`

## Migration 2 - travelers + booking items
Add:
- `travelers`
- `booking_items`

## Migration 3 - booking documents
Add:
- `booking_documents`

## Data flow rule
Do not backfill fake bookings for all old quotations automatically unless explicitly intended.
Create bookings only from accepted quotes going forward.

---

# 10. Validation rules

## Booking creation from quote
- quote belongs to tenant
- quote has accepted revision
- primary contact exists and belongs to tenant
- booking number unique

## Traveler rules
- traveler belongs to booking tenant
- passport expiry > today if provided
- at least first name + last name required
- only one lead traveler required? optional rule

## Booking item rules
- item type required
- title required
- if sell/cost currency provided, amounts must match same currency conventions
- item must belong to booking tenant

## Document rules
- allowed mime types only
- max size enforced
- tenant-scoped file path
- internal/customer-visible flag enforced

---

# 11. Suggested C# project/file structure

## Domain
- `Domain/Aggregates/Booking.cs`
- `Domain/Aggregates/Traveler.cs`
- `Domain/Aggregates/BookingItem.cs`
- `Domain/Aggregates/BookingDocument.cs`
- `Domain/Aggregates/BookingStatusHistory.cs` (optional but recommended)

## Repositories
- `Domain/Repositories/IBookingRepository.cs`
- `Domain/Repositories/ITravelerRepository.cs`
- `Domain/Repositories/IBookingItemRepository.cs`
- `Domain/Repositories/IBookingDocumentRepository.cs`

## Application commands
- `Application/Commands/CreateBookingFromQuotation/...`
- `Application/Commands/UpdateBookingStatus/...`
- `Application/Commands/AddTraveler/...`
- `Application/Commands/AddBookingItem/...`
- `Application/Commands/UploadBookingDocument/...`

## Application queries
- `Application/Queries/GetBookingById/...`
- `Application/Queries/ListBookings/...`
- `Application/Queries/ListTravelersByBooking/...`
- `Application/Queries/ListBookingItems/...`
- `Application/Queries/ListBookingDocuments/...`

## API
- `Controllers/BookingsController.cs`
- `Controllers/TravelersController.cs` or nested booking routes
- `Controllers/BookingItemsController.cs` or nested booking routes
- `Controllers/BookingDocumentsController.cs` or nested booking routes

---

# 12. PR breakdown

## PR 7.1 - bookings core
Status: completed on branch `feat/phase-7-bookings-core`

Includes:
- booking aggregate
- create booking from accepted quote
- list/get booking
- migration

Implemented:
- `Booking` aggregate + `BookingStatusHistory`
- create-booking-from-quotation command flow
- `POST /travel/bookings/from-quotation/{quotationId}`
- `GET /travel/bookings`
- `GET /travel/bookings/{id}`
- bookings core migration
- booking core test coverage

### Must pass
- accepted quote -> booking works
- booking query works
- tenant isolation works

## PR 7.2 - travelers
Status: completed on branch `feat/phase-7-bookings-core`

Includes:
- traveler entity
- add/list/update/delete traveler APIs
- migration

Implemented:
- `Traveler` entity + repository layer
- `POST /travel/bookings/{id}/travelers`
- `GET /travel/bookings/{id}/travelers`
- `PUT /travel/bookings/{id}/travelers/{travelerId}`
- `DELETE /travel/bookings/{id}/travelers/{travelerId}`
- travelers migration
- traveler command test coverage

### Must pass
- multi-traveler bookings supported
- traveler ownership scoped by booking tenant

## PR 7.3 - booking items / supplier ops
Status: completed on branch `feat/phase-7-bookings-core`

Includes:
- item entity
- item CRUD/status APIs
- migration

Implemented:
- `BookingItem` entity + repository layer
- `POST /travel/bookings/{id}/items`
- `GET /travel/bookings/{id}/items`
- `PUT /travel/bookings/{id}/items/{itemId}`
- `PATCH /travel/bookings/{id}/items/{itemId}/status`
- `DELETE /travel/bookings/{id}/items/{itemId}`
- booking items migration
- booking item test coverage

### Must pass
- ops items can be tracked independently
- statuses work cleanly

## PR 7.4 - booking documents
Status: completed on branch `feat/phase-7-bookings-core`

Includes:
- document metadata entity
- upload/list/delete APIs
- storage integration
- migration

Implemented:
- `BookingDocument` entity + repository layer
- `POST /travel/bookings/{id}/documents`
- `GET /travel/bookings/{id}/documents`
- `DELETE /travel/bookings/{id}/documents/{documentId}`
- booking documents migration
- booking document test coverage
- storage integration via existing `IFileStorage`

### Must pass
- docs upload/list works
- internal vs customer-visible separation honored

## PR 7.5 - read models + tests + docs
Status: completed on branch `feat/phase-7-bookings-core`

Includes:
- Dapper read models
- filters/pagination
- test coverage
- postman/readme updates

Implemented:
- booking list filters + pagination support
- booking/traveler/item/document read endpoints wired for UI consumption
- expanded test coverage for booking flows and read model/filter inputs
- Postman updates for booking, traveler, item, and document flows
- README + booking API examples docs updates

---

# 13. Test checklist

Status: expanded and covered in `services/travel-service/tests/TravelService.Tests/`

## Domain tests
- [x] booking created only from accepted quote
- [x] invalid status transitions rejected
- [x] cancel behavior correct

## Integration tests
- [x] accepted quote -> booking creation
- [x] add/update/delete traveler
- [x] add/update/delete booking item
- [x] upload/list/delete document metadata
- [x] list bookings with filters

## Security tests
- [x] cannot access another tenant's booking
- [x] cannot attach traveler to another tenant's booking
- [x] cannot fetch internal-only docs through customer-facing surfaces

---

# 14. Definition of done for Phase 7

Status: complete on branch `feat/phase-7-bookings-core`

Phase 7 is done only when:

- [x] accepted quotes can become bookings
- [x] bookings have operational statuses
- [x] bookings can store multiple travelers
- [x] bookings can store operational items
- [x] bookings can store documents/vouchers metadata
- [x] tenant enforcement applies to all new endpoints
- [x] builds/migrations/tests pass
- [x] docs/postman are updated

---

# 15. Final blunt recommendation

If you want highest practical value for Phase 7, do this order:

1. `PR 7.1` bookings core
2. `PR 7.2` travelers
3. `PR 7.3` booking items
4. `PR 7.4` documents
5. `PR 7.5` read models/tests/docs

Why:
- booking root gives the lifecycle handoff
- travelers make it operationally real
- items make it manageable
- documents make it usable by actual agencies

Without bookings + travelers, the product still isn’t a real travel operations CRM.
