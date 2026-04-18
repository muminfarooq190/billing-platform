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
- seeded/in-memory lead catalog with simple polygon filtering and ranking
- in-memory result store for current MVP phase
- domain/data structures for queries and leads

## What is NOT implemented yet

Not built yet:
- real scraper/indexer pipeline
- persistent database storage
- PostGIS support
- source ingestion jobs
- external enrichment
- compliance allowlist/blacklist tooling
- saved queries / refresh jobs

## Current purpose

This service is an honest starting point so frontend and product flow can be wired now while scraper/indexing architecture lands later.

## Endpoints

- `POST /geo-leads/queries`
- `GET /geo-leads/queries/{queryId}`
- `GET /geo-leads/queries/{queryId}/export?format=csv`
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
