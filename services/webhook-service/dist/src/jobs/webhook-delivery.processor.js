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
var WebhookDeliveryProcessor_1;
Object.defineProperty(exports, "__esModule", { value: true });
exports.WebhookDeliveryProcessor = void 0;
const axios_1 = require("axios");
const common_1 = require("@nestjs/common");
const delivery_log_service_1 = require("../modules/delivery-log/delivery-log.service");
const webhook_service_1 = require("../modules/webhook/webhook.service");
const webhook_signer_service_1 = require("../signing/webhook-signer.service");
const webhook_delivery_queue_1 = require("./webhook-delivery.queue");
let WebhookDeliveryProcessor = WebhookDeliveryProcessor_1 = class WebhookDeliveryProcessor {
    constructor(webhookService, signerService, deliveryLogService, deliveryQueue) {
        this.webhookService = webhookService;
        this.signerService = signerService;
        this.deliveryLogService = deliveryLogService;
        this.deliveryQueue = deliveryQueue;
        this.logger = new common_1.Logger(WebhookDeliveryProcessor_1.name);
    }
    async process(job) {
        const payload = job.data;
        const subscription = await this.webhookService.getById(payload.webhookSubscriptionId);
        if (!subscription) {
            this.logger.warn(`Subscription ${payload.webhookSubscriptionId} not found.`);
            return;
        }
        const body = JSON.stringify(payload.payload);
        const timestamp = Math.floor(Date.now() / 1000).toString();
        const signature = this.signerService.sign(subscription.signingSecret, timestamp, body);
        try {
            const response = await axios_1.default.post(subscription.targetUrl, payload.payload, {
                headers: {
                    'X-Webhook-Signature': signature,
                    'X-Webhook-Timestamp': timestamp,
                    'X-Webhook-Event': payload.eventType,
                    'X-Webhook-Delivery-Id': payload.deliveryLogId,
                },
                timeout: 10_000,
            });
            await this.deliveryLogService.markResult(payload.deliveryLogId, 'Delivered', payload.attemptNumber, response.status, JSON.stringify(response.data));
        }
        catch (error) {
            const statusCode = axios_1.default.isAxiosError(error) ? (error.response?.status ?? null) : null;
            const responseBody = axios_1.default.isAxiosError(error) ? JSON.stringify(error.response?.data ?? null) : String(error);
            if (payload.attemptNumber >= 5) {
                await this.deliveryLogService.markResult(payload.deliveryLogId, 'Dead', payload.attemptNumber, statusCode, responseBody);
                return;
            }
            await this.deliveryLogService.markResult(payload.deliveryLogId, 'Failed', payload.attemptNumber, statusCode, responseBody);
            const nextAttempt = payload.attemptNumber + 1;
            await this.deliveryQueue.enqueue({
                ...payload,
                attemptNumber: nextAttempt,
            }, this.getRetryDelayMs(nextAttempt));
        }
    }
    getRetryDelayMs(attempt) {
        return ({
            1: 0,
            2: 30_000,
            3: 5 * 60_000,
            4: 30 * 60_000,
            5: 2 * 60 * 60_000,
        }[attempt] ?? 0);
    }
};
exports.WebhookDeliveryProcessor = WebhookDeliveryProcessor;
exports.WebhookDeliveryProcessor = WebhookDeliveryProcessor = WebhookDeliveryProcessor_1 = __decorate([
    (0, common_1.Injectable)(),
    __metadata("design:paramtypes", [webhook_service_1.WebhookService,
        webhook_signer_service_1.WebhookSignerService,
        delivery_log_service_1.DeliveryLogService,
        webhook_delivery_queue_1.WebhookDeliveryQueue])
], WebhookDeliveryProcessor);
//# sourceMappingURL=webhook-delivery.processor.js.map