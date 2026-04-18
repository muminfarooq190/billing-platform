# Implementation Spec - Geo Tourist Leads Service (Low-Cost MVP)

_Last updated: 2026-04-18_

## 1. Feature summary

This feature allows a tenant user on the frontend to:

1. draw/select a polygon or shape on a map
2. submit that area to the backend
3. trigger a low-cost backend lead discovery workflow
4. receive a ranked list of the most relevant/frequent tourist prospects associated with that area
5. use those leads for outbound sales workflows such as cold email or follow-up campaigns

The business goal is simple:
- help travel/business tenants discover location-based lead opportunities cheaply
- create a repeatable top-of-funnel input for future inquiry/conversion workflows

This feature should be built as an MVP-oriented, cost-aware service — not a massive geospatial intelligence platform.

---

## 2. Product goal

### What problem it solves
Tenants want to find high-potential tourist leads near or associated with a chosen geographic area without paying for an expensive third-party lead intelligence platform.

### MVP outcome
A tenant can select an area on a map and get back:
- a list of likely tourist leads or tourist demand clusters
- contactable business/person prospects where legally/operationally allowed
- enough signal to start manual or semi-automated outbound qualification

### Non-goal
This is **not** a surveillance product or a people-tracking product.
This is **not** a hidden-profile harvesting system.
This is **not** a tool for scraping personal data from prohibited/private sources.

The service must stay on the side of:
- legal
- low-cost
- operationally sustainable
- explainable to tenants

---

## 3. Core product interpretation

The phrase “find most frequent tourists from that area” needs product clarification because it can mean multiple things.

For MVP, we should **not** interpret it as:
- identify private individuals physically present in a polygon
- infer live location history of real people
- scrape personal data from social or private services

That is a fast route to legal and compliance pain.

### Safer MVP interpretation
The service should identify **tourism-related lead candidates associated with a selected area**, such as:
- hotels / guest houses / hostels
- tour operators
- travel influencers/bloggers with public contact details and public geotagged relevance
- event venues / destination businesses
- activity providers
- destination-based public business contacts
- area tourism demand signals / points of interest / high-tourism clusters

Optional extended interpretation for later:
- publicly visible social or content-based signals indicating tourist interest around a location
- public businesses or creators repeatedly posting about the location

This makes the feature:
- cheaper
- more legal
- more buildable
- actually usable for sales

---

## 4. Recommended naming

Avoid internally naming this feature “find tourists”.
That sounds creepy and misleading.

Prefer one of these:
- **Geo Lead Discovery**
- **Area Lead Finder**
- **Destination Lead Discovery**
- **Geo Prospecting Service**

Recommended service name:
- `geo-leads-service`

Recommended UI label:
- **Find leads in this area**

---

## 5. MVP scope

## 5.1 User flow

1. Tenant opens map UI
2. Tenant draws polygon / circle / rectangle / custom shape
3. Frontend submits shape + optional filters
4. Backend validates tenant + shape + limits
5. Geo leads service launches low-cost discovery pipeline
6. Service returns ranked leads with source metadata and confidence
7. Tenant can review/export/use leads for campaigns

## 5.2 MVP input

Required:
- tenant id from auth context
- geometry type
- polygon coordinates or equivalent map shape

Optional filters:
- lead types
  - hotel
  - tour operator
  - influencer
  - travel agency
  - venue
  - activity provider
- language
- country
- result limit
- ranking mode
  - relevance
  - contactability
  - popularity

## 5.3 MVP output

Each lead should include at minimum:
- lead id
- display name
- lead type
- location/address
- lat/lng if available
- public contact data
  - email if available
  - phone if available
  - website if available
  - instagram/social if available
- source(s)
- confidence score
- reason / explanation
- tags
- last discovered timestamp

---

## 6. Cost strategy

This feature must be intentionally cheap.

### Cost principles
- prefer public web sources over paid APIs
- cache aggressively
- scrape once, reuse many times
- use batch area processing
- rate limit source scraping
- store normalized leads and area-to-lead matches
- avoid per-request deep crawling when possible

### MVP architecture principle
**Do not crawl the web fresh every time a tenant draws a polygon.**
That becomes slow and expensive.

Instead:
- maintain a small background scraper/indexer
- pre-ingest public tourism/business/location signals by region/city
- query that indexed dataset by polygon at request time

This is the only sensible low-cost path.

---

## 7. Data source strategy

## 7.1 Recommended MVP sources

