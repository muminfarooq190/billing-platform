using FluentAssertions;
using TravelService.Application.Abstractions;
using TravelService.Application.Commands.AcceptQuotation;
using TravelService.Application.Commands.FollowUps;
using TravelService.Application.Commands.RejectQuotation;
using TravelService.Application.Queries.GetBookingFinancialSummary;
using TravelService.Application.Queries.GetWorkQueue;
using TravelService.Domain.Aggregates;
using TravelService.Domain.Enums;
using TravelService.Domain.Repositories;

namespace TravelService.Tests;

public sealed class AdditionalEntitlementEnforcementTests
{
    [Fact]
    public async Task AcceptQuotation_ShouldFail_WhenFeatureDisabled()
    {
        var quotation = CreateQuotation();
        quotation.AddLineItem("Hotel", 100m, 1, "USD");
        var revision = quotation.CreateRevision("Visible", "Internal");
        var handler = new AcceptQuotationCommandHandler(
            new SingleQuotationRepository(quotation),
            new SingleRevisionRepository(revision),
            new InMemoryQuotationStatusHistoryRepository(),
            new DenyFeatureGate(),
            new NoOpAuditWriter(),
            new FakeActorContext(quotation.TenantId),
            new NoOpUnitOfWork(), new FakeTenantContext());

        var act = async () => await handler.Handle(new AcceptQuotationCommand(quotation.TenantId, quotation.Id, revision.Id, "customer accepted"), CancellationToken.None);

        await act.Should().ThrowAsync<TravelService.Domain.Exceptions.DomainException>().WithMessage("*not enabled*");
    }

    [Fact]
    public async Task RejectQuotation_ShouldFail_WhenFeatureDisabled()
    {
        var quotation = CreateQuotation();
        var handler = new RejectQuotationCommandHandler(
            new SingleQuotationRepository(quotation),
            new InMemoryQuotationStatusHistoryRepository(),
            new DenyFeatureGate(),
            new NoOpAuditWriter(),
            new FakeActorContext(quotation.TenantId),
            new NoOpUnitOfWork(), new FakeTenantContext());

        var act = async () => await handler.Handle(new RejectQuotationCommand(quotation.TenantId, quotation.Id, "customer rejected"), CancellationToken.None);

        await act.Should().ThrowAsync<TravelService.Domain.Exceptions.DomainException>().WithMessage("*not enabled*");
    }

    [Fact]
    public async Task CompleteFollowUp_ShouldFail_WhenFeatureDisabled()
    {
        var followUp = FollowUp.Create(Guid.NewGuid(), Guid.NewGuid(), "Jane Doe", "Call customer", "Check payment", FollowUpPriority.High, DateTimeOffset.UtcNow.AddDays(1), null);
        var handler = new CompleteFollowUpCommandHandler(new SingleFollowUpRepository(followUp), new DenyFeatureGate(), new NoOpActivityWriter(), new FakeActorContext(followUp.TenantId), new NoOpUnitOfWork(), new FakeTenantContext());

        var act = async () => await handler.Handle(new CompleteFollowUpCommand(followUp.Id), CancellationToken.None);

        await act.Should().ThrowAsync<TravelService.Domain.Exceptions.DomainException>().WithMessage("*not enabled*");
    }

