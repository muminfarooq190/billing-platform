import { Injectable } from '@nestjs/common';
import { Queue } from 'bullmq';
import { WebhookDeliveryJob } from './webhook-delivery.job';

@Injectable()
export class WebhookDeliveryQueue {
  private readonly queue: Queue<WebhookDeliveryJob>;

  public constructor() {
    const redisUrl = process.env.REDIS_URL ?? 'redis://redis:6379';
    this.queue = new Queue<WebhookDeliveryJob>('webhook-delivery', {
      connection: { url: redisUrl },
      defaultJobOptions: {
        removeOnComplete: false,
        removeOnFail: false,
      },
    });
  }

  public async enqueue(job: WebhookDeliveryJob, delayMs = 0): Promise<void> {
    await this.queue.add(`delivery-${job.deliveryLogId}-attempt-${job.attemptNumber}`, job, {
      delay: delayMs,
      jobId: `${job.deliveryLogId}-attempt-${job.attemptNumber}`,
    });
  }
}
