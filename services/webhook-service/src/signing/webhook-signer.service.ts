import { createHmac } from 'crypto';
import { Injectable } from '@nestjs/common';

@Injectable()
export class WebhookSignerService {
  public sign(secret: string, timestamp: string, body: string): string {
    const signedPayload = `${timestamp}.${body}`;
    const digest = createHmac('sha256', secret).update(signedPayload).digest('hex');
    return `sha256=${digest}`;
  }
}
