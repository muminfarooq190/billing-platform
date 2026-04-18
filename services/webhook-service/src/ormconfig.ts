import { DataSource } from 'typeorm';
import { WebhookDeliveryLogEntity } from './entities/webhook-delivery-log.entity';
import { WebhookSubscriptionEntity } from './entities/webhook-subscription.entity';
import { InitialSchema0000000000001 } from './migrations/0000000000001-initial-schema';
import { AddEventFingerprint0000000000002 } from './migrations/0000000000002-add-event-fingerprint';

export default new DataSource({
  type: 'postgres',
  url: process.env.DATABASE_URL,
  entities: [WebhookDeliveryLogEntity, WebhookSubscriptionEntity],
  migrations: [InitialSchema0000000000001, AddEventFingerprint0000000000002],
  synchronize: false,
});
