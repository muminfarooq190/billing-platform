using MediatR;

namespace GeoLeadsService.Application.Commands.DeleteSavedGeoArea;

public sealed record DeleteSavedGeoAreaCommand(Guid TenantId, Guid AreaId) : IRequest<bool>;
