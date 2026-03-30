import { Body, Controller, Delete, Get, Param, Post, Query } from '@nestjs/common';
import { DeliveryLogService } from '../delivery-log/delivery-log.service';
import { WebhookService } from './webhook.service';

@Controller('webhooks')
export class WebhookController {
  public constructor(
    private readonly webhookService: WebhookService,
    private readonly deliveryLogService: DeliveryLogService,
  ) {}

  @Get('deliveries')
  public async listDeliveries(
    @Query('page') page = '1',
    @Query('page_size') pageSize = '20',
  ): Promise<unknown> {
    return this.deliveryLogService.list(Number(page), Number(pageSize));
  }

  @Get('deliveries/:id')
  public async getDelivery(@Param('id') id: string): Promise<unknown> {
    return this.deliveryLogService.getById(id);
  }

  @Post('deliveries/:id/replay')
  public async replay(@Param('id') id: string): Promise<{ replayRequested: boolean; id: string }> {
    return { replayRequested: true, id };
  }

  @Get('subscriptions')
  public async listSubscriptions(): Promise<unknown> {
    return this.webhookService.listSubscriptions();
  }

  @Delete('subscriptions/:id')
  public async remove(@Param('id') id: string): Promise<void> {
    await this.webhookService.deactivateSubscription(id);
  }
}
