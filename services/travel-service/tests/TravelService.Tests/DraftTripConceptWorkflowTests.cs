using FluentAssertions;
using TravelService.Application.Abstractions;
using TravelService.Application.Commands.DraftTripConcepts;
using TravelService.Domain.Aggregates;
using TravelService.Domain.Repositories;

namespace TravelService.Tests;

public sealed class DraftTripConceptWorkflowTests
{
    [Fact]
    public async Task CreateDraftTripConcept_ShouldPersistConcept()
    {
        var inquiry = TravelInquiry.Create(Guid.NewGuid(), "Website", "Jane Doe", "jane@example.com", "+911234567890", null, "Mumbai", "Bali", DateTimeOffset.UtcNow.AddDays(20), DateTimeOffset.UtcNow.AddDays(25), false, 2, 150000m, "INR", "Need honeymoon package");
        var inquiryRepository = new InMemoryInquiryRepository(inquiry);
        var conceptRepository = new InMemoryConceptRepository();
        var handler = new CreateDraftTripConceptCommandHandler(inquiryRepository, conceptRepository, new NoOpActivityWriter(), new NoOpAuditWriter(), new FakeActorContext(inquiry.TenantId), new NoOpUnitOfWork());

        var conceptId = await handler.Handle(new CreateDraftTripConceptCommand(
            inquiry.TenantId,
            inquiry.Id,
            "Bali Honeymoon Option A",
            "Bali",
            "Beach + Ubud split",
            inquiry.TravelDate,
            inquiry.ReturnDate,
            inquiry.Travellers,
            inquiry.BudgetCurrency,
            inquiry.BudgetAmount,
            "Option A",
            "Premium positioning",
            Guid.NewGuid(),
            [new CreateDraftTripConceptDayDto(1, "Arrival", "Airport pickup", "Bali", "Seminyak")]), CancellationToken.None);

        conceptId.Should().NotBeEmpty();
        conceptRepository.Items.Should().ContainSingle();
        conceptRepository.Items[0].Days.Should().ContainSingle();
    }

    [Fact]
    public async Task MarkPrimary_ShouldClearOtherPrimaryConcepts()
    {
        var inquiry = TravelInquiry.Create(Guid.NewGuid(), "Website", "Jane Doe", "jane@example.com", "+911234567890", null, "Mumbai", "Bali", DateTimeOffset.UtcNow.AddDays(20), DateTimeOffset.UtcNow.AddDays(25), false, 2, 150000m, "INR", "Need honeymoon package");
        var first = DraftTripConcept.Create(inquiry.TenantId, inquiry.Id, "Option A", "Bali", null, null, null, 2, "USD", null, null, null, null);
        var second = DraftTripConcept.Create(inquiry.TenantId, inquiry.Id, "Option B", "Bali", null, null, null, 2, "USD", null, null, null, null);
        first.MarkPrimary();
        var handler = new MarkPrimaryDraftTripConceptCommandHandler(new InMemoryInquiryRepository(inquiry), new InMemoryConceptRepository(first, second), new NoOpActivityWriter(), new NoOpAuditWriter(), new FakeActorContext(inquiry.TenantId), new NoOpUnitOfWork());

        await handler.Handle(new MarkPrimaryDraftTripConceptCommand(inquiry.TenantId, inquiry.Id, second.Id), CancellationToken.None);

        first.IsPrimary.Should().BeFalse();
        second.IsPrimary.Should().BeTrue();
    }

    private sealed class InMemoryInquiryRepository(TravelInquiry inquiry) : ITravelInquiryRepository
    {
        public Task AddAsync(TravelInquiry inquiry, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task<TravelInquiry?> GetByIdAsync(Guid id, CancellationToken cancellationToken) => Task.FromResult(id == inquiry.Id ? inquiry : null);
        public Task UpdateAsync(TravelInquiry inquiry, CancellationToken cancellationToken) => Task.CompletedTask;
    }

    private sealed class InMemoryConceptRepository(params DraftTripConcept[] concepts) : IDraftTripConceptRepository
    {
        public List<DraftTripConcept> Items { get; } = concepts.ToList();
        public Task AddAsync(DraftTripConcept concept, CancellationToken cancellationToken)
        {
            Items.Add(concept);
            return Task.CompletedTask;
        }
        public Task<DraftTripConcept?> GetByIdAsync(Guid id, CancellationToken cancellationToken) => Task.FromResult(Items.SingleOrDefault(x => x.Id == id));
        public Task<IReadOnlyList<DraftTripConcept>> ListByInquiryIdAsync(Guid inquiryId, CancellationToken cancellationToken) => Task.FromResult<IReadOnlyList<DraftTripConcept>>(Items.Where(x => x.TravelInquiryId == inquiryId).ToList());
        public Task UpdateAsync(DraftTripConcept concept, CancellationToken cancellationToken) => Task.CompletedTask;
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
