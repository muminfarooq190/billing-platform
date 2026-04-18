import { BillingEventsConsumer } from '../src/consumers/billing-events.consumer';

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

    const consumer = new BillingEventsConsumer(
      webhookService as never,
      deliveryLogService as never,
      deliveryQueue as never,
    );

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
