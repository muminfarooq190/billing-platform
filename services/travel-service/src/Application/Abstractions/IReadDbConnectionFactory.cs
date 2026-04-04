using System.Data;

namespace TravelService.Application.Abstractions;

public interface IReadDbConnectionFactory
{
    Task<IDbConnection> CreateOpenConnectionAsync(CancellationToken cancellationToken);
}
