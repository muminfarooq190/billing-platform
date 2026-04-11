using CommunicationService.Application.Abstractions;
using Dapper;
using MediatR;

namespace CommunicationService.Application.Queries.GetUnreadNotificationCount;

public sealed class GetUnreadNotificationCountQueryHandler(IReadDbConnectionFactory connectionFactory, IFeatureGate featureGate) : IRequestHandler<GetUnreadNotificationCountQuery, int>
{
    public async Task<int> Handle(GetUnreadNotificationCountQuery request, CancellationToken cancellationToken)
    {
        await featureGate.EnsureEnabledAsync(FeatureKeys.CommunicationLogsRead, request.TenantId, cancellationToken);

        await using var connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken) as IAsyncDisposable;
        var dbConnection = (System.Data.IDbConnection)connection!;
        return await dbConnection.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM notifications WHERE tenant_id = @TenantId AND recipient_id = @RecipientId AND read_at IS NULL",
            new { request.TenantId, request.RecipientId });
    }
}
