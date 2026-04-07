# Phase 7 Booking API Examples

Practical sample payloads for the booking and fulfillment workflow.

## Create booking from accepted quotation

`POST /api/travel/bookings/from-quotation/{quotationId}`

```json
{
  "assignedToUserId": null,
  "internalNotes": "Priority booking"
}
```

## List bookings with filters

`GET /api/travel/bookings?page=1&pageSize=20&status=Pending&destination=Rome`

Optional filters:
- `status`
- `destination`
- `startDateFrom`
- `startDateTo`
- `assignedToUserId`
- `primaryContactId`

## Add traveler

`POST /api/travel/bookings/{bookingId}/travelers`

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

## Add booking item

`POST /api/travel/bookings/{bookingId}/items`

```json
{
  "type": "Hotel",
  "title": "Rome hotel stay",
  "description": "4 nights in central Rome",
  "supplierName": "Example Hotels",
  "supplierReference": "EH-7782",
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

## Update booking item status

`PATCH /api/travel/bookings/{bookingId}/items/{itemId}/status`

```json
{
  "status": "Confirmed"
}
```

## Upload booking document

`POST /api/travel/bookings/{bookingId}/documents`

Multipart form fields:
- `file`
- `travelerId` (optional)
- `documentType`
- `description`
- `isCustomerVisible`

Typical `documentType` values:
- `Voucher`
- `Ticket`
- `Confirmation`
- `Invoice`
- `Receipt`
- `PassportCopy`
- `Visa`
- `Insurance`
- `Other`
