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
exports.WebhookSubscriptionEntity = void 0;
const typeorm_1 = require("typeorm");
let WebhookSubscriptionEntity = class WebhookSubscriptionEntity {
};
exports.WebhookSubscriptionEntity = WebhookSubscriptionEntity;
__decorate([
    (0, typeorm_1.PrimaryGeneratedColumn)('uuid'),
    __metadata("design:type", String)
], WebhookSubscriptionEntity.prototype, "id", void 0);
__decorate([
    (0, typeorm_1.Column)({ name: 'tenant_id', type: 'uuid' }),
    __metadata("design:type", String)
], WebhookSubscriptionEntity.prototype, "tenantId", void 0);
__decorate([
    (0, typeorm_1.Column)({ name: 'target_url', type: 'varchar', length: 500 }),
    __metadata("design:type", String)
], WebhookSubscriptionEntity.prototype, "targetUrl", void 0);
__decorate([
    (0, typeorm_1.Column)({ name: 'events', type: 'text', array: true }),
    __metadata("design:type", Array)
], WebhookSubscriptionEntity.prototype, "events", void 0);
__decorate([
    (0, typeorm_1.Column)({ name: 'signing_secret', type: 'varchar', length: 255 }),
    __metadata("design:type", String)
], WebhookSubscriptionEntity.prototype, "signingSecret", void 0);
__decorate([
    (0, typeorm_1.Column)({ name: 'is_active', type: 'boolean', default: true }),
    __metadata("design:type", Boolean)
], WebhookSubscriptionEntity.prototype, "isActive", void 0);
__decorate([
    (0, typeorm_1.CreateDateColumn)({ name: 'created_at' }),
    __metadata("design:type", Date)
], WebhookSubscriptionEntity.prototype, "createdAt", void 0);
exports.WebhookSubscriptionEntity = WebhookSubscriptionEntity = __decorate([
    (0, typeorm_1.Entity)({ name: 'webhook_subscriptions' })
], WebhookSubscriptionEntity);
//# sourceMappingURL=webhook-subscription.entity.js.map