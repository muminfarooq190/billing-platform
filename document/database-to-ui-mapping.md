# Database to UI Mapping

## 1. Purpose

This document maps current domain entities and key schema fields to frontend modules and screens.

---

## 2. Identity domain

## Tenant
**Fields:**
- Id
- Name
- Email
- Plan
- Status
- CreatedAt
- UpdatedAt
- DeletedAt

**UI usage:**
- tenant settings page
- plan management
- tenant summary cards
- onboarding/register success state

## User
**Fields:**
- Id
- TenantId
- Email
- Role
- LastLoginAt
- CreatedAt
- UpdatedAt
- DeletedAt

**UI usage:**
- users list
- user detail/edit modal
- role badge rendering
- last active display

---

## 3. Billing domain

## Subscription
**Fields:**
- Id
- TenantId
- PlanType
- BillingCycle
- Status
- StartDate
- NextBillingDate
- CancelledAt

**UI usage:**
- billing dashboard
- subscription summary card
- plan lifecycle status

## Invoice
**Fields:**
- Id
- SubscriptionId
- TenantId
- Subtotal
- TaxAmount
- Total
- Status
- DueDate
- PaidAt
- IssuedAt
- CreatedAt
- UpdatedAt

**UI usage:**
- invoices list
- invoice detail
- due/paid/overdue badges
- customer billing view

---

## 4. Travel CRM domain

## Contact
**Fields:**
- Id
- TenantId
- FirstName
- LastName
- Email
- Phone
- Company
- Notes
- TagsJson / Tags
- CreatedAt
- UpdatedAt
- DeletedAt

**UI usage:**
- contacts table
- contact profile
- search results
- CRM linking in quotation/follow-up flows

## FollowUp
**Fields:**
- Id
- CustomerContactId
- CustomerName
- Subject
- Notes
- Priority
- Status
- DueDate
- AssignedToUserId
- CompletedAt
- CreatedAt
- UpdatedAt

**UI usage:**
- follow-up queue
- overdue indicators
- assignee filters
- dashboard reminders

## Quotation
**Fields:**
- Id
- CustomerContactId
- CustomerName
- Title
- Destination
- TravelDate
- ReturnDate
- Travellers
- Currency
- Notes
- Status
- ValidUntil
- CurrentRevisionNumber
- AcceptedRevisionId
- LastSentAt
- LastViewedAt
- ExpiredAt
- RejectedAt
- ShareToken
- ShareTokenExpiresAt
- CreatedAt
- UpdatedAt

**UI usage:**
- quotations list
- quotation detail header
- validity warning
- customer portal quotations
- public quote page

## QuotationRevision
**Fields:**
- Id
- QuotationId
- RevisionNumber
- Status
- CustomerName
- Title
- Destination
- TravelDate
- ReturnDate
- Travellers
- Currency
- Notes
- VisibleNotes
- InternalNotes
- ValidUntil
- SubtotalAmount
- TaxAmount
- TotalAmount
- CreatedByUserId
- CreatedAt

**UI usage:**
- revision list
- revision detail
- customer-visible quote snapshot
- internal audit/history view

## QuotationRevisionLineItem
**Fields:**
- Description
- Quantity
- UnitPriceAmount
- Currency
- SortOrder
- LineTotal

**UI usage:**
- quotation editor
- revision detail breakdown
- public quote pricing display

## QuotationAttachment
**Fields:**
- Id
- QuotationId
- QuotationRevisionId
- OriginalFileName
- ContentType
- SizeBytes
- AttachmentType
- Caption
- IsCustomerVisible
- SortOrder
- CreatedAt

**UI usage:**
- attachments tab
- public quote media/gallery
- customer-visible documents/media

## QuotationStatusHistory
**Fields:**
- FromStatus
- ToStatus
- Reason
- ChangedByUserId
- CreatedAt

**UI usage:**
- history tab
- timeline/history feed

## QuotationShareLink
**Fields:**
- Token
- ExpiresAt
- RevokedAt
- LastViewedAt
- CreatedAt

