using System.Data;

namespace BillingService.Application.Abstractions;

public interface IReadDbConnectionFactory
{
    Task<IDbConnection> CreateOpenConnectionAsync(CancellationToken cancellationToken);
}
