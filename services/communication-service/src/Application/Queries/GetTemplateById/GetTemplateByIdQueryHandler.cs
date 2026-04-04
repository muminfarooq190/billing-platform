using Dapper;
using CommunicationService.Application.Abstractions;
using MediatR;

namespace CommunicationService.Application.Queries.GetTemplateById;

public sealed class GetTemplateByIdQueryHandler(IReadDbConnectionFactory connectionFactory) : IRequestHandler<GetTemplateByIdQuery, TemplateReadModel?>
{
    public async Task<TemplateReadModel?> Handle(GetTemplateByIdQuery request, CancellationToken cancellationToken)
    {
        await using var connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken) as IAsyncDisposable;
        var dbConnection = (System.Data.IDbConnection)connection!;
        return await dbConnection.QuerySingleOrDefaultAsync<TemplateReadModel>(
            "SELECT id, tenant_id AS TenantId, name, subject, body_template AS BodyTemplate, channel, description, status, created_at AS CreatedAt, updated_at AS UpdatedAt FROM notification_templates WHERE id = @Id AND deleted_at IS NULL",
            new { request.Id });
    }
}
