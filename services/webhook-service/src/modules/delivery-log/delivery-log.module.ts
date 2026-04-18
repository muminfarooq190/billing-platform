import { Module } from '@nestjs/common';
import { TypeOrmModule } from '@nestjs/typeorm';
import { WebhookDeliveryLogEntity } from '../../entities/webhook-delivery-log.entity';
import { WebhookSubscriptionEntity } from '../../entities/webhook-subscription.entity';
import { DeliveryLogService } from './delivery-log.service';

@Module({
  imports: [TypeOrmModule.forFeature([WebhookDeliveryLogEntity, WebhookSubscriptionEntity])],
  providers: [DeliveryLogService],
  exports: [DeliveryLogService],
})
export class DeliveryLogModule {}
