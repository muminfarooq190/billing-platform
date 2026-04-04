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
var BillingEventsConsumer_1;
Object.defineProperty(exports, "__esModule", { value: true });
exports.BillingEventsConsumer = void 0;
const common_1 = require("@nestjs/common");
const delivery_log_service_1 = require("../modules/delivery-log/delivery-log.service");
const webhook_service_1 = require("../modules/webhook/webhook.service");
const webhook_delivery_queue_1 = require("../jobs/webhook-delivery.queue");
let BillingEventsConsumer = BillingEventsConsumer_1 = class BillingEventsConsumer {
    constructor(webhookService, deliveryLogService, deliveryQueue) {
        this.webhookService = webhookService;
        this.deliveryLogService = deliveryLogService;
        this.deliveryQueue = deliveryQueue;
        this.logger = new common_1.Logger(BillingEventsConsumer_1.name);
    }
    async handleEvent(event) {
        const subscriptions = await this.webhookService.listByTenantAndEvent(event.tenantId, event.eventType);
        if (subscriptions.length === 0) {
            this.logger.debug(`No webhook subscriptions matched ${event.eventType} for tenant ${event.tenantId}.`);
            return;
        }
        for (const subscription of subscriptions) {
            const log = await this.deliveryLogService.createPending(subscription.id, event.eventType, event.payload);
            await this.deliveryQueue.enqueue({
                webhookSubscriptionId: subscription.id,
                deliveryLogId: log.id,
                eventType: event.eventType,
                payload: event.payload,
                attemptNumber: 1,
            });
        }
    }
};
exports.BillingEventsConsumer = BillingEventsConsumer;
exports.BillingEventsConsumer = BillingEventsConsumer = BillingEventsConsumer_1 = __decorate([
    (0, common_1.Injectable)(),
    __metadata("design:paramtypes", [webhook_service_1.WebhookService,
        delivery_log_service_1.DeliveryLogService,
        webhook_delivery_queue_1.WebhookDeliveryQueue])
], BillingEventsConsumer);
//# sourceMappingURL=billing-events.consumer.js.map