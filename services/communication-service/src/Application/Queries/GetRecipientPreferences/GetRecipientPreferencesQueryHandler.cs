using Dapper;
using CommunicationService.Application.Abstractions;
using MediatR;

namespace CommunicationService.Application.Queries.GetRecipientPreferences;

public sealed class GetRecipientPreferencesQueryHandler(IReadDbConnectionFactory connectionFactory) : IRequestHandler<GetRecipientPreferencesQuery, RecipientPreferencesReadModel?>
{
    public async Task<RecipientPreferencesReadModel?> Handle(GetRecipientPreferencesQuery request, CancellationToken cancellationToken)
    {
        await using var connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken) as IAsyncDisposable;
        var dbConnection = (System.Data.IDbConnection)connection!;
        return await dbConnection.QuerySingleOrDefaultAsync<RecipientPreferencesReadModel>(
            "SELECT id, tenant_id AS TenantId, recipient_id AS RecipientId, recipient_type AS RecipientType, email, phone, device_token AS DeviceToken, timezone, channel_preferences_json AS ChannelPreferencesJson, created_at AS CreatedAt, updated_at AS UpdatedAt FROM recipient_preferences WHERE recipient_id = @RecipientId AND tenant_id = @TenantId",
            new { request.RecipientId, request.TenantId });
    }
}
