using System.Data;

namespace IdentityService.Application.Abstractions;

public interface IReadDbConnectionFactory
{
    Task<IDbConnection> CreateOpenConnectionAsync(CancellationToken cancellationToken);
}
