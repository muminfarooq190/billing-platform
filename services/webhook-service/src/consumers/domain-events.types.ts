export interface DomainEventEnvelope {
  tenantId: string;
  eventType: string;
  payload: Record<string, unknown>;
  source: 'billing.events' | 'identity.events' | 'travel.events';
  routingKey: string;
}
