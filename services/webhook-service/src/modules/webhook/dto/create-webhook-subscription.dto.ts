export interface CreateWebhookSubscriptionDto {
  tenantId: string;
  targetUrl: string;
  events: string[];
  signingSecret: string;
  isActive?: boolean;
}
