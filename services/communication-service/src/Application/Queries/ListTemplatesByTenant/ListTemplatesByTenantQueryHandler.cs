using Dapper;
using CommunicationService.Application.Abstractions;
using CommunicationService.Application.Queries.GetTemplateById;
using MediatR;

namespace CommunicationService.Application.Queries.ListTemplatesByTenant;

public sealed class ListTemplatesByTenantQueryHandler(IReadDbConnectionFactory connectionFactory) : IRequestHandler<ListTemplatesByTenantQuery, IReadOnlyList<TemplateReadModel>>
{
    public async Task<IReadOnlyList<TemplateReadModel>> Handle(ListTemplatesByTenantQuery request, CancellationToken cancellationToken)
    {
        await using var connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken) as IAsyncDisposable;
        var dbConnection = (System.Data.IDbConnection)connection!;
        var results = await dbConnection.QueryAsync<TemplateReadModel>(
            "SELECT id, tenant_id AS TenantId, name, subject, body_template AS BodyTemplate, channel, description, status, created_at AS CreatedAt, updated_at AS UpdatedAt FROM notification_templates WHERE tenant_id = @TenantId AND deleted_at IS NULL ORDER BY name",
            new { request.TenantId });
        return results.ToList().AsReadOnly();
    }
}
