using MediatR;

namespace TravelService.Application.Queries.EntityNotes;

public sealed record ListEntityNotesQuery(Guid TenantId, string EntityType, Guid EntityId) : IRequest<IReadOnlyList<EntityNoteReadModel>>;

public sealed record EntityNoteReadModel(
    Guid Id,
    string EntityType,
    Guid EntityId,
    string Visibility,
    string Content,
    Guid? CreatedByUserId,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
