using System.Data;
using BillingService.Application.Abstractions;
using Npgsql;

namespace BillingService.Infrastructure.Persistence;

public sealed class ReadDbConnectionFactory(IConfiguration configuration) : IReadDbConnectionFactory
{
    public async Task<IDbConnection> CreateOpenConnectionAsync(CancellationToken cancellationToken)
    {
        var connection = new NpgsqlConnection(configuration["DATABASE_URL"]);
        await connection.OpenAsync(cancellationToken);
        return connection;
    }
}
