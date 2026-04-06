export const USER_CREATED_EVENT_TYPE = 'identity.user.created' as const;

export type UserRole = 'Owner' | 'Admin' | 'Member';

export interface UserCreatedEventPayload {
  readonly userId: string;
  readonly tenantId: string;
  readonly email: string;
  readonly role: UserRole;
  readonly createdAt: string;
}

export interface UserCreatedEvent {
  readonly eventId: string;
  readonly eventType: typeof USER_CREATED_EVENT_TYPE;
  readonly occurredAt: string;
  readonly aggregateId: string;
  readonly aggregateType: 'User';
  readonly tenantId: string;
  readonly version: number;
  readonly payload: UserCreatedEventPayload;
}
