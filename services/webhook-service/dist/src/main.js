"use strict";
var __decorate = (this && this.__decorate) || function (decorators, target, key, desc) {
    var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
    if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
    else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
    return c > 3 && r && Object.defineProperty(target, key, r), r;
};
Object.defineProperty(exports, "__esModule", { value: true });
require("reflect-metadata");
const common_1 = require("@nestjs/common");
const health_controller_1 = require("./common/health.controller");
const core_1 = require("@nestjs/core");
const typeorm_1 = require("@nestjs/typeorm");
const bullmq_1 = require("bullmq");
const billing_events_consumer_1 = require("./consumers/billing-events.consumer");
const rabbitmq_events_listener_1 = require("./consumers/rabbitmq-events.listener");
const webhook_delivery_processor_1 = require("./jobs/webhook-delivery.processor");
const delivery_log_module_1 = require("./modules/delivery-log/delivery-log.module");
const webhook_module_1 = require("./modules/webhook/webhook.module");
const webhook_signer_service_1 = require("./signing/webhook-signer.service");
const webhook_delivery_log_entity_1 = require("./entities/webhook-delivery-log.entity");
const webhook_subscription_entity_1 = require("./entities/webhook-subscription.entity");
const _0000000000001_initial_schema_1 = require("./migrations/0000000000001-initial-schema");
const _0000000000002_add_event_fingerprint_1 = require("./migrations/0000000000002-add-event-fingerprint");
let AppModule = class AppModule {
};
AppModule = __decorate([
    (0, common_1.Module)({
        imports: [
            typeorm_1.TypeOrmModule.forRoot({
                type: 'postgres',
                url: process.env.DATABASE_URL,
                entities: [webhook_delivery_log_entity_1.WebhookDeliveryLogEntity, webhook_subscription_entity_1.WebhookSubscriptionEntity],
                migrations: [_0000000000001_initial_schema_1.InitialSchema0000000000001, _0000000000002_add_event_fingerprint_1.AddEventFingerprint0000000000002],
                synchronize: false,
            }),
            webhook_module_1.WebhookModule,
            delivery_log_module_1.DeliveryLogModule,
        ],
        controllers: [health_controller_1.HealthController],
        providers: [webhook_signer_service_1.WebhookSignerService, webhook_delivery_processor_1.WebhookDeliveryProcessor, billing_events_consumer_1.BillingEventsConsumer, rabbitmq_events_listener_1.RabbitMqEventsListener],
    })
], AppModule);
async function bootstrap() {
    const app = await core_1.NestFactory.create(AppModule);
    app.useGlobalPipes(new common_1.ValidationPipe({ whitelist: true, transform: true, forbidNonWhitelisted: true }));
    const dataSource = app.get((0, typeorm_1.getDataSourceToken)());
    await dataSource.runMigrations();
    const processor = app.get(webhook_delivery_processor_1.WebhookDeliveryProcessor);
    const redisUrl = process.env.REDIS_URL ?? 'redis://redis:6379';
    const worker = new bullmq_1.Worker('webhook-delivery', async (job) => processor.process(job), {
        connection: { url: redisUrl },
    });
    const port = Number.parseInt(process.env.PORT ?? '3000', 10);
    await app.listen(port);
}
void bootstrap();
//# sourceMappingURL=main.js.map