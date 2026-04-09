using FluentAssertions;
using TravelService.Application.Abstractions;
using TravelService.Application.Commands.CreateQuotation;
using TravelService.Application.Commands.EntityNotes;
using TravelService.Application.Commands.SendQuotation;
using TravelService.Domain.Aggregates;
using TravelService.Domain.Repositories;

namespace TravelService.Tests;

public sealed class EntitlementEnforcementTests
{
    [Fact]
    public async Task CreateQuotation_ShouldFail_WhenFeatureDisabled()
    {
        var handler = new CreateQuotationCommandHandler(new InMemoryQuotationRepository(), new DenyFeatureGate(), new NoOpUnitOfWork());
        var command = new CreateQuotationCommand(Guid.NewGuid(), Guid.NewGuid(), "Ava", "Trip", "Istanbul", DateTimeOffset.UtcNow.AddDays(10), DateTimeOffset.UtcNow.AddDays(15), 2, "USD", "notes", []);

        var act = async () => await handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<TravelService.Domain.Exceptions.DomainException>().WithMessage("*not enabled*");
    }

    [Fact]
    public async Task SendQuotation_ShouldFail_WhenFeatureDisabled()
    {
        var quotation = Quotation.Create(Guid.NewGuid(), Guid.NewGuid(), "Ava", "Trip", "Rome", DateTimeOffset.UtcNow.AddDays(10), DateTimeOffset.UtcNow.AddDays(15), 2, "USD", "notes");
        quotation.AddLineItem("Hotel", 100m, 1, "USD");
        var revision = quotation.CreateRevision("visible", "internal");
        var handler = new SendQuotationCommandHandler(
            new SingleQuotationRepository(quotation),
            new SingleRevisionRepository(revision),
            new InMemoryShareLinkRepository(),
            new InMemoryStatusHistoryRepository(),
            new InMemoryApprovalRepository(),
            new DenyFeatureGate(),
            new NoOpActivityWriter(),
            new FakeActorContext(quotation.TenantId),
            new NoOpUnitOfWork());

        var act = async () => await handler.Handle(new SendQuotationCommand(quotation.TenantId, quotation.Id, revision.Id, "Email", "test@example.com", null, DateTimeOffset.UtcNow.AddDays(2)), CancellationToken.None);

        await act.Should().ThrowAsync<TravelService.Domain.Exceptions.DomainException>().WithMessage("*not enabled*");
    }

    [Fact]
    public async Task CreateEntityNote_ShouldFail_WhenFeatureDisabled()
    {
        var handler = new CreateEntityNoteCommandHandler(new InMemoryEntityNoteRepository(), new DenyFeatureGate(), new FakeActorContext(Guid.NewGuid()), new NoOpActivityWriter(), new NoOpUnitOfWork());

        var act = async () => await handler.Handle(new CreateEntityNoteCommand(Guid.NewGuid(), "Quotation", Guid.NewGuid(), "Internal", "blocked"), CancellationToken.None);

        await act.Should().ThrowAsync<TravelService.Domain.Exceptions.DomainException>().WithMessage("*not enabled*");
    }

    private sealed class DenyFeatureGate : IFeatureGate
    {
        public Task EnsureEnabledAsync(string featureKey, Guid tenantId, CancellationToken cancellationToken)
            => throw new TravelService.Domain.Exceptions.DomainException($"Feature '{featureKey}' is not enabled for tenant '{tenantId}'.");
        public Task<bool> IsEnabledAsync(string featureKey, Guid tenantId, CancellationToken cancellationToken) => Task.FromResult(false);
        public Task<int?> GetLimitAsync(string featureKey, Guid tenantId, CancellationToken cancellationToken) => Task.FromResult<int?>(null);
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

