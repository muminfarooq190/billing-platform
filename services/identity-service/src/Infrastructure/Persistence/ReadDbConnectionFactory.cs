using System.Data;
using IdentityService.Application.Abstractions;
using Npgsql;

namespace IdentityService.Infrastructure.Persistence;

public sealed class ReadDbConnectionFactory(IConfiguration configuration) : IReadDbConnectionFactory
{
    public async Task<IDbConnection> CreateOpenConnectionAsync(CancellationToken cancellationToken)
    {
        var connectionString = configuration["DATABASE_URL"] ?? throw new InvalidOperationException("DATABASE_URL is missing.");
        var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
        return connection;
    }
}
