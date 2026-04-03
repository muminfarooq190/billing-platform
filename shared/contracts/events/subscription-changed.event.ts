export const SUBSCRIPTION_CHANGED_EVENT_TYPE = 'billing.subscription.changed' as const;

export type SubscriptionAction = 'Created' | 'Renewed' | 'Cancelled' | 'Paused' | 'Resumed';
export type PlanType = 'Free' | 'Pro' | 'Enterprise';
export type BillingCycle = 'Monthly' | 'Annual';

export interface SubscriptionChangedEventPayload {
  readonly subscriptionId: string;
  readonly tenantId: string;
  readonly action: SubscriptionAction;
  readonly previousPlan: PlanType | null;
  readonly currentPlan: PlanType;
  readonly billingCycle: BillingCycle;
  readonly effectiveAt: string;
}

export interface SubscriptionChangedEvent {
  readonly eventId: string;
  readonly eventType: typeof SUBSCRIPTION_CHANGED_EVENT_TYPE;
  readonly occurredAt: string;
  readonly aggregateId: string;
  readonly aggregateType: 'Subscription';
  readonly tenantId: string;
  readonly version: number;
  readonly payload: SubscriptionChangedEventPayload;
}
