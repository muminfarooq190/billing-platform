using FluentAssertions;
using TravelService.Application.Abstractions;
using TravelService.Application.Commands.FollowUps;
using TravelService.Domain.Aggregates;
using TravelService.Domain.Enums;
using TravelService.Domain.Repositories;

namespace TravelService.Tests;

public sealed class FollowUpWorkflowTests
{
    [Fact]
    public async Task CompleteFollowUp_ShouldMarkCompleted()
    {
        var followUp = FollowUp.Create(Guid.NewGuid(), Guid.NewGuid(), "Jane Doe", "Call customer", "Check payment", FollowUpPriority.High, DateTimeOffset.UtcNow.AddDays(1), null);
        var handler = new CompleteFollowUpCommandHandler(new SingleFollowUpRepository(followUp), new NoOpActivityWriter(), new FakeActorContext(followUp.TenantId), new NoOpUnitOfWork());

        await handler.Handle(new CompleteFollowUpCommand(followUp.Id), CancellationToken.None);

        followUp.Status.Should().Be(FollowUpStatus.Completed);
    }

    [Fact]
    public async Task ReassignFollowUp_ShouldChangeAssignee()
    {
        var followUp = FollowUp.Create(Guid.NewGuid(), Guid.NewGuid(), "Jane Doe", "Call customer", "Check payment", FollowUpPriority.High, DateTimeOffset.UtcNow.AddDays(1), null);
        var assignee = Guid.NewGuid();
        var handler = new ReassignFollowUpCommandHandler(new SingleFollowUpRepository(followUp), new NoOpActivityWriter(), new FakeActorContext(followUp.TenantId), new NoOpUnitOfWork());

        await handler.Handle(new ReassignFollowUpCommand(followUp.Id, assignee), CancellationToken.None);

        followUp.AssignedToUserId.Should().Be(assignee);
    }

    private sealed class SingleFollowUpRepository(FollowUp followUp) : IFollowUpRepository
    {
        public Task AddAsync(FollowUp followUp, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task<FollowUp?> GetByIdAsync(Guid id, CancellationToken cancellationToken) => Task.FromResult(id == followUp.Id ? followUp : null);
        public Task<IReadOnlyList<FollowUp>> ListByTenantIdAsync(Guid tenantId, int page, int pageSize, string? status, string? customerName, CancellationToken cancellationToken) => Task.FromResult<IReadOnlyList<FollowUp>>([followUp]);
        public Task UpdateAsync(FollowUp followUp, CancellationToken cancellationToken) => Task.CompletedTask;
    }

    private sealed class FakeActorContext(Guid tenantId) : IActorContext
    {
        public Guid? UserId { get; } = Guid.NewGuid();
        public Guid TenantId { get; } = tenantId;
        public string? IpAddress { get; } = "127.0.0.1";
        public string? UserAgent { get; } = "tests";
    }

    private sealed class NoOpActivityWriter : IActivityWriter
    {
        public Task WriteAsync(ActivityEntry entry, CancellationToken cancellationToken) => Task.CompletedTask;
    }

    private sealed class NoOpUnitOfWork : IUnitOfWork
    {
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken) => Task.FromResult(1);
    }
}
