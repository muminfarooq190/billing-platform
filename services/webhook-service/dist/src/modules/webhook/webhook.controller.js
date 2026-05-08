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
const tenant_context_1 = require("../../common/tenant-context");
const webhook_delivery_queue_1 = require("../../jobs/webhook-delivery.queue");
const delivery_log_service_1 = require("../delivery-log/delivery-log.service");
const create_webhook_subscription_dto_1 = require("./dto/create-webhook-subscription.dto");
const webhook_service_1 = require("./webhook.service");
let WebhookController = class WebhookController {
    constructor(webhookService, deliveryLogService, deliveryQueue, tenantContext) {
        this.webhookService = webhookService;
        this.deliveryLogService = deliveryLogService;
        this.deliveryQueue = deliveryQueue;
        this.tenantContext = tenantContext;
    }
    async listDeliveries(request, page = '1', pageSize = '20') {
        const tenantId = this.tenantContext.getTenantId(request);
        return this.deliveryLogService.list(tenantId, Number(page), Number(pageSize));
    }
    async getDelivery(request, id) {
        const tenantId = this.tenantContext.getTenantId(request);
        const log = await this.deliveryLogService.getById(id, tenantId);
        if (!log)
            throw new common_1.NotFoundException();
        return log;
    }
    async replay(request, id) {
        const tenantId = this.tenantContext.getTenantId(request);
        const log = await this.deliveryLogService.getById(id, tenantId);
        if (!log)
            throw new common_1.NotFoundException();
        const subscription = await this.webhookService.getById(log.webhookSubscriptionId);
        if (!subscription || subscription.tenantId !== tenantId || !subscription.isActive) {
            throw new common_1.UnauthorizedException('Webhook subscription is not active for this tenant.');
        }
        await this.deliveryQueue.enqueue({
            webhookSubscriptionId: log.webhookSubscriptionId,
            deliveryLogId: log.id,
            eventType: log.eventType,
            payload: log.payload,
            attemptNumber: 1,
        }, 0);
        return { replayRequested: true, id };
    }
    async listSubscriptions(request) {
        const tenantId = this.tenantContext.getTenantId(request);
        return this.webhookService.listSubscriptions(tenantId);
    }
    async createSubscription(request, body) {
        const tenantId = this.tenantContext.getTenantId(request);
        if (body.tenantId !== tenantId) {
            throw new common_1.UnauthorizedException('tenantId in payload must match x-tenant-id header.');
        }
        return this.webhookService.createSubscription(body);
    }
    async remove(request, id) {
        const tenantId = this.tenantContext.getTenantId(request);
        await this.webhookService.deactivateSubscription(id, tenantId);
    }
    eventCatalog() {
        return {
            events: [
                { key: 'billing.invoice.created', group: 'Billing', description: 'New invoice generated for tenant subscription or booking.' },
                { key: 'billing.invoice.paid', group: 'Billing', description: 'Invoice marked paid via Stripe checkout or manual reconciliation.' },
                { key: 'billing.invoice.failed', group: 'Billing', description: 'Invoice payment attempt failed (declined card, insufficient funds, etc.).' },
                { key: 'billing.subscription.created', group: 'Billing', description: 'Tenant subscription activated.' },
                { key: 'billing.subscription.cancelled', group: 'Billing', description: 'Tenant subscription cancelled or expired.' },
                { key: 'travel.inquiry.created', group: 'Inquiries', description: 'New inquiry submitted (web form, email, partner channel).' },
                { key: 'travel.inquiry.assigned', group: 'Inquiries', description: 'Inquiry assigned to a consultant.' },
                { key: 'travel.inquiry.qualified', group: 'Inquiries', description: 'Inquiry marked qualified for quotation.' },
                { key: 'travel.quotation.created', group: 'Quotations', description: 'New quotation drafted from inquiry.' },
                { key: 'travel.quotation.sent', group: 'Quotations', description: 'Quotation emailed to customer with public share link.' },
                { key: 'travel.quotation.accepted', group: 'Quotations', description: 'Customer accepted quotation (public token or portal action).' },
                { key: 'travel.quotation.rejected', group: 'Quotations', description: 'Customer rejected quotation.' },
                { key: 'travel.quotation.expired', group: 'Quotations', description: 'Quotation passed valid-until date without acceptance.' },
                { key: 'travel.booking.confirmed', group: 'Bookings', description: 'Booking created from accepted quotation.' },
                { key: 'travel.booking.payment_received', group: 'Bookings', description: 'Booking payment milestone marked paid.' },
                { key: 'travel.booking.cancelled', group: 'Bookings', description: 'Booking cancelled before travel.' },
                { key: 'communication.notification.sent', group: 'Communication', description: 'Outbound notification dispatched (email/SMS/in-app).' },
                { key: 'communication.notification.delivered', group: 'Communication', description: 'Notification confirmed delivered by provider.' },
                { key: 'communication.notification.failed', group: 'Communication', description: 'Notification dispatch or delivery failed.' },
                { key: 'identity.user.invited', group: 'Identity', description: 'Team member invitation sent.' },
                { key: 'identity.user.activated', group: 'Identity', description: 'Invited user accepted invitation.' },
            ],
        };
    }
};
exports.WebhookController = WebhookController;
__decorate([
    (0, common_1.Get)('deliveries'),
    __param(0, (0, common_1.Req)()),
    __param(1, (0, common_1.Query)('page')),
    __param(2, (0, common_1.Query)('page_size')),
    __metadata("design:type", Function),
    __metadata("design:paramtypes", [Object, Object, Object]),
    __metadata("design:returntype", Promise)
], WebhookController.prototype, "listDeliveries", null);
__decorate([
    (0, common_1.Get)('deliveries/:id'),
    __param(0, (0, common_1.Req)()),
    __param(1, (0, common_1.Param)('id')),
    __metadata("design:type", Function),
    __metadata("design:paramtypes", [Object, String]),
    __metadata("design:returntype", Promise)
], WebhookController.prototype, "getDelivery", null);
__decorate([
    (0, common_1.Post)('deliveries/:id/replay'),
    __param(0, (0, common_1.Req)()),
    __param(1, (0, common_1.Param)('id')),
    __metadata("design:type", Function),
    __metadata("design:paramtypes", [Object, String]),
    __metadata("design:returntype", Promise)
], WebhookController.prototype, "replay", null);
__decorate([
    (0, common_1.Get)('subscriptions'),
    __param(0, (0, common_1.Req)()),
    __metadata("design:type", Function),
    __metadata("design:paramtypes", [Object]),
    __metadata("design:returntype", Promise)
], WebhookController.prototype, "listSubscriptions", null);
__decorate([
    (0, common_1.Post)('subscriptions'),
    __param(0, (0, common_1.Req)()),
    __param(1, (0, common_1.Body)()),
    __metadata("design:type", Function),
    __metadata("design:paramtypes", [Object, create_webhook_subscription_dto_1.CreateWebhookSubscriptionDto]),
    __metadata("design:returntype", Promise)
], WebhookController.prototype, "createSubscription", null);
__decorate([
    (0, common_1.Delete)('subscriptions/:id'),
    __param(0, (0, common_1.Req)()),
    __param(1, (0, common_1.Param)('id')),
    __metadata("design:type", Function),
    __metadata("design:paramtypes", [Object, String]),
    __metadata("design:returntype", Promise)
], WebhookController.prototype, "remove", null);
__decorate([
    (0, common_1.Get)('event-catalog'),
    __metadata("design:type", Function),
    __metadata("design:paramtypes", []),
    __metadata("design:returntype", Object)
], WebhookController.prototype, "eventCatalog", null);
exports.WebhookController = WebhookController = __decorate([
    (0, common_1.Controller)('webhooks'),
    __metadata("design:paramtypes", [webhook_service_1.WebhookService,
        delivery_log_service_1.DeliveryLogService,
        webhook_delivery_queue_1.WebhookDeliveryQueue,
        tenant_context_1.TenantContext])
], WebhookController);
//# sourceMappingURL=webhook.controller.js.map