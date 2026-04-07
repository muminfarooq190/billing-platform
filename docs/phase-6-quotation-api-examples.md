# Phase 6 Quotation API Examples

Practical sample payloads for the Phase 6 quotation workflow.

## Create quotation revision

`POST /api/travel/quotations/{quotationId}/revisions`

```json
{
  "title": "Summer Europe Trip - Premium",
  "destination": "Italy",
  "travelDate": "2026-06-10T09:00:00Z",
  "returnDate": "2026-06-20T18:00:00Z",
  "travellers": 2,
  "currency": "USD",
  "visibleNotes": "Breakfast included. Flexible date changes allowed before ticketing.",
  "internalNotes": "Target 22% margin. Keep hotel option B as fallback.",
  "validUntil": "2026-05-01T00:00:00Z",
  "lineItems": [
    {
      "description": "Flight + Hotel",
      "quantity": 1,
      "unitPrice": 2500,
      "currency": "USD"
    },
    {
      "description": "Airport transfer",
      "quantity": 1,
      "unitPrice": 120,
      "currency": "USD"
    }
  ]
}
```

## Send quotation

`POST /api/travel/quotations/{quotationId}/send`

```json
{
  "revisionId": "0d0fbad0-5af4-4d03-b636-63be56f72f4a",
  "channel": "Email",
  "recipientEmail": "traveler@example.com",
  "message": "Please review your updated proposal.",
  "expiresAt": "2026-05-01T00:00:00Z"
}
```

## Public quotation fetch

`GET /travel/quotations/public/{token}`

Returns customer-safe quotation data only:
- visible notes
- revision line items
- customer-visible attachments
- no internal notes
- no internal attachments

## Public quotation viewed tracking

`POST /travel/quotations/public/{token}/viewed`

No body required.

## Accept quotation

`POST /api/travel/quotations/{quotationId}/accept`

```json
{
  "revisionId": "0d0fbad0-5af4-4d03-b636-63be56f72f4a",
  "reason": "Approved by customer over WhatsApp"
}
```

## Reject quotation

`POST /api/travel/quotations/{quotationId}/reject`

```json
{
  "reason": "Customer postponed travel to next quarter"
}
```

## Expire quotation

`POST /api/travel/quotations/{quotationId}/expire`

```json
{
  "reason": "Fare validity window ended"
}
```
