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
exports.WebhookController = void 0;
const common_1 = require("@nestjs/common");
const webhook_delivery_queue_1 = require("../../jobs/webhook-delivery.queue");
const delivery_log_service_1 = require("../delivery-log/delivery-log.service");
const webhook_service_1 = require("./webhook.service");
let WebhookController = class WebhookController {
    constructor(webhookService, deliveryLogService, deliveryQueue) {
        this.webhookService = webhookService;
        this.deliveryLogService = deliveryLogService;
        this.deliveryQueue = deliveryQueue;
    }
    async listDeliveries(page = '1', pageSize = '20') {
        return this.deliveryLogService.list(Number(page), Number(pageSize));
    }
    async getDelivery(id) {
        return this.deliveryLogService.getById(id);
    }
    async replay(id) {
        const log = await this.deliveryLogService.getById(id);
        if (log) {
            await this.deliveryQueue.enqueue({
                webhookSubscriptionId: log.webhookSubscriptionId,
                deliveryLogId: log.id,
                eventType: log.eventType,
                payload: log.payload,
                attemptNumber: 1,
            }, 0);
        }
        return { replayRequested: true, id };
    }
    async listSubscriptions() {
        return this.webhookService.listSubscriptions();
    }
    async createSubscription(body) {
        return this.webhookService.createSubscription(body);
    }
    async remove(id) {
        await this.webhookService.deactivateSubscription(id);
    }
};
exports.WebhookController = WebhookController;
__decorate([
    (0, common_1.Get)('deliveries'),
    __param(0, (0, common_1.Query)('page')),
    __param(1, (0, common_1.Query)('page_size')),
    __metadata("design:type", Function),
    __metadata("design:paramtypes", [Object, Object]),
    __metadata("design:returntype", Promise)
], WebhookController.prototype, "listDeliveries", null);
__decorate([
    (0, common_1.Get)('deliveries/:id'),
    __param(0, (0, common_1.Param)('id')),
    __metadata("design:type", Function),
    __metadata("design:paramtypes", [String]),
    __metadata("design:returntype", Promise)
], WebhookController.prototype, "getDelivery", null);
__decorate([
    (0, common_1.Post)('deliveries/:id/replay'),
    __param(0, (0, common_1.Param)('id')),
    __metadata("design:type", Function),
    __metadata("design:paramtypes", [String]),
    __metadata("design:returntype", Promise)
], WebhookController.prototype, "replay", null);
__decorate([
    (0, common_1.Get)('subscriptions'),
    __metadata("design:type", Function),
    __metadata("design:paramtypes", []),
    __metadata("design:returntype", Promise)
], WebhookController.prototype, "listSubscriptions", null);
__decorate([
    (0, common_1.Post)('subscriptions'),
    __param(0, (0, common_1.Body)()),
    __metadata("design:type", Function),
    __metadata("design:paramtypes", [Object]),
    __metadata("design:returntype", Promise)
], WebhookController.prototype, "createSubscription", null);
__decorate([
    (0, common_1.Delete)('subscriptions/:id'),
    __param(0, (0, common_1.Param)('id')),
    __metadata("design:type", Function),
    __metadata("design:paramtypes", [String]),
    __metadata("design:returntype", Promise)
], WebhookController.prototype, "remove", null);
exports.WebhookController = WebhookController = __decorate([
    (0, common_1.Controller)('webhooks'),
    __metadata("design:paramtypes", [webhook_service_1.WebhookService,
        delivery_log_service_1.DeliveryLogService,
        webhook_delivery_queue_1.WebhookDeliveryQueue])
], WebhookController);
//# sourceMappingURL=webhook.controller.js.map