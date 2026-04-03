export interface WebhookDeliveryJob {
  webhookSubscriptionId: string;
  deliveryLogId: string;
  eventType: string;
  payload: Record<string, unknown>;
  attemptNumber: number;
}
