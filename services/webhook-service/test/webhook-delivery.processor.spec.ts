import axios from 'axios';
import { WebhookDeliveryProcessor } from '../src/jobs/webhook-delivery.processor';

jest.mock('axios');
const mockedAxios = axios as jest.Mocked<typeof axios>;

describe('WebhookDeliveryProcessor', () => {
  const subscription = {
    id: 'sub-1',
    tenantId: 'tenant-1',
    targetUrl: 'https://example.com/webhook',
    signingSecret: 'secret',
    isActive: true,
  };

  const webhookService = {
    getById: jest.fn().mockResolvedValue(subscription),
  };

  const signerService = {
    sign: jest.fn().mockReturnValue('sha256=test'),
  };

  const deliveryLogService = {
    markResult: jest.fn(),
  };

  const deliveryQueue = {
    enqueue: jest.fn(),
  };

  const processor = new WebhookDeliveryProcessor(
    webhookService as never,
    signerService as never,
    deliveryLogService as never,
    deliveryQueue as never,
  );

  beforeEach(() => {
    jest.clearAllMocks();
    mockedAxios.isAxiosError.mockImplementation((value: unknown) => Boolean((value as { isAxiosError?: boolean })?.isAxiosError));
  });

  it('marks 400 failures dead immediately', async () => {
    mockedAxios.post.mockRejectedValueOnce({
      isAxiosError: true,
      response: {
        status: 400,
        data: { error: 'bad request' },
        headers: {},
      },
    });

    await processor.process({
      data: {
        webhookSubscriptionId: 'sub-1',
        deliveryLogId: 'log-1',
        eventType: 'billing.invoice.created',
        payload: { hello: 'world' },
        attemptNumber: 1,
      },
    } as never);

    expect(deliveryLogService.markResult).toHaveBeenCalledWith('log-1', 'Dead', 1, 400, JSON.stringify({ error: 'bad request' }));
    expect(deliveryQueue.enqueue).not.toHaveBeenCalled();
  });

  it('retries 500 failures', async () => {
    mockedAxios.post.mockRejectedValueOnce({
      isAxiosError: true,
      response: {
        status: 500,
        data: { error: 'server error' },
        headers: {},
      },
    });

    await processor.process({
      data: {
        webhookSubscriptionId: 'sub-1',
        deliveryLogId: 'log-2',
        eventType: 'billing.invoice.created',
        payload: { hello: 'world' },
        attemptNumber: 1,
      },
    } as never);

    expect(deliveryLogService.markResult).toHaveBeenCalledWith('log-2', 'Failed', 1, 500, JSON.stringify({ error: 'server error' }));
    expect(deliveryQueue.enqueue).toHaveBeenCalled();
  });

  it('honors retry-after for 429 responses', async () => {
    mockedAxios.post.mockRejectedValueOnce({
      isAxiosError: true,
      response: {
        status: 429,
        data: { error: 'rate limited' },
        headers: { 'retry-after': '120' },
      },
    });

    await processor.process({
      data: {
        webhookSubscriptionId: 'sub-1',
        deliveryLogId: 'log-3',
        eventType: 'billing.invoice.created',
        payload: { hello: 'world' },
        attemptNumber: 1,
      },
    } as never);

    expect(deliveryQueue.enqueue).toHaveBeenCalledWith(
      expect.objectContaining({ deliveryLogId: 'log-3', attemptNumber: 2 }),
      120000,
    );
  });
});
