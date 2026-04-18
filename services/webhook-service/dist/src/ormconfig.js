"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const typeorm_1 = require("typeorm");
const webhook_delivery_log_entity_1 = require("./entities/webhook-delivery-log.entity");
const webhook_subscription_entity_1 = require("./entities/webhook-subscription.entity");
const _0000000000001_initial_schema_1 = require("./migrations/0000000000001-initial-schema");
const _0000000000002_add_event_fingerprint_1 = require("./migrations/0000000000002-add-event-fingerprint");
exports.default = new typeorm_1.DataSource({
    type: 'postgres',
    url: process.env.DATABASE_URL,
    entities: [webhook_delivery_log_entity_1.WebhookDeliveryLogEntity, webhook_subscription_entity_1.WebhookSubscriptionEntity],
    migrations: [_0000000000001_initial_schema_1.InitialSchema0000000000001, _0000000000002_add_event_fingerprint_1.AddEventFingerprint0000000000002],
    synchronize: false,
});
//# sourceMappingURL=ormconfig.js.map