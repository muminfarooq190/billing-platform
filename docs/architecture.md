# Architecture Decision Records (ADRs)

## ADR-001: Adopt CQRS + MediatR over CRUD controllers
- **Status:** Accepted
- **Date:** 2026-03-30
- **Context:** Billing and identity flows have distinct read/write concerns (validation, idempotency, outbox writes, cache invalidation, and optimized reads).
- **Decision:** Use CQRS with MediatR command/query handlers in .NET services.
- **Consequences:**
  - Clear separation of write invariants from read optimization.
  - Easier instrumentation (`command.{CommandName}` spans) and test isolation.
  - Additional complexity in boilerplate and handler orchestration.

## ADR-002: Use Transactional Outbox instead of direct broker publish
- **Status:** Accepted
- **Date:** 2026-03-30
- **Context:** Direct broker publish inside request handlers can lose events when DB commit succeeds but publish fails.
- **Decision:** Persist domain events to `domain_events` in the same DB transaction as aggregate writes; publish asynchronously.
- **Consequences:**
  - At-least-once delivery and eventual consistency guaranteed.
  - Consumer idempotency required.
  - Added background publisher complexity.

## ADR-003: Per-service databases (database-per-service)
- **Status:** Accepted
- **Date:** 2026-03-30
- **Context:** Shared DB creates coupling and cross-team migration risk.
- **Decision:** Identity, Billing, and Webhook services own independent schemas and migrations.
- **Consequences:**
  - Strong bounded contexts and independent deployability.
  - Cross-service joins are forbidden; integration requires events or APIs.

## ADR-004: Money value object for financial arithmetic
- **Status:** Accepted
- **Date:** 2026-03-30
- **Context:** Raw decimal arithmetic can silently mix currencies and create precision defects.
- **Decision:** Represent all financial amounts with `Money(amount, currency)` and guard cross-currency operations.
- **Consequences:**
  - Currency-safe invariants and explicit domain semantics.
  - More mapping code in persistence layers.

## ADR-005: BullMQ for webhook retries over custom scheduler
- **Status:** Accepted
- **Date:** 2026-03-30
- **Context:** Webhook delivery needs persistence, delayed retries, replay, and dead-letter semantics.
- **Decision:** Use BullMQ (Redis-backed) with explicit retry schedule and delivery log state machine.
- **Consequences:**
  - Reliable retry orchestration and operational replay.
  - Redis dependency for webhook pipeline availability.

