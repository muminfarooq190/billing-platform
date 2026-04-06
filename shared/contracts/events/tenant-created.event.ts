export const TENANT_CREATED_EVENT_TYPE = 'identity.tenant.created' as const;

export type TenantPlan = 'Free' | 'Pro' | 'Enterprise';
export type TenantStatus = 'Active' | 'Suspended' | 'Deleted';

export interface TenantCreatedEventPayload {
  readonly tenantId: string;
  readonly name: string;
  readonly email: string;
  readonly plan: TenantPlan;
  readonly status: TenantStatus;
  readonly createdAt: string;
}

export interface TenantCreatedEvent {
  readonly eventId: string;
  readonly eventType: typeof TENANT_CREATED_EVENT_TYPE;
  readonly occurredAt: string;
  readonly aggregateId: string;
  readonly aggregateType: 'Tenant';
  readonly tenantId: string;
  readonly version: number;
  readonly payload: TenantCreatedEventPayload;
}
