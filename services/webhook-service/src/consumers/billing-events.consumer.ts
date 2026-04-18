import { createHash } from 'crypto';
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
      const fingerprint = this.buildFingerprint(subscription.id, event);
      const existing = await this.deliveryLogService.getByFingerprint(subscription.id, fingerprint);
      if (existing) {
        this.logger.debug(`Skipping duplicate webhook event ${event.eventType} for subscription ${subscription.id}.`);
        continue;
      }

      const log = await this.deliveryLogService.createPending(subscription.id, event.eventType, event.payload, fingerprint);
      await this.deliveryQueue.enqueue({
        webhookSubscriptionId: subscription.id,
        deliveryLogId: log.id,
        eventType: event.eventType,
        payload: event.payload,
        attemptNumber: 1,
      });
    }
  }

  private buildFingerprint(subscriptionId: string, event: DomainEventEnvelope): string {
    const payload = JSON.stringify(this.sortObject(event.payload));
    return createHash('sha256')
      .update(`${subscriptionId}|${event.tenantId}|${event.eventType}|${payload}`)
      .digest('hex');
  }

  private sortObject(value: unknown): unknown {
    if (Array.isArray(value)) {
      return value.map((item) => this.sortObject(item));
    }

    if (value && typeof value === 'object') {
      return Object.keys(value as Record<string, unknown>)
        .sort()
        .reduce<Record<string, unknown>>((accumulator, key) => {
          accumulator[key] = this.sortObject((value as Record<string, unknown>)[key]);
          return accumulator;
        }, {});
    }

    return value;
  }
}