    private sealed class InMemoryQuotationRepository : IQuotationRepository
    {
        public Task AddAsync(Quotation quotation, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task<Quotation?> GetByIdAsync(Guid id, CancellationToken cancellationToken) => Task.FromResult<Quotation?>(null);
        public Task<IReadOnlyList<Quotation>> ListByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken) => Task.FromResult<IReadOnlyList<Quotation>>([]);
        public Task UpdateAsync(Quotation quotation, CancellationToken cancellationToken) => Task.CompletedTask;
    }

    private sealed class SingleQuotationRepository(Quotation quotation) : IQuotationRepository
    {
        public Task AddAsync(Quotation quotation, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task<Quotation?> GetByIdAsync(Guid id, CancellationToken cancellationToken) => Task.FromResult(id == quotation.Id ? quotation : null);
        public Task<IReadOnlyList<Quotation>> ListByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken) => Task.FromResult<IReadOnlyList<Quotation>>([quotation]);
        public Task UpdateAsync(Quotation quotation, CancellationToken cancellationToken) => Task.CompletedTask;
    }

    private sealed class SingleRevisionRepository(QuotationRevision revision) : IQuotationRevisionRepository
    {
        public Task AddAsync(QuotationRevision revision, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task<QuotationRevision?> GetByIdAsync(Guid quotationId, Guid revisionId, CancellationToken cancellationToken) => Task.FromResult(quotationId == revision.QuotationId && revisionId == revision.Id ? revision : null);
        public Task<IReadOnlyList<QuotationRevision>> ListByQuotationIdAsync(Guid quotationId, CancellationToken cancellationToken) => Task.FromResult<IReadOnlyList<QuotationRevision>>([revision]);
    }

    private sealed class InMemoryShareLinkRepository : IQuotationShareLinkRepository
    {
        public Task AddAsync(QuotationShareLink shareLink, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task<QuotationShareLink?> GetActiveByTokenAsync(string token, CancellationToken cancellationToken) => Task.FromResult<QuotationShareLink?>(null);
        public Task<IReadOnlyList<QuotationShareLink>> ListByQuotationIdAsync(Guid quotationId, CancellationToken cancellationToken) => Task.FromResult<IReadOnlyList<QuotationShareLink>>([]);
        public Task UpdateAsync(QuotationShareLink shareLink, CancellationToken cancellationToken) => Task.CompletedTask;
    }

    private sealed class InMemoryStatusHistoryRepository : IQuotationStatusHistoryRepository
    {
        public Task AddAsync(QuotationStatusHistory history, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task<IReadOnlyList<QuotationStatusHistory>> ListByQuotationIdAsync(Guid quotationId, CancellationToken cancellationToken) => Task.FromResult<IReadOnlyList<QuotationStatusHistory>>([]);
    }

    private sealed class InMemoryApprovalRepository : IQuotationApprovalRequestRepository
    {
        public Task AddAsync(QuotationApprovalRequest request, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task<QuotationApprovalRequest?> GetByIdAsync(Guid quotationId, Guid approvalRequestId, CancellationToken cancellationToken) => Task.FromResult<QuotationApprovalRequest?>(null);
        public Task<IReadOnlyList<QuotationApprovalRequest>> ListByQuotationIdAsync(Guid quotationId, CancellationToken cancellationToken) => Task.FromResult<IReadOnlyList<QuotationApprovalRequest>>([]);
        public Task UpdateAsync(QuotationApprovalRequest request, CancellationToken cancellationToken) => Task.CompletedTask;
    }

    private sealed class InMemoryEntityNoteRepository : IEntityNoteRepository
    {
        public Task AddAsync(EntityNote note, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task<EntityNote?> GetByIdAsync(Guid noteId, CancellationToken cancellationToken) => Task.FromResult<EntityNote?>(null);
        public Task<IReadOnlyList<EntityNote>> ListByEntityAsync(Guid tenantId, string entityType, Guid entityId, CancellationToken cancellationToken) => Task.FromResult<IReadOnlyList<EntityNote>>([]);
        public Task UpdateAsync(EntityNote note, CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
