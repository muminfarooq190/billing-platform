"use strict";
var __decorate = (this && this.__decorate) || function (decorators, target, key, desc) {
    var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
    if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
    else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
    return c > 3 && r && Object.defineProperty(target, key, r), r;
};
var __metadata = (this && this.__metadata) || function (k, v) {
    if (typeof Reflect === "object" && typeof Reflect.metadata === "function") return Reflect.metadata(k, v);
};
Object.defineProperty(exports, "__esModule", { value: true });
exports.WebhookDeliveryLogEntity = void 0;
const typeorm_1 = require("typeorm");
let WebhookDeliveryLogEntity = class WebhookDeliveryLogEntity {
};
exports.WebhookDeliveryLogEntity = WebhookDeliveryLogEntity;
__decorate([
    (0, typeorm_1.PrimaryGeneratedColumn)('uuid'),
    __metadata("design:type", String)
], WebhookDeliveryLogEntity.prototype, "id", void 0);
__decorate([
    (0, typeorm_1.Column)({ name: 'webhook_subscription_id', type: 'uuid' }),
    __metadata("design:type", String)
], WebhookDeliveryLogEntity.prototype, "webhookSubscriptionId", void 0);
__decorate([
    (0, typeorm_1.Column)({ name: 'event_type', type: 'varchar', length: 200 }),
    __metadata("design:type", String)
], WebhookDeliveryLogEntity.prototype, "eventType", void 0);
__decorate([
    (0, typeorm_1.Column)({ name: 'event_fingerprint', type: 'varchar', length: 128, nullable: true }),
    __metadata("design:type", Object)
], WebhookDeliveryLogEntity.prototype, "eventFingerprint", void 0);
__decorate([
    (0, typeorm_1.Column)({ name: 'payload', type: 'jsonb' }),
    __metadata("design:type", Object)
], WebhookDeliveryLogEntity.prototype, "payload", void 0);
__decorate([
    (0, typeorm_1.Column)({ name: 'status', type: 'varchar', length: 50 }),
    __metadata("design:type", String)
], WebhookDeliveryLogEntity.prototype, "status", void 0);
__decorate([
    (0, typeorm_1.Column)({ name: 'attempt_count', type: 'int', default: 0 }),
    __metadata("design:type", Number)
], WebhookDeliveryLogEntity.prototype, "attemptCount", void 0);
__decorate([
    (0, typeorm_1.Column)({ name: 'last_attempt_at', type: 'timestamptz', nullable: true }),
    __metadata("design:type", Object)
], WebhookDeliveryLogEntity.prototype, "lastAttemptAt", void 0);
__decorate([
    (0, typeorm_1.Column)({ name: 'response_status_code', type: 'int', nullable: true }),
    __metadata("design:type", Object)
], WebhookDeliveryLogEntity.prototype, "responseStatusCode", void 0);
__decorate([
    (0, typeorm_1.Column)({ name: 'response_body', type: 'text', nullable: true }),
    __metadata("design:type", Object)
], WebhookDeliveryLogEntity.prototype, "responseBody", void 0);
__decorate([
    (0, typeorm_1.CreateDateColumn)({ name: 'created_at' }),
    __metadata("design:type", Date)
], WebhookDeliveryLogEntity.prototype, "createdAt", void 0);
__decorate([
    (0, typeorm_1.UpdateDateColumn)({ name: 'updated_at' }),
    __metadata("design:type", Date)
], WebhookDeliveryLogEntity.prototype, "updatedAt", void 0);
exports.WebhookDeliveryLogEntity = WebhookDeliveryLogEntity = __decorate([
    (0, typeorm_1.Entity)({ name: 'webhook_delivery_logs' })
], WebhookDeliveryLogEntity);
//# sourceMappingURL=webhook-delivery-log.entity.js.map