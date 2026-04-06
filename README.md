# Voyara

![CI](https://github.com/{user}/voyara/actions/workflows/ci.yml/badge.svg)
![Coverage](https://img.shields.io/codecov/c/github/{user}/voyara)

Voyara is a travel CRM platform built on independent microservices, event-driven communication, webhook delivery, and end-to-end observability.

## Architecture Diagram

![Architecture Diagram](docs/architecture-diagram.png)

## Tech Stack

| Layer | Technology | Why chosen |
|---|---|---|
| API Gateway | ASP.NET Core 8 + YARP | High-performance reverse proxy with flexible route/transforms and first-class .NET middleware integration. |
| Identity Service | ASP.NET Core 8 + MediatR + EF Core + Dapper | CQRS split for complex auth writes and fast read models while preserving domain invariants. |
| Billing Service | ASP.NET Core 8 + MediatR + EF Core + Dapper + Redis | Handles heavy financial write rules + cached dashboards and invoice reads for throughput. |
| Webhook Service | NestJS + TypeORM + BullMQ | Strong TypeScript modularity plus resilient delayed retries and replay-friendly delivery jobs. |
| Data | PostgreSQL 16 | ACID transactions and relational consistency for aggregates + outbox. |
| Async Messaging | RabbitMQ | Reliable broker semantics for cross-service domain event fan-out. |
| Cache | Redis 7.2 | Fast tenant-aware rate limiting, token state, and query cache-aside patterns. |
| Observability | OpenTelemetry + Prometheus + Jaeger | Distributed tracing + metrics with low-friction local deployment. |
| CI/CD | GitHub Actions + Codecov | Automated build/test/lint and coverage publishing per service. |

## Performance Benchmarks

> Benchmarks are reproducible through the included scripts. Run against your machine/environment and publish your own measured numbers.

### Load Test Commands

```bash
# API Gateway p95 latency (500 concurrent virtual users)
k6 run scripts/load/gateway-loadtest.js
```

### Benchmark Targets

- API Gateway p95 under load: target `< 250ms`.
- Invoice list endpoint: capture uncached vs cached response time.
- Webhook delivery latency: capture average time from publish to delivered.

### Results Template

| Scenario | Command | Result |
|---|---|---|
| API Gateway p95 | `k6 run scripts/load/gateway-loadtest.js` | _record in your environment_ |
| Invoice list uncached | `curl .../api/billing/invoices` first hit | _record_ |
| Invoice list cached | `curl .../api/billing/invoices` second hit | _record_ |
| Webhook E2E latency | event publish + delivery log timestamps | _record_ |

## Design Decisions (ADR)

See [`docs/architecture.md`](docs/architecture.md) for ADR entries:
1. CQRS over CRUD
2. Transactional Outbox over direct publish
3. Per-service DB over shared DB
4. Money value object over raw decimal
5. BullMQ retry orchestration over custom scheduler

## Quick Start

```bash
git clone https://github.com/{user}/voyara.git
cd voyara
cp .env.example .env
docker compose up -d

# seed demo data
./scripts/seed-demo.sh

# alternatively create tenant manually
curl -X POST http://localhost:5000/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{"tenantName":"Acme Corp","email":"admin@acme.com","password":"Demo1234!"}'
```

## API Reference

- Identity Swagger: http://localhost:5001/swagger
- Billing Swagger: http://localhost:5002/swagger

## Services

- `api-gateway`: auth validation, routing, rate limiting, metrics.
- `identity-service`: tenants/users, JWT/refresh flows, JWKS, outbox.
- `billing-service`: subscriptions, invoices, payments, dashboard, outbox.
- `webhook-service`: event consumption, signed delivery, retries, replay.

## Testing Strategy

- Unit tests: domain + application layers (no I/O).
- Integration tests: endpoint + database paths.
- Webhook tests: signing and delivery processor behavior.
- Coverage target: `>=80%` for Domain + Application layers.

## Seed Data

`scripts/seed-demo.sh` provisions:
- 1 demo tenant
- 1 subscription
- 3 generated invoices

## Observability

- Metrics: `http://localhost:9090` (Prometheus)
- Traces: `http://localhost:16686` (Jaeger)
- Gateway metrics endpoint: `http://localhost:5000/metrics`


