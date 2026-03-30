export const INVOICE_CREATED_EVENT_TYPE = 'billing.invoice.created' as const;

export type InvoiceStatus = 'Draft' | 'Issued' | 'Paid' | 'Overdue' | 'Void';

export interface InvoiceLineItemPayload {
  readonly description: string;
  readonly quantity: number;
  readonly unitPriceAmount: string;
  readonly unitPriceCurrency: string;
  readonly lineTotalAmount: string;
}

export interface InvoiceCreatedEventPayload {
  readonly invoiceId: string;
  readonly subscriptionId: string;
  readonly tenantId: string;
  readonly status: InvoiceStatus;
  readonly subtotalAmount: string;
  readonly subtotalCurrency: string;
  readonly taxAmount: string;
  readonly taxCurrency: string;
  readonly totalAmount: string;
  readonly totalCurrency: string;
  readonly dueDate: string;
  readonly issuedAt: string | null;
  readonly lineItems: ReadonlyArray<InvoiceLineItemPayload>;
}

export interface InvoiceCreatedEvent {
  readonly eventId: string;
  readonly eventType: typeof INVOICE_CREATED_EVENT_TYPE;
  readonly occurredAt: string;
  readonly aggregateId: string;
  readonly aggregateType: 'Invoice';
  readonly tenantId: string;
  readonly version: number;
  readonly payload: InvoiceCreatedEventPayload;
}
