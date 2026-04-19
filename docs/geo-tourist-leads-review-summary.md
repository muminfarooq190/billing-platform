# Geo Tourist Leads - Review Summary

## Branch

- `feature/geo-tourist-leads-spec`

## Implemented backend scope

### Query lifecycle
- submit geo area query
- get query by id
- list tenant query history
- refresh existing query
- export query results as CSV
- ranking modes: `relevance`, `contactability`, `popularity`

### Saved areas
- create saved area
- list saved areas
- get saved area by id
- update saved area
- delete saved area
- run a geo lead query directly from a saved area

### Source ingestion
- ingest source records
- multiple adapters supported
- seeded adapter
- config-backed public directory snapshot adapter
- adapter enable/disable flags via config
- dedupe/upsert by `(source_name, source_record_id)`
- ingestion status endpoint
- ingestion run audit trail

### Platform/runtime integration
- service Dockerfile added
- service added to `docker-compose.yml`
- `GEO_LEADS_DATABASE_URL` added to `.env.example`
- Postgres init script creates `billing_geo_leads`
- service added to `billing-platform.sln`
- api-gateway reverse proxy route added
- api-gateway downstream health check added
- api-gateway entitlement route mapping added

### Entitlements
- gateway route entitlement enforcement for `/api/geo-leads`
- in-service feature gate via billing entitlements
- `geo-leads.read` enforced on read handlers
- `geo-leads.manage` enforced on write/manage handlers
- saved area flows moved behind handlers so they are feature-gated consistently

### Persistence
- EF Core model in place
- initial EF migration scaffolded
- startup now uses `Database.Migrate()` instead of `EnsureCreated()`

## Current API surface

### Queries
- `GET /geo-leads/queries`
- `POST /geo-leads/queries`
- `GET /geo-leads/queries/{queryId}`
- `POST /geo-leads/queries/{queryId}/refresh`
- `GET /geo-leads/queries/{queryId}/export?format=csv`

### Saved areas
- `POST /geo-leads/saved-areas`
- `GET /geo-leads/saved-areas`
- `GET /geo-leads/saved-areas/{areaId}`
- `PUT /geo-leads/saved-areas/{areaId}`
- `DELETE /geo-leads/saved-areas/{areaId}`
- `POST /geo-leads/saved-areas/{areaId}/run-query`

### Sources
- `POST /geo-leads/sources/ingest`
- `GET /geo-leads/sources/status`

## What remains out of scope / not finished
- real live external source integrations or scraping
- PostGIS/native spatial querying
- saved area sharing/collaboration
- source allowlist/blacklist compliance workflows
- frontend integration
- generated API clients
- richer export formats beyond CSV

## Validation status
- geo-leads test suite passing locally
- api-gateway test suite updated for geo-leads entitlement route matching

## Reviewer notes
- The feature is no longer just a spec scaffold; it is a working backend slice wired into the platform runtime.
- The current quality level is MVP-to-review-ready backend, not final production-finished product.
