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

  @Get('event-catalog')
  public eventCatalog(): { events: Array<{ key: string; group: string; description: string }> } {
    return {
      events: [
        // Billing
        { key: 'billing.invoice.created', group: 'Billing', description: 'New invoice generated for tenant subscription or booking.' },
        { key: 'billing.invoice.paid', group: 'Billing', description: 'Invoice marked paid via Stripe checkout or manual reconciliation.' },
        { key: 'billing.invoice.failed', group: 'Billing', description: 'Invoice payment attempt failed (declined card, insufficient funds, etc.).' },
        { key: 'billing.subscription.created', group: 'Billing', description: 'Tenant subscription activated.' },
        { key: 'billing.subscription.cancelled', group: 'Billing', description: 'Tenant subscription cancelled or expired.' },

        // Travel — Inquiries
        { key: 'travel.inquiry.created', group: 'Inquiries', description: 'New inquiry submitted (web form, email, partner channel).' },
        { key: 'travel.inquiry.assigned', group: 'Inquiries', description: 'Inquiry assigned to a consultant.' },
        { key: 'travel.inquiry.qualified', group: 'Inquiries', description: 'Inquiry marked qualified for quotation.' },

        // Travel — Quotations
        { key: 'travel.quotation.created', group: 'Quotations', description: 'New quotation drafted from inquiry.' },
        { key: 'travel.quotation.sent', group: 'Quotations', description: 'Quotation emailed to customer with public share link.' },
        { key: 'travel.quotation.accepted', group: 'Quotations', description: 'Customer accepted quotation (public token or portal action).' },
        { key: 'travel.quotation.rejected', group: 'Quotations', description: 'Customer rejected quotation.' },
        { key: 'travel.quotation.expired', group: 'Quotations', description: 'Quotation passed valid-until date without acceptance.' },

        // Travel — Bookings
        { key: 'travel.booking.confirmed', group: 'Bookings', description: 'Booking created from accepted quotation.' },
        { key: 'travel.booking.payment_received', group: 'Bookings', description: 'Booking payment milestone marked paid.' },
        { key: 'travel.booking.cancelled', group: 'Bookings', description: 'Booking cancelled before travel.' },

        // Communication
        { key: 'communication.notification.sent', group: 'Communication', description: 'Outbound notification dispatched (email/SMS/in-app).' },
        { key: 'communication.notification.delivered', group: 'Communication', description: 'Notification confirmed delivered by provider.' },
        { key: 'communication.notification.failed', group: 'Communication', description: 'Notification dispatch or delivery failed.' },

        // Identity
        { key: 'identity.user.invited', group: 'Identity', description: 'Team member invitation sent.' },
        { key: 'identity.user.activated', group: 'Identity', description: 'Invited user accepted invitation.' },
      ],
    };
  }
}
