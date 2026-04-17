using FluentAssertions;
using TravelService.Application.Abstractions;
using TravelService.Application.Commands.TravelInquiries;
using TravelService.Domain.Aggregates;
using TravelService.Domain.Repositories;

namespace TravelService.Tests;

public sealed class PublicInquiryCommandTests
{
    [Fact]
    public async Task CreatePublicInquiry_ShouldPersistInquiry_AndInitialHistory()
    {
        var inquiryRepository = new InMemoryInquiryRepository();
        var historyRepository = new InMemoryInquiryHistoryRepository();
        var handler = new CreatePublicInquiryCommandHandler(inquiryRepository, historyRepository, new NoOpActivityWriter(), new NoOpAuditWriter(), new NoOpUnitOfWork());

        var inquiryId = await handler.Handle(new CreatePublicInquiryCommand(
            Guid.NewGuid(),
            "Jane Doe",
            "jane@example.com",
            "+911234567890",
            null,
            "Mumbai",
            "Bali",
            DateTimeOffset.UtcNow.AddDays(30),
            DateTimeOffset.UtcNow.AddDays(35),
            false,
            2,
            150000m,
            "INR",
            "Need honeymoon package",
            "Website",
            "127.0.0.1",
            "tests"), CancellationToken.None);

        inquiryId.Should().NotBeEmpty();
        inquiryRepository.Items.Should().ContainSingle();
        historyRepository.Items.Should().ContainSingle();
        historyRepository.Items[0].ToStatus.Should().Be("New");
    }

    private sealed class InMemoryInquiryRepository : ITravelInquiryRepository
    {
        public List<TravelInquiry> Items { get; } = [];
        public Task AddAsync(TravelInquiry inquiry, CancellationToken cancellationToken)
        {
            Items.Add(inquiry);
            return Task.CompletedTask;
        }
        public Task<TravelInquiry?> GetByIdAsync(Guid id, CancellationToken cancellationToken) => Task.FromResult(Items.SingleOrDefault(x => x.Id == id));
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
