using System.Data;
using TravelService.Application.Abstractions;
using Npgsql;

namespace TravelService.Infrastructure.Persistence;

public sealed class ReadDbConnectionFactory(IConfiguration configuration) : IReadDbConnectionFactory
{
    public async Task<IDbConnection> CreateOpenConnectionAsync(CancellationToken cancellationToken)
    {
        var connection = new NpgsqlConnection(configuration["DATABASE_URL"]);
        await connection.OpenAsync(cancellationToken);
        return connection;
    }
}
