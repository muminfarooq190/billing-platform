import { Injectable } from '@nestjs/common';
import { InjectRepository } from '@nestjs/typeorm';
import { Repository } from 'typeorm';
import { WebhookSubscriptionEntity } from '../../entities/webhook-subscription.entity';

@Injectable()
export class WebhookService {
  public constructor(
    @InjectRepository(WebhookSubscriptionEntity)
    private readonly subscriptionRepository: Repository<WebhookSubscriptionEntity>,
  ) {}

  public async listSubscriptions(): Promise<WebhookSubscriptionEntity[]> {
    return this.subscriptionRepository.find({ where: { isActive: true }, order: { createdAt: 'DESC' } });
  }

  public async listByTenantAndEvent(tenantId: string, eventType: string): Promise<WebhookSubscriptionEntity[]> {
    const rows = await this.subscriptionRepository.find({ where: { tenantId, isActive: true } });
    return rows.filter((row) => row.events.includes(eventType));
  }

  public async deactivateSubscription(id: string): Promise<void> {
    await this.subscriptionRepository.update(id, { isActive: false });
  }

  public async getById(id: string): Promise<WebhookSubscriptionEntity | null> {
    return this.subscriptionRepository.findOne({ where: { id } });
  }
}