    [Fact]
    public async Task ReassignFollowUp_ShouldFail_WhenFeatureDisabled()
    {
        var followUp = FollowUp.Create(Guid.NewGuid(), Guid.NewGuid(), "Jane Doe", "Call customer", "Check payment", FollowUpPriority.High, DateTimeOffset.UtcNow.AddDays(1), null);
        var handler = new ReassignFollowUpCommandHandler(new SingleFollowUpRepository(followUp), new DenyFeatureGate(), new NoOpActivityWriter(), new FakeActorContext(followUp.TenantId), new NoOpUnitOfWork(), new FakeTenantContext());

        var act = async () => await handler.Handle(new ReassignFollowUpCommand(followUp.Id, Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<TravelService.Domain.Exceptions.DomainException>().WithMessage("*not enabled*");
    }

    [Fact]
    public async Task GetWorkQueue_ShouldFail_WhenFeatureDisabled()
    {
        var handler = new GetWorkQueueQueryHandler(new ThrowingReadDbConnectionFactory(), new DenyFeatureGate());

        var act = async () => await handler.Handle(new GetWorkQueueQuery(Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<TravelService.Domain.Exceptions.DomainException>().WithMessage("*not enabled*");
    }

    [Fact]
    public async Task GetBookingFinancialSummary_ShouldFail_WhenFeatureDisabled()
    {
        var booking = Booking.CreateFromAcceptedQuotation(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "VOY-BKG-2026-0001", "Italy Trip", "Italy", DateTimeOffset.UtcNow.AddDays(10), DateTimeOffset.UtcNow.AddDays(15), 2, "USD", 5000m);
        var handler = new GetBookingFinancialSummaryQueryHandler(new SingleBookingRepository(booking), new StubBillingFinanceClient([]), new DenyFeatureGate());

        var act = async () => await handler.Handle(new GetBookingFinancialSummaryQuery(booking.TenantId, booking.Id), CancellationToken.None);

        await act.Should().ThrowAsync<TravelService.Domain.Exceptions.DomainException>().WithMessage("*not enabled*");
    }

    private static Quotation CreateQuotation()
        => Quotation.Create(Guid.NewGuid(), Guid.NewGuid(), "Ava", "Summer Trip", "Istanbul", DateTimeOffset.UtcNow.AddDays(20), DateTimeOffset.UtcNow.AddDays(25), 2, "USD", "notes");

    private sealed class DenyFeatureGate : IFeatureGate
    {
        public Task EnsureEnabledAsync(string featureKey, Guid tenantId, CancellationToken cancellationToken)
            => throw new TravelService.Domain.Exceptions.DomainException($"Feature '{featureKey}' is not enabled for tenant '{tenantId}'.");
        public Task EnsureEnabledAsync(string featureKey, Guid tenantId, Guid? userId, CancellationToken cancellationToken)
            => throw new TravelService.Domain.Exceptions.DomainException($"Feature '{featureKey}' is not enabled for tenant '{tenantId}' and user '{userId}'.");
        public Task<bool> IsEnabledAsync(string featureKey, Guid tenantId, CancellationToken cancellationToken) => Task.FromResult(false);
        public Task<bool> IsEnabledAsync(string featureKey, Guid tenantId, Guid? userId, CancellationToken cancellationToken) => Task.FromResult(false);
        public Task<int?> GetLimitAsync(string featureKey, Guid tenantId, CancellationToken cancellationToken) => Task.FromResult<int?>(null);
        public Task<int?> GetLimitAsync(string featureKey, Guid tenantId, Guid? userId, CancellationToken cancellationToken) => Task.FromResult<int?>(null);
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

    private sealed class InMemoryQuotationStatusHistoryRepository : IQuotationStatusHistoryRepository
    {
        public Task AddAsync(QuotationStatusHistory history, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task<IReadOnlyList<QuotationStatusHistory>> ListByQuotationIdAsync(Guid quotationId, CancellationToken cancellationToken) => Task.FromResult<IReadOnlyList<QuotationStatusHistory>>([]);
    }

    private sealed class SingleFollowUpRepository(FollowUp followUp) : IFollowUpRepository
    {
        public Task AddAsync(FollowUp followUp, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task<FollowUp?> GetByIdAsync(Guid id, CancellationToken cancellationToken) => Task.FromResult(id == followUp.Id ? followUp : null);
        public Task<IReadOnlyList<FollowUp>> ListByTenantIdAsync(Guid tenantId, int page, int pageSize, string? status, string? customerName, CancellationToken cancellationToken) => Task.FromResult<IReadOnlyList<FollowUp>>([followUp]);
        public Task<IReadOnlyList<FollowUp>> ListByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken) => Task.FromResult<IReadOnlyList<FollowUp>>([followUp]);
        public Task<IReadOnlyList<FollowUp>> ListOverdueAsync(DateTimeOffset asOf, CancellationToken cancellationToken) => Task.FromResult<IReadOnlyList<FollowUp>>([followUp]);
        public Task UpdateAsync(FollowUp followUp, CancellationToken cancellationToken) => Task.CompletedTask;
    }

    private sealed class ThrowingReadDbConnectionFactory : IReadDbConnectionFactory
    {
        public Task<System.Data.IDbConnection> CreateOpenConnectionAsync(CancellationToken cancellationToken)
            => throw new InvalidOperationException("The feature gate should block before any DB call is made.");
    }

    private sealed class SingleBookingRepository(Booking booking) : IBookingRepository
    {
        public Task AddAsync(Booking booking, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task<Booking?> GetByIdAsync(Guid id, CancellationToken cancellationToken) => Task.FromResult(id == booking.Id ? booking : null);
        public Task<Booking?> GetByAcceptedRevisionIdAsync(Guid acceptedRevisionId, CancellationToken cancellationToken) => Task.FromResult<Booking?>(null);
        public Task<IReadOnlyList<Booking>> ListByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken) => Task.FromResult<IReadOnlyList<Booking>>([booking]);
        public Task UpdateAsync(Booking booking, CancellationToken cancellationToken) => Task.CompletedTask;
    }

    private sealed class StubBillingFinanceClient(IReadOnlyList<BookingInvoiceDto> invoices) : IBillingFinanceClient
    {
        public Task<IReadOnlyList<BookingInvoiceDto>> GetInvoicesAsync(Guid tenantId, CancellationToken cancellationToken)
            => Task.FromResult(invoices.Where(x => x.TenantId == tenantId).ToList() as IReadOnlyList<BookingInvoiceDto>);
    }


    private sealed class FakeTenantContext : TravelService.Api.ITenantContext
    {
        public Guid TenantId { get; } = Guid.NewGuid();
        public Guid? UserId { get; } = Guid.NewGuid();
    }
}