Prefer sources that are public, stable, and low-cost.

### Source class A - public business/location data
Use for initial MVP:
- OpenStreetMap / Overpass-derived tourism/business POIs
- Wikivoyage / public tourism pages for destination context
- public business directories where scraping is allowed
- publicly listed travel businesses with websites/contact pages

### Source class B - website contact enrichment
Use carefully:
- official business websites
- public contact pages
- mailto extraction where allowed
- structured metadata parsing

### Source class C - public content relevance signals
Optional MVP+:
- publicly viewable social profile metadata
- public blog/creator pages mentioning the area
- public hashtags/geotag pages only if compliant and stable

### Source class D - explicit do-not-do sources for MVP
Do not build MVP around:
- private user profiles
- scraped private traveler identities
- platform-prohibited scraping
- any source that violates terms or privacy expectations

---

## 8. Product model

## 8.1 Core entities

### LeadSourceRecord
Represents a raw scraped public record.

Fields:
- id
- source_name
- source_record_id
- raw_name
- raw_category
- raw_address
- raw_phone
- raw_email
- raw_website
- raw_social_links
- raw_lat
- raw_lng
- raw_payload_json
- first_seen_at
- last_seen_at
- scrape_job_id

### GeoLead
Normalized lead entity.

Fields:
- id
- canonical_name
- lead_type
- primary_email
- primary_phone
- website
- social_links_json
- address
- latitude
- longitude
- city
- region
- country
- tags_json
- confidence_score
- contactability_score
- tourism_relevance_score
- explanation_json
- created_at
- updated_at

### GeoLeadSourceLink
Maps normalized leads to raw sources.

Fields:
- id
- geo_lead_id
- source_record_id
- source_name
- match_confidence

### GeoAreaQuery
Represents tenant queries.

Fields:
- id
- tenant_id
- geometry_json
- requested_lead_types_json
- requested_limit
- status
- created_at
- completed_at
- cache_key

### GeoAreaQueryResult
Maps query to leads.

Fields:
- id
- geo_area_query_id
- geo_lead_id
- rank
- score
- reasoning_json

---

## 9. Architecture recommendation

## 9.1 Service boundary

Create a new service:
- `services/geo-leads-service`

Responsibilities:
- accept area lead discovery requests
- query indexed lead store by geometry
- run ranking/filtering
- optionally trigger background enrichment
- expose API for frontend/other services

Do **not** cram this into travel-service.
This is a separate product capability.

## 9.2 Components

### API layer
Endpoints for:
- submit/query area
- get query results
- trigger refresh/reindex
- export results

### Geospatial query layer
- point-in-polygon matching
- bounding box prefilter
- shape normalization

### Scraper/indexer layer
- background jobs to ingest source records
- dedupe and normalize public leads
- refresh stale leads

### Ranking layer
- confidence scoring
- tourism relevance scoring
- contactability scoring
- source-quality weighting

### Storage
Use Postgres with PostGIS if possible.
If PostGIS is too heavy for MVP launch, fallback to:
- store normalized geometry JSON
- use bounding boxes + application-layer point-in-polygon logic

But honest recommendation:
- use **PostGIS** if adding it is feasible

---

## 10. API design

## 10.1 Submit area query

`POST /geo-leads/queries`

Request:
```json
{
  "geometry": {
    "type": "Polygon",
    "coordinates": [[[72.82,18.92],[72.83,18.92],[72.83,18.93],[72.82,18.93],[72.82,18.92]]]
  },
  "leadTypes": ["hotel", "tour_operator", "activity_provider"],
  "limit": 100,
  "rankingMode": "relevance"
}
```

Response:
```json
{
  "queryId": "uuid",
  "status": "Completed",
  "count": 47
}
```

## 10.2 Get query result

`GET /geo-leads/queries/{queryId}`

Response:
```json
{
  "queryId": "uuid",
  "status": "Completed",
  "results": [
    {
      "leadId": "uuid",
      "name": "Sunrise Adventures",
      "leadType": "tour_operator",
      "email": "hello@example.com",
      "phone": "+91...",
      "website": "https://...",
      "location": {
        "address": "...",
        "lat": 18.925,
        "lng": 72.825
      },
      "confidenceScore": 0.87,
      "contactabilityScore": 0.75,
      "tourismRelevanceScore": 0.91,
      "reason": ["inside selected area", "public tourism business", "email available"]
    }
  ]
}
```

## 10.3 Refresh area query

`POST /geo-leads/queries/{queryId}/refresh`

