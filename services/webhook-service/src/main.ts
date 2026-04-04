import 'reflect-metadata';
import { Module } from '@nestjs/common';
import { HealthController } from './common/health.controller';
import { NestFactory } from '@nestjs/core';
import { TypeOrmModule } from '@nestjs/typeorm';
import { Worker } from 'bullmq';
import { BillingEventsConsumer } from './consumers/billing-events.consumer';
import { RabbitMqEventsListener } from './consumers/rabbitmq-events.listener';
import { WebhookDeliveryProcessor } from './jobs/webhook-delivery.processor';
import { DeliveryLogModule } from './modules/delivery-log/delivery-log.module';
import { WebhookModule } from './modules/webhook/webhook.module';
import { WebhookSignerService } from './signing/webhook-signer.service';
import { WebhookDeliveryLogEntity } from './entities/webhook-delivery-log.entity';
import { WebhookSubscriptionEntity } from './entities/webhook-subscription.entity';

@Module({
  imports: [
    TypeOrmModule.forRoot({
      type: 'postgres',
      url: process.env.DATABASE_URL,
      entities: [WebhookDeliveryLogEntity, WebhookSubscriptionEntity],
      synchronize: true,
    }),
    WebhookModule,
    DeliveryLogModule,
  ],
  controllers: [HealthController],
  providers: [WebhookSignerService, WebhookDeliveryProcessor, BillingEventsConsumer, RabbitMqEventsListener],
})
class AppModule {}

async function bootstrap(): Promise<void> {
  const app = await NestFactory.create(AppModule);
  const processor = app.get(WebhookDeliveryProcessor);

  const redisUrl = process.env.REDIS_URL ?? 'redis://redis:6379';
  // eslint-disable-next-line @typescript-eslint/no-unused-vars
  const worker = new Worker('webhook-delivery', async (job) => processor.process(job), {
    connection: { url: redisUrl },
  });

  const port = Number.parseInt(process.env.PORT ?? '3000', 10);
  await app.listen(port);
}

void bootstrap();
