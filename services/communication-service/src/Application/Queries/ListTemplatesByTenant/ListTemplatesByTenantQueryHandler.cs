using Dapper;
using CommunicationService.Application.Abstractions;
using CommunicationService.Application.Queries.GetTemplateById;
using MediatR;

namespace CommunicationService.Application.Queries.ListTemplatesByTenant;

public sealed class ListTemplatesByTenantQueryHandler(IReadDbConnectionFactory connectionFactory, IFeatureGate featureGate) : IRequestHandler<ListTemplatesByTenantQuery, IReadOnlyList<TemplateReadModel>>
{
    public async Task<IReadOnlyList<TemplateReadModel>> Handle(ListTemplatesByTenantQuery request, CancellationToken cancellationToken)
    {
        await featureGate.EnsureEnabledAsync(FeatureKeys.CommunicationTemplatesManage, request.TenantId, cancellationToken);

        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        await using var connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken) as IAsyncDisposable;
        var dbConnection = (System.Data.IDbConnection)connection!;
        var results = await dbConnection.QueryAsync<TemplateReadModel>(
            "SELECT id, tenant_id AS TenantId, name, subject, body_template AS BodyTemplate, channel, description, status, created_at AS CreatedAt, updated_at AS UpdatedAt FROM notification_templates WHERE tenant_id = @TenantId AND deleted_at IS NULL ORDER BY name OFFSET @Offset LIMIT @Limit",
            new { request.TenantId, Offset = (page - 1) * pageSize, Limit = pageSize });
        return results.ToList().AsReadOnly();
    }
}
