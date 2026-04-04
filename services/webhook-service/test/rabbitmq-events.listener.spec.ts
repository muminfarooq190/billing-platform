import { RabbitMqEventsListener } from '../src/consumers/rabbitmq-events.listener';

describe('RabbitMqEventsListener', () => {
  const listener = new RabbitMqEventsListener({ handleEvent: jest.fn() } as never);

  it('normalizes legacy billing event names', () => {
    const eventType = (listener as unknown as { normalizeEventType: (exchange: 'billing.events', routingKey: string) => string }).normalizeEventType(
      'billing.events',
      'InvoiceCreatedEvent',
    );

    expect(eventType).toBe('billing.invoice.created');
  });

  it('normalizes legacy identity event names', () => {
    const eventType = (listener as unknown as { normalizeEventType: (exchange: 'identity.events', routingKey: string) => string }).normalizeEventType(
      'identity.events',
      'UserCreatedEvent',
    );

    expect(eventType).toBe('identity.user.created');
  });

  it('keeps already-stable routing keys untouched', () => {
    const eventType = (listener as unknown as { normalizeEventType: (exchange: 'billing.events', routingKey: string) => string }).normalizeEventType(
      'billing.events',
      'billing.payment.processed',
    );

    expect(eventType).toBe('billing.payment.processed');
  });
});
