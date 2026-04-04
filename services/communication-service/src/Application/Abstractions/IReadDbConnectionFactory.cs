using System.Data;

namespace CommunicationService.Application.Abstractions;

public interface IReadDbConnectionFactory
{
    Task<IDbConnection> CreateOpenConnectionAsync(CancellationToken cancellationToken);
}
