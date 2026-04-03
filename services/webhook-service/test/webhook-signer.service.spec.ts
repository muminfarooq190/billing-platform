import { WebhookSignerService } from '../src/signing/webhook-signer.service';

describe('WebhookSignerService', () => {
  it('signs payload deterministically', () => {
    const signer = new WebhookSignerService();
    const signature = signer.sign('secret', '1700000000', '{"ok":true}');
    expect(signature.startsWith('sha256=')).toBe(true);
  });
});
