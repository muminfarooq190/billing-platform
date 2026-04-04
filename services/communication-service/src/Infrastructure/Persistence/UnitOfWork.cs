using System.Text.Json;
using CommunicationService.Application.Abstractions;
using CommunicationService.Domain.Common;

namespace CommunicationService.Infrastructure.Persistence;

public sealed class UnitOfWork(CommunicationDbContext dbContext) : IUnitOfWork
{
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        var aggregates = dbContext.ChangeTracker.Entries<AggregateRoot>()
            .Where(e => e.Entity.DomainEvents.Any())
            .Select(e => e.Entity)
            .ToList();

        foreach (var aggregate in aggregates)
        {
            foreach (var domainEvent in aggregate.DomainEvents)
            {
                dbContext.DomainEvents.Add(new OutboxMessage
                {
                    Id = Guid.NewGuid(),
                    AggregateType = aggregate.GetType().Name,
                    AggregateId = aggregate.GetType().GetProperty("Id")?.GetValue(aggregate) as Guid? ?? Guid.Empty,
                    EventType = domainEvent.GetType().Name,
                    Payload = JsonSerializer.Serialize(domainEvent),
                    CreatedAt = DateTimeOffset.UtcNow
                });
            }

            aggregate.ClearDomainEvents();
        }

        return await dbContext.SaveChangesAsync(cancellationToken);
    }
}
