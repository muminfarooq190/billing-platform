import { Controller, Get } from '@nestjs/common';

@Controller()
export class HealthController {
  @Get('health')
  public health(): { service: string; status: string } {
    return { service: 'webhook-service', status: 'ok' };
  }
}
