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
var __param = (this && this.__param) || function (paramIndex, decorator) {
    return function (target, key) { decorator(target, key, paramIndex); }
};
Object.defineProperty(exports, "__esModule", { value: true });
exports.DeliveryLogService = void 0;
const common_1 = require("@nestjs/common");
const typeorm_1 = require("@nestjs/typeorm");
const typeorm_2 = require("typeorm");
const webhook_delivery_log_entity_1 = require("../../entities/webhook-delivery-log.entity");
const webhook_subscription_entity_1 = require("../../entities/webhook-subscription.entity");
let DeliveryLogService = class DeliveryLogService {
    constructor(deliveryLogRepository, subscriptionRepository) {
        this.deliveryLogRepository = deliveryLogRepository;
        this.subscriptionRepository = subscriptionRepository;
    }
    async list(tenantId, page, pageSize) {
        const subscriptions = await this.subscriptionRepository.find({ where: { tenantId } });
        const subscriptionIds = subscriptions.map((x) => x.id);
        if (subscriptionIds.length === 0)
            return [];
        return this.deliveryLogRepository.find({
            where: subscriptionIds.map((id) => ({ webhookSubscriptionId: id })),
            order: { createdAt: 'DESC' },
            skip: (page - 1) * pageSize,
            take: pageSize,
        });
    }
    async getById(id, tenantId) {
        const log = await this.deliveryLogRepository.findOne({ where: { id } });
        if (!log || !tenantId)
            return log;
        const subscription = await this.subscriptionRepository.findOne({ where: { id: log.webhookSubscriptionId, tenantId } });
        return subscription ? log : null;
    }
    async getByFingerprint(webhookSubscriptionId, eventFingerprint) {
        return this.deliveryLogRepository.findOne({ where: { webhookSubscriptionId, eventFingerprint } });
    }
    async createPending(webhookSubscriptionId, eventType, payload, eventFingerprint) {
        const entity = this.deliveryLogRepository.create({
            webhookSubscriptionId,
            eventType,
            eventFingerprint: eventFingerprint ?? null,
            payload,
            status: 'Pending',
            attemptCount: 0,
            lastAttemptAt: null,
            responseStatusCode: null,
            responseBody: null,
        });
        return this.deliveryLogRepository.save(entity);
    }
    async markResult(id, status, attemptCount, responseStatusCode, responseBody) {
        await this.deliveryLogRepository.update(id, {
            status,
            attemptCount,
            responseStatusCode,
            responseBody,
            lastAttemptAt: new Date(),
        });
    }
};
exports.DeliveryLogService = DeliveryLogService;
exports.DeliveryLogService = DeliveryLogService = __decorate([
    (0, common_1.Injectable)(),
    __param(0, (0, typeorm_1.InjectRepository)(webhook_delivery_log_entity_1.WebhookDeliveryLogEntity)),
    __param(1, (0, typeorm_1.InjectRepository)(webhook_subscription_entity_1.WebhookSubscriptionEntity)),
    __metadata("design:paramtypes", [typeorm_2.Repository,
        typeorm_2.Repository])
], DeliveryLogService);
//# sourceMappingURL=delivery-log.service.js.map