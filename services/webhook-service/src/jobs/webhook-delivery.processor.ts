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
      const retryAfterHeader = axios.isAxiosError(error) ? error.response?.headers?.['retry-after'] : undefined;

      if (this.shouldMarkDeadImmediately(statusCode)) {
        await this.deliveryLogService.markResult(payload.deliveryLogId, 'Dead', payload.attemptNumber, statusCode, responseBody);
        return;
      }

      if (payload.attemptNumber >= 5) {
        await this.deliveryLogService.markResult(payload.deliveryLogId, 'Dead', payload.attemptNumber, statusCode, responseBody);
        return;
      }

      await this.deliveryLogService.markResult(payload.deliveryLogId, 'Failed', payload.attemptNumber, statusCode, responseBody);
      const nextAttempt = payload.attemptNumber + 1;
      const delay = this.getRetryDelayMs(nextAttempt, statusCode, retryAfterHeader);
      await this.deliveryQueue.enqueue(
        {
          ...payload,
          attemptNumber: nextAttempt,
        },
        delay,
      );
    }
  }

  private shouldMarkDeadImmediately(statusCode: number | null): boolean {
    if (statusCode === null) {
      return false;
    }

    if (statusCode === 408 || statusCode === 409 || statusCode === 425 || statusCode === 429) {
      return false;
    }

    return statusCode >= 400 && statusCode < 500;
  }

  private getRetryDelayMs(attempt: number, statusCode: number | null, retryAfterHeader?: string | string[]): number {
    if (statusCode === 429) {
      const retryAfterMs = this.parseRetryAfterMs(retryAfterHeader);
      if (retryAfterMs !== null) {
        return retryAfterMs;
      }
    }

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

  private parseRetryAfterMs(value?: string | string[]): number | null {
    const header = Array.isArray(value) ? value[0] : value;
    if (!header) {
      return null;
    }

    const seconds = Number.parseInt(header, 10);
    if (!Number.isNaN(seconds) && seconds >= 0) {
      return seconds * 1000;
    }

    const when = Date.parse(header);
    if (!Number.isNaN(when)) {
      return Math.max(0, when - Date.now());
    }

    return null;
  }
}
