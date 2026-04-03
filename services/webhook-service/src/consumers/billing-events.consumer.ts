import { Injectable } from '@nestjs/common';
import { DeliveryLogService } from '../modules/delivery-log/delivery-log.service';
import { WebhookService } from '../modules/webhook/webhook.service';
import { WebhookDeliveryQueue } from '../jobs/webhook-delivery.queue';

export interface BillingDomainEventEnvelope {
  tenantId: string;
  eventType: string;
  payload: Record<string, unknown>;
}

@Injectable()
export class BillingEventsConsumer {
  public constructor(
    private readonly webhookService: WebhookService,
    private readonly deliveryLogService: DeliveryLogService,
    private readonly deliveryQueue: WebhookDeliveryQueue,
  ) {}

  public async handleBillingEvent(event: BillingDomainEventEnvelope): Promise<void> {
    const subscriptions = await this.webhookService.listByTenantAndEvent(event.tenantId, event.eventType);
    for (const subscription of subscriptions) {
      const log = await this.deliveryLogService.createPending(subscription.id, event.eventType, event.payload);
      await this.deliveryQueue.enqueue({
        webhookSubscriptionId: subscription.id,
        deliveryLogId: log.id,
        eventType: event.eventType,
        payload: event.payload,
        attemptNumber: 1,
      });
    }
  }
}
