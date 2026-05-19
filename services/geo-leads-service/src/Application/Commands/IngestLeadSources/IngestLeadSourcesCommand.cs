using GeoLeadsService.Application.Abstractions;
using MediatR;

namespace GeoLeadsService.Application.Commands.IngestLeadSources;

/// <summary>
/// Trigger ingestion across all configured adapters.
/// When <paramref name="BoundingBox"/> is supplied, geography-aware adapters
/// (Overpass etc.) restrict their fetch to that area. Static adapters ignore it.
/// </summary>
public sealed record IngestLeadSourcesCommand(GeoBoundingBox? BoundingBox = null) : IRequest<int>;
