"use strict";
var __decorate = (this && this.__decorate) || function (decorators, target, key, desc) {
    var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
    if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
    else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
    return c > 3 && r && Object.defineProperty(target, key, r), r;
};
Object.defineProperty(exports, "__esModule", { value: true });
exports.DeliveryLogModule = void 0;
const common_1 = require("@nestjs/common");
const typeorm_1 = require("@nestjs/typeorm");
const webhook_delivery_log_entity_1 = require("../../entities/webhook-delivery-log.entity");
const webhook_subscription_entity_1 = require("../../entities/webhook-subscription.entity");
const delivery_log_service_1 = require("./delivery-log.service");
let DeliveryLogModule = class DeliveryLogModule {
};
exports.DeliveryLogModule = DeliveryLogModule;
exports.DeliveryLogModule = DeliveryLogModule = __decorate([
    (0, common_1.Module)({
        imports: [typeorm_1.TypeOrmModule.forFeature([webhook_delivery_log_entity_1.WebhookDeliveryLogEntity, webhook_subscription_entity_1.WebhookSubscriptionEntity])],
        providers: [delivery_log_service_1.DeliveryLogService],
        exports: [delivery_log_service_1.DeliveryLogService],
    })
], DeliveryLogModule);
//# sourceMappingURL=delivery-log.module.js.map