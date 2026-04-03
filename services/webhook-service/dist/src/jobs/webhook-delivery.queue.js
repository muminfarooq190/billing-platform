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
exports.WebhookDeliveryQueue = void 0;
const common_1 = require("@nestjs/common");
const bullmq_1 = require("bullmq");
let WebhookDeliveryQueue = class WebhookDeliveryQueue {
    constructor() {
        const redisUrl = process.env.REDIS_URL ?? 'redis://redis:6379';
        this.queue = new bullmq_1.Queue('webhook-delivery', {
            connection: { url: redisUrl },
            defaultJobOptions: {
                removeOnComplete: false,
                removeOnFail: false,
            },
        });
    }
    async enqueue(job, delayMs = 0) {
        await this.queue.add(`delivery-${job.deliveryLogId}-attempt-${job.attemptNumber}`, job, {
            delay: delayMs,
            jobId: `${job.deliveryLogId}-attempt-${job.attemptNumber}`,
        });
    }
};
exports.WebhookDeliveryQueue = WebhookDeliveryQueue;
exports.WebhookDeliveryQueue = WebhookDeliveryQueue = __decorate([
    (0, common_1.Injectable)(),
    __metadata("design:paramtypes", [])
], WebhookDeliveryQueue);
//# sourceMappingURL=webhook-delivery.queue.js.map