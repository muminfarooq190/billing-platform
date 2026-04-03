export const PAYMENT_PROCESSED_EVENT_TYPE = 'billing.payment.processed' as const;

export type PaymentResult = 'Success' | 'Declined' | 'InsufficientFunds' | 'GatewayError';

export interface PaymentProcessedEventPayload {
  readonly paymentId: string;
  readonly invoiceId: string;
  readonly tenantId: string;
  readonly amount: string;
  readonly currency: string;
  readonly gateway: 'Mock' | 'Stripe';
  readonly result: PaymentResult;
  readonly processedAt: string;
}

export interface PaymentProcessedEvent {
  readonly eventId: string;
  readonly eventType: typeof PAYMENT_PROCESSED_EVENT_TYPE;
  readonly occurredAt: string;
  readonly aggregateId: string;
  readonly aggregateType: 'Payment';
  readonly tenantId: string;
  readonly version: number;
  readonly payload: PaymentProcessedEventPayload;
}
