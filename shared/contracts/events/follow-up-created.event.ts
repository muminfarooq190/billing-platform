export const FOLLOW_UP_CREATED_EVENT_TYPE = 'travel.follow-up.created' as const;

export interface FollowUpCreatedEventPayload {
  readonly followUpId: string;
  readonly tenantId: string;
  readonly customerContactId: string;
  readonly customerName: string;
  readonly subject: string;
  readonly dueDate: string;
  readonly priority: string;
  readonly createdAt: string;
}

export interface FollowUpCreatedEvent {
  readonly eventId: string;
  readonly eventType: typeof FOLLOW_UP_CREATED_EVENT_TYPE;
  readonly occurredAt: string;
  readonly aggregateId: string;
  readonly aggregateType: 'FollowUp';
  readonly tenantId: string;
  readonly version: number;
  readonly payload: FollowUpCreatedEventPayload;
}