**UI usage:**
- public quote sharing
- share link panel
- view tracking

## Itinerary
**Fields:**
- Id
- CustomerContactId
- CustomerName
- Title
- Destination
- StartDate
- EndDate
- Travellers
- Currency
- QuotationId
- Status
- TotalCost
- CreatedAt
- UpdatedAt

**UI usage:**
- itinerary list
- itinerary detail
- customer trip plan view
- itinerary timeline/day cards

## Booking
**Fields:**
- Id
- QuotationId
- AcceptedRevisionId
- PrimaryContactId
- BookingNumber
- Status
- TripName
- Destination
- StartDate
- EndDate
- TravellersCount
- Currency
- TotalSellAmount
- TotalCostAmount
- MarginAmount
- AssignedToUserId
- CustomerReference
- InternalNotes
- CreatedAt
- UpdatedAt
- CancelledAt

**UI usage:**
- bookings list
- booking detail
- operations dashboard
- customer trip summary

## Traveler
**Fields:**
- FirstName
- LastName
- DateOfBirth
- Gender
- Email
- Phone
- PassportNumber
- PassportExpiry
- Nationality
- MealPreference
- SpecialAssistanceNotes
- EmergencyContactName
- EmergencyContactPhone
- LeadTraveler

**UI usage:**
- traveler management forms
- traveler cards in booking detail
- customer traveler profile view

## BookingItem
**Fields:**
- Type
- Status
- SupplierName
- SupplierReference
- Title
- Description
- Location
- StartAt
- EndAt
- SellAmount
- CostAmount
- Currency
- VoucherNumber
- ConfirmationNumber
- AssignedToUserId
- Notes
- SortOrder

**UI usage:**
- operations fulfillment list
- itinerary-like booking item display
- customer trip component summaries

## BookingDocument
**Fields:**
- TravelerId
- OriginalFileName
- ContentType
- SizeBytes
- DocumentType
- IsCustomerVisible
- Description
- CreatedAt

**UI usage:**
- booking documents center
- traveler-specific document tabs
- customer mobile/web documents area

## ActivityEntry
**Fields:**
- EntityType
- EntityId
- ActivityType
- Summary
- DetailJson
- ActorUserId
- OccurredAt

**UI usage:**
- timeline components
- audit feed
- activity widgets

---

## 5. Communication domain

## Notification
**Fields:**
- RecipientId
- RecipientType
- Channel
- Subject
- Body
- Priority
- Status
- TemplateId
- ReferenceId
- RetryCount
- LastError
- ProviderMessageId
- SentAt
- DeliveredAt
- ReadAt
- CreatedAt

**UI usage:**
- notification center
- recipient history
- unread count widgets
- admin communications monitoring

## NotificationTemplate
**Fields:**
- Name
- Subject
- BodyTemplate
- Channel
- Description
- Status
- CreatedAt
- UpdatedAt

**UI usage:**
- templates list
- template editor
- template lifecycle actions

## RecipientPreferences
**Fields:**
- Email
- Phone
- DeviceToken
- Timezone
- ChannelPreferences
- CreatedAt
- UpdatedAt

**UI usage:**
- preferences forms
- quiet hours UI
- customer self-service settings

---

## 6. Webhook domain

## WebhookSubscription
**Fields:**
- id
- tenant_id
- target_url
- events[]
- signing_secret
- is_active
- created_at
- updated_at

**UI usage:**
- webhook subscriptions list
- create subscription form
- integration management screens

## WebhookDeliveryLog
**Fields:**
- id
- webhook_subscription_id
- event_type
- payload
- status
- attempt_count
- last_attempt_at
- response_status_code
- response_body
- created_at
- updated_at

**UI usage:**
- delivery logs table
- delivery detail drawer
- replay actions

---

## 7. Shared UI primitives driven by this schema
- StatusBadge
- EntityHeader
- TimelineList
- ActivityFeed
- FileList
- FilterBar
- DataTable
- MetricCard
- DateRangeDisplay
- CurrencyAmount
- NotificationRow
- DocumentRow
