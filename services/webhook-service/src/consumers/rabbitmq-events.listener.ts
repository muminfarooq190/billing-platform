import { Injectable, Logger, OnModuleDestroy, OnModuleInit } from '@nestjs/common';
import { connect, ChannelWrapper } from 'amqp-connection-manager';
import type { Channel, ConsumeMessage } from 'amqplib';
import { BillingEventsConsumer } from './billing-events.consumer';
import { DomainEventEnvelope } from './domain-events.types';

interface ExchangeBindingConfig {
  exchange: 'billing.events' | 'identity.events' | 'travel.events';
  queue: string;
}

@Injectable()
export class RabbitMqEventsListener implements OnModuleInit, OnModuleDestroy {
  private readonly logger = new Logger(RabbitMqEventsListener.name);
  private connection?: ReturnType<typeof connect>;
  private channelWrapper?: ChannelWrapper;

  public constructor(private readonly billingEventsConsumer: BillingEventsConsumer) {}

  public async onModuleInit(): Promise<void> {
    const bindings: ExchangeBindingConfig[] = [
      { exchange: 'billing.events', queue: process.env.BILLING_EVENTS_QUEUE ?? 'webhook-service.billing.events' },
      { exchange: 'identity.events', queue: process.env.IDENTITY_EVENTS_QUEUE ?? 'webhook-service.identity.events' },
      { exchange: 'travel.events', queue: process.env.TRAVEL_EVENTS_QUEUE ?? 'webhook-service.travel.events' },
    ];

    this.connection = connect([process.env.RABBITMQ_URL ?? 'amqp://guest:guest@rabbitmq:5672']);

    this.connection.on('connect', () => this.logger.log('Connected to RabbitMQ.'));
    this.connection.on('disconnect', (params) =>
      this.logger.error(`Disconnected from RabbitMQ: ${params.err?.message ?? 'unknown error'}`),
    );

    this.channelWrapper = this.connection.createChannel({
      setup: async (channel: Channel) => {
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

  public async onModuleDestroy(): Promise<void> {
    await this.channelWrapper?.close();
    await this.connection?.close();
  }

  private async handleMessage(
    channel: Channel,
    exchange: ExchangeBindingConfig['exchange'],
    message: ConsumeMessage | null,
  ): Promise<void> {
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
    } catch (error) {
      this.logger.error(`Failed to process ${exchange} event ${message.fields.routingKey}.`, error instanceof Error ? error.stack : undefined);
      channel.nack(message, false, true);
    }
  }

  private toEnvelope(
    exchange: ExchangeBindingConfig['exchange'],
    message: ConsumeMessage,
  ): DomainEventEnvelope | null {
    const parsedPayload = JSON.parse(message.content.toString('utf8')) as Record<string, unknown> | string;
    const payload = typeof parsedPayload === 'string'
      ? (JSON.parse(parsedPayload) as Record<string, unknown>)
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

  private extractTenantId(payload: Record<string, unknown>): string | null {
    const matchingEntry = Object.entries(payload).find(([key, value]) =>
      key.replace(/[^a-z]/gi, '').toLowerCase() === 'tenantid' && typeof value === 'string' && value.length > 0,
    );

    return typeof matchingEntry?.[1] === 'string' ? matchingEntry[1] : null;
  }

  private normalizeEventType(exchange: ExchangeBindingConfig['exchange'], routingKey: string): string {
    const normalizedRoutingKey = routingKey.trim();

    const billingEventMap: Record<string, string> = {
      InvoiceCreatedEvent: 'billing.invoice.created',
      InvoicePaidEvent: 'billing.invoice.paid',
      PaymentProcessedEvent: 'billing.payment.processed',
      SubscriptionCreatedEvent: 'billing.subscription.created',
      SubscriptionCancelledEvent: 'billing.subscription.cancelled',
    };

    const identityEventMap: Record<string, string> = {
      TenantCreatedEvent: 'identity.tenant.created',
      TenantSuspendedEvent: 'identity.tenant.suspended',
      UserCreatedEvent: 'identity.user.created',
      UserPasswordChangedEvent: 'identity.user.password.changed',
    };

    const travelEventMap: Record<string, string> = {
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
}
