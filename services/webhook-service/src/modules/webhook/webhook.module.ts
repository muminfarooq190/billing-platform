import { Module } from '@nestjs/common';
import { TypeOrmModule } from '@nestjs/typeorm';
import { WebhookDeliveryQueue } from '../../jobs/webhook-delivery.queue';
import { WebhookSubscriptionEntity } from '../../entities/webhook-subscription.entity';
import { DeliveryLogModule } from '../delivery-log/delivery-log.module';
import { WebhookController } from './webhook.controller';
import { WebhookService } from './webhook.service';

@Module({
  imports: [TypeOrmModule.forFeature([WebhookSubscriptionEntity]), DeliveryLogModule],
  controllers: [WebhookController],
  providers: [WebhookService, WebhookDeliveryQueue],
  exports: [WebhookService, WebhookDeliveryQueue],
})
export class WebhookModule {}
