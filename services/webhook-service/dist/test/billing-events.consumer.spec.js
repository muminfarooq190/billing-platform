"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const billing_events_consumer_1 = require("../src/consumers/billing-events.consumer");
describe('BillingEventsConsumer', () => {
    it('skips duplicate events when fingerprint already exists', async () => {
        const webhookService = {
            listByTenantAndEvent: jest.fn().mockResolvedValue([{ id: 'sub-1' }]),
        };
        const deliveryLogService = {
            getByFingerprint: jest.fn().mockResolvedValue({ id: 'existing-log' }),
            createPending: jest.fn(),
        };
        const deliveryQueue = {
            enqueue: jest.fn(),
        };
        const consumer = new billing_events_consumer_1.BillingEventsConsumer(webhookService, deliveryLogService, deliveryQueue);
        await consumer.handleEvent({
            tenantId: 'tenant-1',
            eventType: 'billing.invoice.created',
            payload: { InvoiceId: 'inv-1', TotalAmount: 100 },
            source: 'billing.events',
            routingKey: 'billing.invoice.created',
        });
        expect(deliveryLogService.createPending).not.toHaveBeenCalled();
        expect(deliveryQueue.enqueue).not.toHaveBeenCalled();
    });
});
//# sourceMappingURL=billing-events.consumer.spec.js.map