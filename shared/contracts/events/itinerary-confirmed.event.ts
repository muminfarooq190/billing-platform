export const ITINERARY_CONFIRMED_EVENT_TYPE = 'travel.itinerary.confirmed' as const;

export interface ItineraryConfirmedEventPayload {
  readonly itineraryId: string;
  readonly tenantId: string;
  readonly customerContactId: string;
  readonly destination: string;
  readonly startDate: string;
  readonly endDate: string;
  readonly confirmedAt: string;
}

export interface ItineraryConfirmedEvent {
  readonly eventId: string;
  readonly eventType: typeof ITINERARY_CONFIRMED_EVENT_TYPE;
  readonly occurredAt: string;
  readonly aggregateId: string;
  readonly aggregateType: 'Itinerary';
  readonly tenantId: string;
  readonly version: number;
  readonly payload: ItineraryConfirmedEventPayload;
}
