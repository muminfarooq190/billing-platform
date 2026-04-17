using FluentAssertions;
using TravelService.Application.Abstractions;
using TravelService.Application.Commands.TravelInquiries;
using TravelService.Domain.Aggregates;
using TravelService.Domain.Repositories;

namespace TravelService.Tests;

public sealed class TravelInquiryConversionTests
{
    [Fact]
    public async Task ConvertInquiryToQuotation_ShouldCreateContactAndQuotation_AndMarkInquiryQuoted()
    {
        var inquiry = TravelInquiry.Create(Guid.NewGuid(), "Website", "Jane Doe", "jane@example.com", "+911234567890", null, "Mumbai", "Bali", DateTimeOffset.UtcNow.AddDays(20), DateTimeOffset.UtcNow.AddDays(25), false, 2, 150000m, "INR", "Need honeymoon package");
        var inquiryRepository = new InMemoryInquiryRepository(inquiry);
        var historyRepository = new InMemoryInquiryHistoryRepository();
        var contactRepository = new InMemoryContactRepository();
        var quotationRepository = new InMemoryQuotationRepository();
        var handler = new ConvertInquiryToQuotationCommandHandler(inquiryRepository, historyRepository, contactRepository, quotationRepository, new NoOpActivityWriter(), new NoOpAuditWriter(), new FakeActorContext(inquiry.TenantId), new NoOpUnitOfWork());

        var result = await handler.Handle(new ConvertInquiryToQuotationCommand(inquiry.TenantId, inquiry.Id, null, "Bali Honeymoon", "INR", "Sales qualified", null, true), CancellationToken.None);

        result.InquiryId.Should().Be(inquiry.Id);
        result.ContactId.Should().NotBeEmpty();
        result.QuotationId.Should().NotBeEmpty();
        inquiry.Status.ToString().Should().Be("Quoted");
        inquiry.ConvertedContactId.Should().Be(result.ContactId);
        inquiry.ConvertedQuotationId.Should().Be(result.QuotationId);
        contactRepository.Items.Should().ContainSingle();
        quotationRepository.Items.Should().ContainSingle();
        quotationRepository.Items[0].Destination.Should().Be("Bali");
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

    private sealed class InMemoryContactRepository : IContactRepository
    {
        public List<Contact> Items { get; } = [];
        public Task AddAsync(Contact contact, CancellationToken cancellationToken)
        {
            Items.Add(contact);
            return Task.CompletedTask;
        }
        public Task<Contact?> GetByIdAsync(Guid id, CancellationToken cancellationToken) => Task.FromResult(Items.SingleOrDefault(x => x.Id == id));
        public Task UpdateAsync(Contact contact, CancellationToken cancellationToken) => Task.CompletedTask;
    }

    private sealed class InMemoryQuotationRepository : IQuotationRepository
    {
        public List<Quotation> Items { get; } = [];
        public Task AddAsync(Quotation quotation, CancellationToken cancellationToken)
        {
            Items.Add(quotation);
            return Task.CompletedTask;
        }
        public Task<Quotation?> GetByIdAsync(Guid id, CancellationToken cancellationToken) => Task.FromResult(Items.SingleOrDefault(x => x.Id == id));
        public Task<IReadOnlyList<Quotation>> ListByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken) => Task.FromResult<IReadOnlyList<Quotation>>(Items);
        public Task UpdateAsync(Quotation quotation, CancellationToken cancellationToken) => Task.CompletedTask;
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
