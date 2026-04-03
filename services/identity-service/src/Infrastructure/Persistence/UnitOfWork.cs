using System.Text.Json;
using IdentityService.Application.Abstractions;
using IdentityService.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Infrastructure.Persistence;

public sealed class UnitOfWork(IdentityDbContext dbContext) : IUnitOfWork
{
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        AppendOutboxMessages();
        return await dbContext.SaveChangesAsync(cancellationToken);
    }

    private void AppendOutboxMessages()
    {
        var aggregates = dbContext.ChangeTracker.Entries<AggregateRoot>()
            .Where(e => e.Entity.DomainEvents.Count > 0)
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
                    AggregateId = TryResolveAggregateId(aggregate),
                    EventType = domainEvent.GetType().Name,
                    Payload = JsonSerializer.Serialize(domainEvent),
                    CreatedAt = DateTimeOffset.UtcNow
                });
            }

            aggregate.ClearDomainEvents();
        }
    }

    private static Guid TryResolveAggregateId(object aggregate)
    {
        var idProperty = aggregate.GetType().GetProperty("Id");
        return idProperty?.GetValue(aggregate) as Guid? ?? Guid.Empty;
    }
}
