import { Body, Controller, Delete, Get, NotFoundException, Param, Post, Query, Req, UnauthorizedException } from '@nestjs/common';
import { Request } from 'express';
import { TenantContext } from '../../common/tenant-context';
import { WebhookDeliveryQueue } from '../../jobs/webhook-delivery.queue';
import { DeliveryLogService } from '../delivery-log/delivery-log.service';
import { CreateWebhookSubscriptionDto } from './dto/create-webhook-subscription.dto';
import { WebhookService } from './webhook.service';

@Controller('webhooks')
export class WebhookController {
  public constructor(
    private readonly webhookService: WebhookService,
    private readonly deliveryLogService: DeliveryLogService,
    private readonly deliveryQueue: WebhookDeliveryQueue,
    private readonly tenantContext: TenantContext,
  ) {}

  @Get('deliveries')
  public async listDeliveries(
    @Req() request: Request,
    @Query('page') page = '1',
    @Query('page_size') pageSize = '20',
  ): Promise<unknown> {
    const tenantId = this.tenantContext.getTenantId(request);
    return this.deliveryLogService.list(tenantId, Number(page), Number(pageSize));
  }

  @Get('deliveries/:id')
  public async getDelivery(@Req() request: Request, @Param('id') id: string): Promise<unknown> {
    const tenantId = this.tenantContext.getTenantId(request);
    const log = await this.deliveryLogService.getById(id, tenantId);
    if (!log) throw new NotFoundException();
    return log;
  }

  @Post('deliveries/:id/replay')
  public async replay(@Req() request: Request, @Param('id') id: string): Promise<{ replayRequested: boolean; id: string }> {
    const tenantId = this.tenantContext.getTenantId(request);
    const log = await this.deliveryLogService.getById(id, tenantId);
    if (!log) throw new NotFoundException();

    const subscription = await this.webhookService.getById(log.webhookSubscriptionId);
    if (!subscription || subscription.tenantId !== tenantId || !subscription.isActive) {
      throw new UnauthorizedException('Webhook subscription is not active for this tenant.');
    }

    await this.deliveryQueue.enqueue(
      {
        webhookSubscriptionId: log.webhookSubscriptionId,
        deliveryLogId: log.id,
        eventType: log.eventType,
        payload: log.payload,
        attemptNumber: 1,
      },
      0,
    );

    return { replayRequested: true, id };
  }

  @Get('subscriptions')
  public async listSubscriptions(@Req() request: Request): Promise<unknown> {
    const tenantId = this.tenantContext.getTenantId(request);
    return this.webhookService.listSubscriptions(tenantId);
  }

  @Post('subscriptions')
  public async createSubscription(@Req() request: Request, @Body() body: CreateWebhookSubscriptionDto): Promise<unknown> {
    const tenantId = this.tenantContext.getTenantId(request);
    if (body.tenantId !== tenantId) {
      throw new UnauthorizedException('tenantId in payload must match x-tenant-id header.');
    }

    return this.webhookService.createSubscription(body);
  }

  @Delete('subscriptions/:id')
  public async remove(@Req() request: Request, @Param('id') id: string): Promise<void> {
    const tenantId = this.tenantContext.getTenantId(request);
    await this.webhookService.deactivateSubscription(id, tenantId);
  }
}
