import { Injectable, Logger } from '@nestjs/common';
import { DeliveryLogService } from '../modules/delivery-log/delivery-log.service';
import { WebhookService } from '../modules/webhook/webhook.service';
import { WebhookDeliveryQueue } from '../jobs/webhook-delivery.queue';
import { DomainEventEnvelope } from './domain-events.types';

@Injectable()
export class BillingEventsConsumer {
  private readonly logger = new Logger(BillingEventsConsumer.name);

  public constructor(
    private readonly webhookService: WebhookService,
    private readonly deliveryLogService: DeliveryLogService,
    private readonly deliveryQueue: WebhookDeliveryQueue,
  ) {}

  public async handleEvent(event: DomainEventEnvelope): Promise<void> {
    const subscriptions = await this.webhookService.listByTenantAndEvent(event.tenantId, event.eventType);

    if (subscriptions.length === 0) {
      this.logger.debug(`No webhook subscriptions matched ${event.eventType} for tenant ${event.tenantId}.`);
      return;
    }

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
