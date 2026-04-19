# Geo Leads Service

This service is the first implementation pass for the feature described in:
- `docs/geo-tourist-leads-implementation-spec.md`

## What is implemented now

Initial MVP backend skeleton:
- submit geo lead query by polygon
- get query results by id
- export query results as CSV
- tenant-scoped query access via `x-tenant-id`
- stricter polygon validation (closed polygons, coordinate bounds, minimum point count)
- persisted query metadata via EF Core/Postgres-ready DbContext
- persisted query result rows with lead snapshots for retrieval/export
- ingestion skeleton for raw source records
- source adapter abstraction for future scraper/indexer plug-in
- searchable catalog that can use ingested source records or fallback seeded data
- domain/data structures for queries and leads

## What is NOT implemented yet

Not built yet:
- real external scraper/indexer pipeline
- PostGIS support
- scheduled/background ingestion jobs
- external enrichment
- compliance allowlist/blacklist tooling
- saved queries / refresh jobs

## Current ingestion endpoint

- `POST /geo-leads/sources/ingest`

Current ingestion uses a seeded source adapter and persists raw source records so future source adapters can plug into the same flow.

## Current purpose

This service is an honest starting point so frontend and product flow can be wired now while scraper/indexing architecture lands later.

## Endpoints

- `GET /geo-leads/queries`
- `POST /geo-leads/queries`
- `GET /geo-leads/queries/{queryId}`
- `POST /geo-leads/queries/{queryId}/refresh`
- `GET /geo-leads/queries/{queryId}/export?format=csv`
- `POST /geo-leads/saved-areas`
- `GET /geo-leads/saved-areas`
- `GET /geo-leads/saved-areas/{areaId}`
- `PUT /geo-leads/saved-areas/{areaId}`
- `POST /geo-leads/saved-areas/{areaId}/run-query`
- `DELETE /geo-leads/saved-areas/{areaId}`
- `GET /health`

## Request notes

Current implementation supports only:
- `Polygon` geometry

Coordinates are expected as:
```json
{
  "geometry": {
    "type": "Polygon",
    "coordinates": [
      [72.82, 18.92],
      [72.83, 18.92],
      [72.83, 18.93],
      [72.82, 18.93]
    ]
  },
  "leadTypes": ["hotel", "tour_operator"],
  "limit": 50,
  "rankingMode": "relevance"
}
```
