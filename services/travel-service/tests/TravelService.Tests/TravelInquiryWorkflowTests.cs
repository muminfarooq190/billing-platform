using FluentAssertions;
using TravelService.Application.Abstractions;
using TravelService.Application.Commands.TravelInquiries;
using TravelService.Domain.Aggregates;
using TravelService.Domain.Repositories;

namespace TravelService.Tests;

public sealed class TravelInquiryWorkflowTests
{
    [Fact]
    public async Task QualifyInquiry_ShouldUpdateStatus_AndWriteHistory()
    {
        var inquiry = TravelInquiry.Create(Guid.NewGuid(), "Website", "Jane Doe", "jane@example.com", "+911234567890", null, "Mumbai", "Bali", DateTimeOffset.UtcNow.AddDays(10), DateTimeOffset.UtcNow.AddDays(15), false, 2, 1000m, "USD", "Trip request");
        var inquiryRepository = new InMemoryInquiryRepository(inquiry);
        var historyRepository = new InMemoryInquiryHistoryRepository();
        var handler = new QualifyInquiryCommandHandler(inquiryRepository, historyRepository, new NoOpActivityWriter(), new NoOpAuditWriter(), new FakeActorContext(inquiry.TenantId), new NoOpUnitOfWork());

        await handler.Handle(new QualifyInquiryCommand(inquiry.TenantId, inquiry.Id, "Looks legit"), CancellationToken.None);

        inquiry.Status.ToString().Should().Be("Qualified");
        historyRepository.Items.Should().ContainSingle();
        historyRepository.Items[0].ToStatus.Should().Be("Qualified");
    }

    [Fact]
    public async Task MarkContactedInquiry_ShouldMoveToContacted()
    {
        var inquiry = TravelInquiry.Create(Guid.NewGuid(), "Website", "Jane Doe", "jane@example.com", "+911234567890", null, "Mumbai", "Bali", DateTimeOffset.UtcNow.AddDays(10), DateTimeOffset.UtcNow.AddDays(15), false, 2, 1000m, "USD", "Trip request");
        var handler = new MarkInquiryContactedCommandHandler(new InMemoryInquiryRepository(inquiry), new InMemoryInquiryHistoryRepository(), new NoOpActivityWriter(), new NoOpAuditWriter(), new FakeActorContext(inquiry.TenantId), new NoOpUnitOfWork());

        await handler.Handle(new MarkInquiryContactedCommand(inquiry.TenantId, inquiry.Id, "Called customer"), CancellationToken.None);

        inquiry.Status.ToString().Should().Be("Contacted");
        inquiry.ContactedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task DisqualifyInquiry_ShouldSupportSpam()
    {
        var inquiry = TravelInquiry.Create(Guid.NewGuid(), "Website", "Jane Doe", "jane@example.com", "+911234567890", null, "Mumbai", "Bali", DateTimeOffset.UtcNow.AddDays(10), DateTimeOffset.UtcNow.AddDays(15), false, 2, 1000m, "USD", "Trip request");
        var handler = new DisqualifyInquiryCommandHandler(new InMemoryInquiryRepository(inquiry), new InMemoryInquiryHistoryRepository(), new NoOpActivityWriter(), new NoOpAuditWriter(), new FakeActorContext(inquiry.TenantId), new NoOpUnitOfWork());

        await handler.Handle(new DisqualifyInquiryCommand(inquiry.TenantId, inquiry.Id, "Spam", "Obvious junk"), CancellationToken.None);

        inquiry.Status.ToString().Should().Be("Spam");
    }

    private sealed class InMemoryInquiryRepository(TravelInquiry inquiry) : ITravelInquiryRepository
    {
        public Task AddAsync(TravelInquiry inquiry, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task<TravelInquiry?> GetByIdAsync(Guid id, CancellationToken cancellationToken) => Task.FromResult(id == inquiry.Id ? inquiry : null);
        public Task UpdateAsync(TravelInquiry inquiry, CancellationToken cancellationToken) => Task.CompletedTask;
    }

    private sealed class InMemoryInquiryHistoryRepository : ITravelInquiryStatusHistoryRepository
    {
        public List<TravelInquiryStatusHistory> Items { get; } = [];
        public Task AddAsync(TravelInquiryStatusHistory entry, CancellationToken cancellationToken)
        {
            Items.Add(entry);
            return Task.CompletedTask;
        }
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

    private sealed class NoOpAuditWriter : IAuditWriter
    {
        public Task WriteAsync(AuditLog entry, CancellationToken cancellationToken) => Task.CompletedTask;
    }

    private sealed class NoOpUnitOfWork : IUnitOfWork
    {
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken) => Task.FromResult(1);
    }
}
