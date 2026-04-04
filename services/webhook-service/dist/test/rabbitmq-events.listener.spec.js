"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const rabbitmq_events_listener_1 = require("../src/consumers/rabbitmq-events.listener");
describe('RabbitMqEventsListener', () => {
    const listener = new rabbitmq_events_listener_1.RabbitMqEventsListener({ handleEvent: jest.fn() });
    it('normalizes legacy billing event names', () => {
        const eventType = listener.normalizeEventType('billing.events', 'InvoiceCreatedEvent');
        expect(eventType).toBe('billing.invoice.created');
    });
    it('normalizes legacy identity event names', () => {
        const eventType = listener.normalizeEventType('identity.events', 'UserCreatedEvent');
        expect(eventType).toBe('identity.user.created');
    });
    it('keeps already-stable routing keys untouched', () => {
        const eventType = listener.normalizeEventType('billing.events', 'billing.payment.processed');
        expect(eventType).toBe('billing.payment.processed');
    });
});
//# sourceMappingURL=rabbitmq-events.listener.spec.js.map