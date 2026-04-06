export const QUOTATION_ACCEPTED_EVENT_TYPE = 'travel.quotation.accepted' as const;

export interface QuotationAcceptedEventPayload {
  readonly quotationId: string;
  readonly tenantId: string;
  readonly acceptedAt: string;
}

export interface QuotationAcceptedEvent {
  readonly eventId: string;
  readonly eventType: typeof QUOTATION_ACCEPTED_EVENT_TYPE;
  readonly occurredAt: string;
  readonly aggregateId: string;
  readonly aggregateType: 'Quotation';
  readonly tenantId: string;
  readonly version: number;
  readonly payload: QuotationAcceptedEventPayload;
}
