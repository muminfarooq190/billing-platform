using Dapper;
using MediatR;
using TravelService.Application.Abstractions;

namespace TravelService.Application.Queries.EntityNotes;

public sealed class ListEntityNotesQueryHandler(IReadDbConnectionFactory connectionFactory) : IRequestHandler<ListEntityNotesQuery, IReadOnlyList<EntityNoteReadModel>>
{
    public async Task<IReadOnlyList<EntityNoteReadModel>> Handle(ListEntityNotesQuery request, CancellationToken cancellationToken)
    {
        using var connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken);

        var items = await connection.QueryAsync<EntityNoteReadModel>(
            @"SELECT id,
                      entity_type AS EntityType,
                      entity_id AS EntityId,
                      visibility,
                      content,
                      created_by_user_id AS CreatedByUserId,
                      created_at AS CreatedAt,
                      updated_at AS UpdatedAt
               FROM entity_notes
               WHERE tenant_id = @TenantId AND entity_type = @EntityType AND entity_id = @EntityId AND deleted_at IS NULL
               ORDER BY created_at DESC",
            new { request.TenantId, request.EntityType, request.EntityId });

        return items.ToList();
    }
}
