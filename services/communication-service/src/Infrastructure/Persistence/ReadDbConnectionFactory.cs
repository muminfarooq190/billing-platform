using System.Data;
using CommunicationService.Application.Abstractions;
using Npgsql;

namespace CommunicationService.Infrastructure.Persistence;

public sealed class ReadDbConnectionFactory(IConfiguration configuration) : IReadDbConnectionFactory
{
    public async Task<IDbConnection> CreateOpenConnectionAsync(CancellationToken cancellationToken)
    {
        var connection = new NpgsqlConnection(configuration["DATABASE_URL"]);
        await connection.OpenAsync(cancellationToken);
        return connection;
    }
}
