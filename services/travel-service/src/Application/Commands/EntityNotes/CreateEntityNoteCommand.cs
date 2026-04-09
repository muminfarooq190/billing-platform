using MediatR;

namespace TravelService.Application.Commands.EntityNotes;

public sealed record CreateEntityNoteCommand(Guid TenantId, string EntityType, Guid EntityId, string Visibility, string Content) : IRequest<Guid>;
