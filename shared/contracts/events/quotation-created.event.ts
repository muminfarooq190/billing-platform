export const QUOTATION_CREATED_EVENT_TYPE = 'travel.quotation.created' as const;

export interface QuotationCreatedEventPayload {
  readonly quotationId: string;
  readonly tenantId: string;
  readonly customerContactId: string;
  readonly customerName: string;
  readonly title: string;
  readonly destination: string;
  readonly travelDate: string;
  readonly returnDate: string;
  readonly travellers: number;
  readonly currency: string;
  readonly totalAmount: string;
  readonly createdAt: string;
}

export interface QuotationCreatedEvent {
  readonly eventId: string;
  readonly eventType: typeof QUOTATION_CREATED_EVENT_TYPE;
  readonly occurredAt: string;
  readonly aggregateId: string;
  readonly aggregateType: 'Quotation';
  readonly tenantId: string;
  readonly version: number;
  readonly payload: QuotationCreatedEventPayload;
}
