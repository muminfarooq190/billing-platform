import { Injectable } from '@nestjs/common';
import { InjectRepository } from '@nestjs/typeorm';
import { Repository } from 'typeorm';
import { WebhookDeliveryLogEntity } from '../../entities/webhook-delivery-log.entity';
import { WebhookSubscriptionEntity } from '../../entities/webhook-subscription.entity';

@Injectable()
export class DeliveryLogService {
  public constructor(
    @InjectRepository(WebhookDeliveryLogEntity)
    private readonly deliveryLogRepository: Repository<WebhookDeliveryLogEntity>,
    @InjectRepository(WebhookSubscriptionEntity)
    private readonly subscriptionRepository: Repository<WebhookSubscriptionEntity>,
  ) {}

  public async list(tenantId: string, page: number, pageSize: number): Promise<WebhookDeliveryLogEntity[]> {
    const subscriptions = await this.subscriptionRepository.find({ where: { tenantId } });
    const subscriptionIds = subscriptions.map((x) => x.id);
    if (subscriptionIds.length === 0) return [];

    return this.deliveryLogRepository.find({
      where: subscriptionIds.map((id) => ({ webhookSubscriptionId: id })),
      order: { createdAt: 'DESC' },
      skip: (page - 1) * pageSize,
      take: pageSize,
    });
  }

  public async getById(id: string, tenantId?: string): Promise<WebhookDeliveryLogEntity | null> {
    const log = await this.deliveryLogRepository.findOne({ where: { id } });
    if (!log || !tenantId) return log;

    const subscription = await this.subscriptionRepository.findOne({ where: { id: log.webhookSubscriptionId, tenantId } });
    return subscription ? log : null;
  }

  public async getByFingerprint(webhookSubscriptionId: string, eventFingerprint: string): Promise<WebhookDeliveryLogEntity | null> {
    return this.deliveryLogRepository.findOne({ where: { webhookSubscriptionId, eventFingerprint } });
  }

  public async createPending(
    webhookSubscriptionId: string,
    eventType: string,
    payload: Record<string, unknown>,
    eventFingerprint?: string,
  ): Promise<WebhookDeliveryLogEntity> {
    const entity = this.deliveryLogRepository.create({
      webhookSubscriptionId,
      eventType,
      eventFingerprint: eventFingerprint ?? null,
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
