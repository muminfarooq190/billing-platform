import { Injectable } from '@nestjs/common';
import { InjectRepository } from '@nestjs/typeorm';
import { Repository } from 'typeorm';
import { WebhookSubscriptionEntity } from '../../entities/webhook-subscription.entity';
import { CreateWebhookSubscriptionDto } from './dto/create-webhook-subscription.dto';

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

  public async createSubscription(input: CreateWebhookSubscriptionDto): Promise<WebhookSubscriptionEntity> {
    const entity = this.subscriptionRepository.create({
      tenantId: input.tenantId,
      targetUrl: input.targetUrl,
      events: [...new Set(input.events.map((eventName) => eventName.trim()).filter(Boolean))],
      signingSecret: input.signingSecret,
      isActive: input.isActive ?? true,
    });

    return this.subscriptionRepository.save(entity);
  }

  public async deactivateSubscription(id: string): Promise<void> {
    await this.subscriptionRepository.update(id, { isActive: false });
  }

  public async getById(id: string): Promise<WebhookSubscriptionEntity | null> {
    return this.subscriptionRepository.findOne({ where: { id } });
  }
}
