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
var RabbitMqEventsListener_1;
Object.defineProperty(exports, "__esModule", { value: true });
exports.RabbitMqEventsListener = void 0;
const common_1 = require("@nestjs/common");
const amqp_connection_manager_1 = require("amqp-connection-manager");
const billing_events_consumer_1 = require("./billing-events.consumer");
let RabbitMqEventsListener = RabbitMqEventsListener_1 = class RabbitMqEventsListener {
    constructor(billingEventsConsumer) {
        this.billingEventsConsumer = billingEventsConsumer;
        this.logger = new common_1.Logger(RabbitMqEventsListener_1.name);
    }
    async onModuleInit() {
        const bindings = [
            { exchange: 'billing.events', queue: process.env.BILLING_EVENTS_QUEUE ?? 'webhook-service.billing.events' },
            { exchange: 'identity.events', queue: process.env.IDENTITY_EVENTS_QUEUE ?? 'webhook-service.identity.events' },
            { exchange: 'travel.events', queue: process.env.TRAVEL_EVENTS_QUEUE ?? 'webhook-service.travel.events' },
        ];
        this.connection = (0, amqp_connection_manager_1.connect)([process.env.RABBITMQ_URL ?? 'amqp://guest:guest@rabbitmq:5672']);
        this.connection.on('connect', () => this.logger.log('Connected to RabbitMQ.'));
        this.connection.on('disconnect', (params) => this.logger.error(`Disconnected from RabbitMQ: ${params.err?.message ?? 'unknown error'}`));
        this.channelWrapper = this.connection.createChannel({
            setup: async (channel) => {
                for (const binding of bindings) {
                    await channel.assertExchange(binding.exchange, 'topic', { durable: true });
                    await channel.assertQueue(binding.queue, { durable: true });
                    await channel.bindQueue(binding.queue, binding.exchange, '#');
                    await channel.consume(binding.queue, async (message) => this.handleMessage(channel, binding.exchange, message), {
                        noAck: false,
                    });
                }
            },
        });
        await this.channelWrapper.waitForConnect();
    }
    async onModuleDestroy() {
        await this.channelWrapper?.close();
        await this.connection?.close();
    }
    async handleMessage(channel, exchange, message) {
        if (!message) {
            return;
        }
        try {
            const envelope = this.toEnvelope(exchange, message);
            if (!envelope) {
                channel.ack(message);
                return;
            }
            await this.billingEventsConsumer.handleEvent(envelope);
            channel.ack(message);
        }
        catch (error) {
            this.logger.error(`Failed to process ${exchange} event ${message.fields.routingKey}.`, error instanceof Error ? error.stack : undefined);
            channel.nack(message, false, true);
        }
    }
    toEnvelope(exchange, message) {
        const parsedPayload = JSON.parse(message.content.toString('utf8'));
        const payload = typeof parsedPayload === 'string'
            ? JSON.parse(parsedPayload)
            : parsedPayload;
        const tenantId = this.extractTenantId(payload);
        if (!tenantId) {
            this.logger.warn(`Skipping ${exchange} event ${message.fields.routingKey} because tenantId is missing. Payload: ${JSON.stringify(payload)}`);
            return null;
        }
        return {
            tenantId,
            eventType: this.normalizeEventType(exchange, message.fields.routingKey),
            payload,
            source: exchange,
            routingKey: message.fields.routingKey,
        };
    }
    extractTenantId(payload) {
        const matchingEntry = Object.entries(payload).find(([key, value]) => key.replace(/[^a-z]/gi, '').toLowerCase() === 'tenantid' && typeof value === 'string' && value.length > 0);
        return typeof matchingEntry?.[1] === 'string' ? matchingEntry[1] : null;
    }
    normalizeEventType(exchange, routingKey) {
        const normalizedRoutingKey = routingKey.trim();
        const billingEventMap = {
            InvoiceCreatedEvent: 'billing.invoice.created',
            InvoicePaidEvent: 'billing.invoice.paid',
            PaymentProcessedEvent: 'billing.payment.processed',
            SubscriptionCreatedEvent: 'billing.subscription.created',
            SubscriptionCancelledEvent: 'billing.subscription.cancelled',
        };
        const identityEventMap = {
            TenantCreatedEvent: 'identity.tenant.created',
            TenantSuspendedEvent: 'identity.tenant.suspended',
            UserCreatedEvent: 'identity.user.created',
            UserPasswordChangedEvent: 'identity.user.password.changed',
        };
        const travelEventMap = {
            FollowUpCreatedEvent: 'travel.follow-up.created',
            FollowUpCompletedEvent: 'travel.follow-up.completed',
            QuotationCreatedEvent: 'travel.quotation.created',
            QuotationSentEvent: 'travel.quotation.sent',
            QuotationAcceptedEvent: 'travel.quotation.accepted',
            ItineraryCreatedEvent: 'travel.itinerary.created',
            ItineraryConfirmedEvent: 'travel.itinerary.confirmed',
        };
        const eventMap = exchange === 'billing.events'
            ? billingEventMap
            : exchange === 'identity.events'
                ? identityEventMap
                : travelEventMap;
        return eventMap[normalizedRoutingKey] ?? normalizedRoutingKey;
    }
};
exports.RabbitMqEventsListener = RabbitMqEventsListener;
exports.RabbitMqEventsListener = RabbitMqEventsListener = RabbitMqEventsListener_1 = __decorate([
    (0, common_1.Injectable)(),
    __metadata("design:paramtypes", [billing_events_consumer_1.BillingEventsConsumer])
], RabbitMqEventsListener);
//# sourceMappingURL=rabbitmq-events.listener.js.map