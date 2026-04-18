using MediatR;

namespace GeoLeadsService.Application.Commands.IngestLeadSources;

public sealed record IngestLeadSourcesCommand : IRequest<int>;