Triggers background refresh/re-enrichment.

## 10.4 Export leads

`GET /geo-leads/queries/{queryId}/export?format=csv`

---

## 11. Discovery pipeline

## 11.1 Offline/background ingestion pipeline

1. pull public area/business/location sources
2. normalize records into `LeadSourceRecord`
3. dedupe into `GeoLead`
4. attach source explanations and scores
5. store geospatial coordinates
6. periodically refresh stale records

## 11.2 Query-time pipeline

1. validate tenant and request
2. normalize geometry
3. find indexed leads inside/intersecting area
4. filter by requested lead types
5. compute ranking
6. store query + query results
7. return ranked list

---

## 12. Ranking model

Use a simple score, not machine learning theater.

### Score components
- location match score
- tourism relevance score
- contactability score
- source confidence score
- recency score

### Example weighted formula
```text
final =
  0.35 * tourism_relevance
+ 0.30 * contactability
+ 0.20 * source_confidence
+ 0.15 * recency
```

### Contactability score factors
- public email available
- public phone available
- working website available
- social profile available

### Tourism relevance factors
- source category indicates tourism/hospitality
- repeated mention of destination
- high-confidence location/category mapping

---

## 13. Frontend requirements

Frontend needs:
- map drawing tool (polygon/rectangle/circle)
- result panel with filters
- lead list table/cards
- export CSV button
- source/reason badges
- warning banner for compliance use

Nice-to-have later:
- save area
- compare areas
- launch email campaign directly

---

## 14. Compliance and safety

This feature can go bad fast if scoped badly.

### Required rules
- only use public or legally permitted data sources
- do not scrape private user identities or private profiles
- do not claim certainty about actual traveler presence
- expose source/explanation per lead
- allow source removal/blacklisting if required
- log scraping provenance

### Messaging guidance
Never market this as:
- “track tourists in real time”
- “identify people inside an area”

Market it as:
- “discover public travel leads associated with an area”
- “find travel businesses and tourism prospects in selected zones”

---

## 15. MVP implementation phases

## Phase 1 - low-cost core MVP

Build:
- `geo-leads-service`
- polygon query API
- Postgres tables
- background ingestion for public business/tourism sources
- result ranking
- CSV export

This is the minimum real product.

## Phase 2 - enrichment

Add:
- website/email enrichment
- social/profile enrichment where compliant
- refresh jobs
- saved queries

## Phase 3 - outbound integration

Add:
- connect leads to communication-service/email campaigns
- lead-to-inquiry tracking
- lead status management

---

## 16. Technical implementation details

## 16.1 Suggested stack
- .NET service to match repo style, or Node service if scraping ecosystem makes that significantly easier
- Postgres + PostGIS preferred
- background jobs via existing infra pattern or separate worker
- Redis queue optional

## 16.2 Why a separate service
Because this feature has its own:
- scraping cadence
- compliance concerns
- ranking model
- storage model
- geospatial logic

It should not pollute billing/travel/communication internals.

---

## 17. Risks

## Risk 1 - legality/compliance
Mitigation:
- public sources only
- source-level allowlist
- explainability

## Risk 2 - low-quality leads
Mitigation:
- scoring
- dedupe
- source confidence
- manual export/review in MVP

## Risk 3 - cost creep
Mitigation:
- offline indexing
- caching
- limited source set
- batch refreshes

## Risk 4 - overpromising
Mitigation:
- market as lead discovery, not person tracking

---

## 18. Acceptance criteria

This feature is MVP-ready when:
- [ ] tenant can draw/select a map area
- [ ] frontend can submit geometry to backend
- [ ] geo-leads-service validates and stores query
- [ ] service returns ranked public tourism/business leads in that area
- [ ] each lead includes at least one source + explanation
- [ ] CSV export works
- [ ] result latency is acceptable (<10s for indexed data path)
- [ ] scraping/indexing cost is low and bounded
- [ ] no prohibited/private data source is required for core value

---

## 19. Final recommendation

Build this feature as:
- **Geo Lead Discovery**
- based on **public tourism/business lead discovery**
- not as a personal traveler tracking tool

That gives you:
- a lower-cost MVP
- something legally safer
- something that actually fits into a SaaS travel sales workflow
- a service that can later plug into communication and CRM/inquiry conversion flows

Bluntly: if you scope this as “find tourists,” it gets creepy, brittle, and expensive.
If you scope it as “find public travel leads in a selected area,” it becomes a real product.
