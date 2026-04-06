using Dapper;
using MediatR;
using TravelService.Application.Abstractions;

namespace TravelService.Application.Queries.Contacts;

public sealed class GetContactByIdQueryHandler(IReadDbConnectionFactory connectionFactory) : IRequestHandler<GetContactByIdQuery, ContactReadModel?>
{
    public async Task<ContactReadModel?> Handle(GetContactByIdQuery request, CancellationToken cancellationToken)
    {
        await using var connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken) as IAsyncDisposable;
        var dbConnection = (System.Data.IDbConnection)connection!;

        return await dbConnection.QuerySingleOrDefaultAsync<ContactReadModel>(
            "SELECT id, tenant_id AS TenantId, first_name AS FirstName, last_name AS LastName, email, phone, company, notes, tags, created_at AS CreatedAt, updated_at AS UpdatedAt FROM contacts WHERE id = @Id AND deleted_at IS NULL",
            new { request.Id });
    }
}
