"use strict";
var __decorate = (this && this.__decorate) || function (decorators, target, key, desc) {
    var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
    if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
    else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
    return c > 3 && r && Object.defineProperty(target, key, r), r;
};
Object.defineProperty(exports, "__esModule", { value: true });
exports.WebhookModule = void 0;
const common_1 = require("@nestjs/common");
const typeorm_1 = require("@nestjs/typeorm");
const webhook_delivery_queue_1 = require("../../jobs/webhook-delivery.queue");
const webhook_subscription_entity_1 = require("../../entities/webhook-subscription.entity");
const delivery_log_module_1 = require("../delivery-log/delivery-log.module");
const webhook_controller_1 = require("./webhook.controller");
const webhook_service_1 = require("./webhook.service");
let WebhookModule = class WebhookModule {
};
exports.WebhookModule = WebhookModule;
exports.WebhookModule = WebhookModule = __decorate([
    (0, common_1.Module)({
        imports: [typeorm_1.TypeOrmModule.forFeature([webhook_subscription_entity_1.WebhookSubscriptionEntity]), delivery_log_module_1.DeliveryLogModule],
        controllers: [webhook_controller_1.WebhookController],
        providers: [webhook_service_1.WebhookService, webhook_delivery_queue_1.WebhookDeliveryQueue],
        exports: [webhook_service_1.WebhookService, webhook_delivery_queue_1.WebhookDeliveryQueue],
    })
], WebhookModule);
//# sourceMappingURL=webhook.module.js.map