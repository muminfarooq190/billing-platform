import { Injectable } from '@nestjs/common';
import { InjectRepository } from '@nestjs/typeorm';
import { Repository } from 'typeorm';
import { WebhookDeliveryLogEntity } from '../../entities/webhook-delivery-log.entity';

@Injectable()
export class DeliveryLogService {
  public constructor(
    @InjectRepository(WebhookDeliveryLogEntity)
    private readonly deliveryLogRepository: Repository<WebhookDeliveryLogEntity>,
  ) {}

  public async list(page: number, pageSize: number): Promise<WebhookDeliveryLogEntity[]> {
    return this.deliveryLogRepository.find({
      order: { createdAt: 'DESC' },
      skip: (page - 1) * pageSize,
      take: pageSize,
    });
  }

  public async getById(id: string): Promise<WebhookDeliveryLogEntity | null> {
    return this.deliveryLogRepository.findOne({ where: { id } });
  }

  public async createPending(webhookSubscriptionId: string, eventType: string, payload: Record<string, unknown>): Promise<WebhookDeliveryLogEntity> {
    const entity = this.deliveryLogRepository.create({
      webhookSubscriptionId,
      eventType,
      payload,
      status: 'Pending',
      attemptCount: 0,
      lastAttemptAt: null,
      responseStatusCode: null,
      responseBody: null,
    });

    return this.deliveryLogRepository.save(entity);
  }

  public async markResult(
    id: string,
    status: 'Delivered' | 'Failed' | 'Dead',
    attemptCount: number,
    responseStatusCode: number | null,
    responseBody: string | null,
  ): Promise<void> {
    await this.deliveryLogRepository.update(id, {
      status,
      attemptCount,
      responseStatusCode,
      responseBody,
      lastAttemptAt: new Date(),
    });
  }
}
