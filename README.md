# Voyara

![CI](https://github.com/{user}/voyara/actions/workflows/ci.yml/badge.svg)
![Coverage](https://img.shields.io/codecov/c/github/{user}/voyara)

Voyara is a travel CRM platform built on independent microservices, event-driven communication, webhook delivery, and end-to-end observability.

## Architecture Diagram

![Architecture Diagram](docs/architecture-diagram.png)

## What the platform includes

- **API Gateway** for routing, JWT validation, rate limiting, metrics, CORS, and readiness checks
- **Identity Service** for tenant registration, auth, JWT/refresh flows, and user management
- **Travel Service** for contacts, quotations, itineraries, and follow-ups
- **Billing Service** for subscriptions, invoices, payments, and dashboards
- **Communication Service** for templates, recipient preferences, notifications, unread counts, and mark-as-read flows
- **Webhook Service** for signed outbound event delivery, replay, subscriptions, and delivery logs

## Tech Stack

| Layer | Technology | Why chosen |
|---|---|---|
| API Gateway | ASP.NET Core 8 + YARP | High-performance reverse proxy with flexible route/transforms and first-class .NET middleware integration. |
| Identity Service | ASP.NET Core 8 + MediatR + EF Core + Dapper | CQRS split for complex auth writes and fast read models while preserving domain invariants. |
| Billing Service | ASP.NET Core 8 + MediatR + EF Core + Dapper + Redis | Handles financial write rules + cached dashboards and invoice reads for throughput. |
| Travel Service | ASP.NET Core 8 + MediatR + EF Core + Dapper | Travel CRM workflows for contacts, quotations, itineraries, and follow-ups. |
| Communication Service | ASP.NET Core 8 + MediatR + EF Core + Dapper | Templates, notifications, recipient preferences, unread counts, and message tracking. |
| Webhook Service | NestJS + TypeORM + BullMQ | Strong TypeScript modularity plus resilient delayed retries and replay-friendly delivery jobs. |
| Data | PostgreSQL 16 | ACID transactions and relational consistency for aggregates + outbox. |
| Async Messaging | RabbitMQ | Reliable broker semantics for cross-service domain event fan-out. |
| Cache | Redis 7.2 | Fast tenant-aware rate limiting, token state, and query cache-aside patterns. |
| Observability | OpenTelemetry + Prometheus + Jaeger | Distributed tracing + metrics with low-friction local deployment. |
| CI/CD | GitHub Actions + Codecov | Automated build/test/lint and coverage publishing per service. |

## Quick Start

```bash
git clone https://github.com/{user}/voyara.git
cd voyara
cp .env.example .env
docker compose up -d
```

## API Reference

- Gateway: `http://localhost:5000`
- Gateway health: `http://localhost:5000/health`
- Gateway readiness: `http://localhost:5000/health/ready`
- Identity Swagger: `http://localhost:5001/swagger`
- Billing Swagger: `http://localhost:5002/swagger`
- Travel Swagger: `http://localhost:5004/swagger`

## Gateway Routes

- `/api/auth/*` → identity-service auth
- `/api/identity/*` → identity-service management APIs
- `/api/billing/*` → billing-service
- `/api/travel/*` → travel-service
- `/api/communication/*` → communication-service
- `/api/webhooks/*` → webhook-service

## Webhooks

Voyara includes a dedicated webhook service for outbound integrations.

### Subscription endpoints

Via gateway:
- `GET /api/webhooks/subscriptions`
- `POST /api/webhooks/subscriptions`
- `DELETE /api/webhooks/subscriptions/{id}`

### Delivery endpoints

Via gateway:
- `GET /api/webhooks/deliveries?page=1&page_size=20`
- `GET /api/webhooks/deliveries/{id}`
- `POST /api/webhooks/deliveries/{id}/replay`

### Example webhook subscription payload

```json
{
  "tenantId": "00000000-0000-0000-0000-000000000000",
  "targetUrl": "https://example.com/webhooks/voyara",
  "events": [
    "travel.quotation.created",
    "travel.itinerary.confirmed"
  ],
  "signingSecret": "replace-me",
  "isActive": true
}
```

### Current shared event contracts

Shared typed contracts live in `shared/contracts/events/`.

Included event contract files:
- `tenant-created.event.ts`
- `user-created.event.ts`
- `invoice-created.event.ts`
- `payment-processed.event.ts`
- `subscription-changed.event.ts`
- `quotation-created.event.ts`
- `quotation-accepted.event.ts`
- `follow-up-created.event.ts`
- `itinerary-confirmed.event.ts`

## Postman

Updated collection:
- `postman/billing-platform.postman_collection.json`

It now includes:
- gateway health + readiness
- identity auth + user management
- travel contacts/quotations/itineraries/follow-ups
- communication notifications/templates/preferences
- webhook subscriptions/deliveries/replay

## Services

- `api-gateway`: auth validation, routing, rate limiting, readiness, metrics
- `identity-service`: tenants, users, JWT/refresh flows, JWKS, outbox
- `billing-service`: subscriptions, invoices, payments, dashboard, outbox
- `travel-service`: contacts, quotations, itineraries, follow-ups, outbox

## Phase 7 booking workflow status

Phase 7 booking and fulfillment foundation is now implemented across the travel service, including:
- accepted quotation -> booking handoff
- booking operational statuses + booking list/detail reads
- traveler CRUD for multi-passenger trips
- operational booking item CRUD + status updates
- booking document upload/list/delete support

Useful booking endpoints:
- `POST /api/travel/bookings/from-quotation/{quotationId}`
- `GET /api/travel/bookings?page=1&pageSize=20&status=Pending&destination=Rome`
- `GET /api/travel/bookings/{id}`
- `POST /api/travel/bookings/{id}/travelers`
- `GET /api/travel/bookings/{id}/travelers`
- `PUT /api/travel/bookings/{id}/travelers/{travelerId}`
- `DELETE /api/travel/bookings/{id}/travelers/{travelerId}`
- `POST /api/travel/bookings/{id}/items`
- `GET /api/travel/bookings/{id}/items`
- `PUT /api/travel/bookings/{id}/items/{itemId}`
- `PATCH /api/travel/bookings/{id}/items/{itemId}/status`
- `DELETE /api/travel/bookings/{id}/items/{itemId}`
- `POST /api/travel/bookings/{id}/documents`
- `GET /api/travel/bookings/{id}/documents`
- `DELETE /api/travel/bookings/{id}/documents/{documentId}`

## Phase 6 quotation workflow status

Phase 6 quotation maturity is now implemented across the travel service, including:
- immutable quotation revisions + revision history
- quotation status history
- attachment/media upload + customer-visible filtering
- send/share/public-view flow with tokenized customer access
- viewed tracking for public quotation links

Useful quotation endpoints:
- `POST /api/travel/quotations/{id}/revisions`
- `GET /api/travel/quotations/{id}/revisions`
- `GET /api/travel/quotations/{id}/revisions/{revisionId}`
- `GET /api/travel/quotations/{id}/history`
- `POST /api/travel/quotations/{id}/attachments`
- `GET /api/travel/quotations/{id}/attachments`
- `DELETE /api/travel/quotations/{id}/attachments/{attachmentId}`
- `POST /api/travel/quotations/{id}/send`
- `GET /travel/quotations/public/{token}`
- `POST /travel/quotations/public/{token}/viewed`
- `POST /api/travel/quotations/{id}/accept`
- `POST /api/travel/quotations/{id}/reject`
- `POST /api/travel/quotations/{id}/expire`
- `communication-service`: templates, notifications, recipient preferences
- `webhook-service`: event consumption, signed delivery, retries, replay

## Testing Strategy

- Unit tests: domain + application layers (no I/O)
- Integration tests: endpoint + database paths
- Webhook tests: signing and delivery processor behavior
- Coverage target: `>=80%` for Domain + Application layers

## Observability

- Metrics: `http://localhost:9090` (Prometheus)
- Traces: `http://localhost:16686` (Jaeger)
- Gateway metrics endpoint: `http://localhost:5000/metrics`
