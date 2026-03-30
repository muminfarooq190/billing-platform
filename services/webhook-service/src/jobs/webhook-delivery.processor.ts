import axios from 'axios';
import { Injectable, Logger } from '@nestjs/common';
import { Job } from 'bullmq';
import { DeliveryLogService } from '../modules/delivery-log/delivery-log.service';
import { WebhookService } from '../modules/webhook/webhook.service';
import { WebhookSignerService } from '../signing/webhook-signer.service';
import { WebhookDeliveryJob } from './webhook-delivery.job';
import { WebhookDeliveryQueue } from './webhook-delivery.queue';

@Injectable()
export class WebhookDeliveryProcessor {
  private readonly logger = new Logger(WebhookDeliveryProcessor.name);

  public constructor(
    private readonly webhookService: WebhookService,
    private readonly signerService: WebhookSignerService,
    private readonly deliveryLogService: DeliveryLogService,
    private readonly deliveryQueue: WebhookDeliveryQueue,
  ) {}

  public async process(job: Job<WebhookDeliveryJob>): Promise<void> {
    const payload = job.data;
    const subscription = await this.webhookService.getById(payload.webhookSubscriptionId);
    if (!subscription) {
      this.logger.warn(`Subscription ${payload.webhookSubscriptionId} not found.`);
      return;
    }

    const body = JSON.stringify(payload.payload);
    const timestamp = Math.floor(Date.now() / 1000).toString();
    const signature = this.signerService.sign(subscription.signingSecret, timestamp, body);

    try {
      const response = await axios.post(subscription.targetUrl, payload.payload, {
        headers: {
          'X-Webhook-Signature': signature,
          'X-Webhook-Timestamp': timestamp,
          'X-Webhook-Event': payload.eventType,
          'X-Webhook-Delivery-Id': payload.deliveryLogId,
        },
        timeout: 10_000,
      });

      await this.deliveryLogService.markResult(payload.deliveryLogId, 'Delivered', payload.attemptNumber, response.status, JSON.stringify(response.data));
    } catch (error) {
      const statusCode = axios.isAxiosError(error) ? (error.response?.status ?? null) : null;
      const responseBody = axios.isAxiosError(error) ? JSON.stringify(error.response?.data ?? null) : String(error);

      if (payload.attemptNumber >= 5) {
        await this.deliveryLogService.markResult(payload.deliveryLogId, 'Dead', payload.attemptNumber, statusCode, responseBody);
        return;
      }

      await this.deliveryLogService.markResult(payload.deliveryLogId, 'Failed', payload.attemptNumber, statusCode, responseBody);
      const nextAttempt = payload.attemptNumber + 1;
      await this.deliveryQueue.enqueue(
        {
          ...payload,
          attemptNumber: nextAttempt,
        },
        this.getRetryDelayMs(nextAttempt),
      );
    }
  }

  private getRetryDelayMs(attempt: number): number {
    return (
      {
        1: 0,
        2: 30_000,
        3: 5 * 60_000,
        4: 30 * 60_000,
        5: 2 * 60 * 60_000,
      }[attempt] ?? 0
    );
  }
}
