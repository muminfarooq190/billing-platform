using FluentAssertions;
using TravelService.Application.Abstractions;
using TravelService.Application.Commands.MarkPublicQuotationViewed;
using TravelService.Application.Commands.SendQuotation;
using TravelService.Domain.Aggregates;
using TravelService.Domain.Enums;
using TravelService.Domain.Repositories;

namespace TravelService.Tests;

public sealed class QuotationSendShareTests
{
    [Fact]
    public async Task SendQuotation_ShouldCreateShareLink_AndMarkQuotationSent()
    {
        var quotation = CreateQuotation();
        quotation.AddLineItem("Hotel", 500m, 1, "USD");
        var revision = quotation.CreateRevision("Visible notes", "Internal notes");

        var quotationRepository = new InMemoryQuotationRepository(quotation);
        var revisionRepository = new InMemoryQuotationRevisionRepository(revision);
        var shareLinkRepository = new InMemoryQuotationShareLinkRepository();
        var historyRepository = new InMemoryQuotationStatusHistoryRepository();
        var handler = new SendQuotationCommandHandler(quotationRepository, revisionRepository, shareLinkRepository, historyRepository, new InMemoryQuotationApprovalRequestRepository(), new AllowAllFeatureGate(), new NoOpActivityWriter(), new FakeActorContext(quotation.TenantId), new NoOpUnitOfWork(), new FakeTenantContext());

        var result = await handler.Handle(new SendQuotationCommand(
            quotation.TenantId,
            quotation.Id,
            revision.Id,
            "Email",
            "traveler@example.com",
            "Please review your quotation.",
            DateTimeOffset.UtcNow.AddDays(7)), CancellationToken.None);

        result.Token.Should().NotBeNullOrWhiteSpace();
        shareLinkRepository.ShareLinks.Should().ContainSingle();
        quotation.Status.Should().Be(QuotationStatus.Sent);
        quotation.ShareToken.Should().Be(result.Token);
        quotation.LastSentAt.Should().NotBeNull();
        historyRepository.Items.Should().ContainSingle(x => x.ToStatus == "Sent");
    }

    [Fact]
    public async Task MarkPublicQuotationViewed_ShouldUpdateShareLink_AndQuotation()
    {
        var quotation = CreateQuotation();
        quotation.AddLineItem("Hotel", 500m, 1, "USD");
        var revision = quotation.CreateRevision("Visible notes", "Internal notes");
        var shareLink = QuotationShareLink.Create(quotation.Id, revision.Id, quotation.TenantId, "token-123", DateTimeOffset.UtcNow.AddDays(3));
        quotation.SetShareToken("token-123", shareLink.ExpiresAt);
        quotation.Send();

        var shareLinkRepository = new InMemoryQuotationShareLinkRepository(shareLink);
        var historyRepository = new InMemoryQuotationStatusHistoryRepository();
        var handler = new MarkPublicQuotationViewedCommandHandler(shareLinkRepository, new InMemoryQuotationRepository(quotation), historyRepository, new NoOpActivityWriter(), new NoOpUnitOfWork());

        var updated = await handler.Handle(new MarkPublicQuotationViewedCommand("token-123"), CancellationToken.None);

        updated.Should().BeTrue();
        shareLink.LastViewedAt.Should().NotBeNull();
        quotation.LastViewedAt.Should().NotBeNull();
        historyRepository.Items.Should().ContainSingle(x => x.ToStatus == "Viewed");
    }

    private static Quotation CreateQuotation()
        => Quotation.Create(Guid.NewGuid(), Guid.NewGuid(), "Ava", "Summer Trip", "Istanbul", DateTimeOffset.UtcNow.AddDays(20), DateTimeOffset.UtcNow.AddDays(25), 2, "USD", "notes");

    private sealed class InMemoryQuotationRepository(Quotation quotation) : IQuotationRepository
    {
        public Task AddAsync(Quotation quotation, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task<Quotation?> GetByIdAsync(Guid id, CancellationToken cancellationToken) => Task.FromResult(id == quotation.Id ? quotation : null);
        public Task<IReadOnlyList<Quotation>> ListByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken) => Task.FromResult<IReadOnlyList<Quotation>>([quotation]);
        public Task UpdateAsync(Quotation quotation, CancellationToken cancellationToken) => Task.CompletedTask;
    }

    private sealed class InMemoryQuotationRevisionRepository(params QuotationRevision[] revisions) : IQuotationRevisionRepository
    {
        private readonly List<QuotationRevision> _revisions = revisions.ToList();

        public Task AddAsync(QuotationRevision revision, CancellationToken cancellationToken)
        {
            _revisions.Add(revision);
            return Task.CompletedTask;
        }

        public Task<QuotationRevision?> GetByIdAsync(Guid quotationId, Guid revisionId, CancellationToken cancellationToken)
            => Task.FromResult(_revisions.SingleOrDefault(x => x.QuotationId == quotationId && x.Id == revisionId));

        public Task<IReadOnlyList<QuotationRevision>> ListByQuotationIdAsync(Guid quotationId, CancellationToken cancellationToken)
            => Task.FromResult<IReadOnlyList<QuotationRevision>>(_revisions.Where(x => x.QuotationId == quotationId).ToList());
    }

    private sealed class InMemoryQuotationShareLinkRepository(params QuotationShareLink[] shareLinks) : IQuotationShareLinkRepository
    {
        public List<QuotationShareLink> ShareLinks { get; } = shareLinks.ToList();

        public Task AddAsync(QuotationShareLink shareLink, CancellationToken cancellationToken)
        {
            ShareLinks.Add(shareLink);
            return Task.CompletedTask;
        }

        public Task<QuotationShareLink?> GetActiveByTokenAsync(string token, CancellationToken cancellationToken)
            => Task.FromResult(ShareLinks.SingleOrDefault(x => x.Token == token && x.RevokedAt is null));

        public Task<IReadOnlyList<QuotationShareLink>> ListByQuotationIdAsync(Guid quotationId, CancellationToken cancellationToken)
            => Task.FromResult<IReadOnlyList<QuotationShareLink>>(ShareLinks.Where(x => x.QuotationId == quotationId).ToList());

        public Task UpdateAsync(QuotationShareLink shareLink, CancellationToken cancellationToken) => Task.CompletedTask;
    }

    private sealed class InMemoryQuotationStatusHistoryRepository : IQuotationStatusHistoryRepository
    {
        public List<QuotationStatusHistory> Items { get; } = [];

        public Task AddAsync(QuotationStatusHistory history, CancellationToken cancellationToken)
        {
            Items.Add(history);
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<QuotationStatusHistory>> ListByQuotationIdAsync(Guid quotationId, CancellationToken cancellationToken)
            => Task.FromResult<IReadOnlyList<QuotationStatusHistory>>(Items.Where(x => x.QuotationId == quotationId).ToList());
    }

    private sealed class InMemoryQuotationApprovalRequestRepository : IQuotationApprovalRequestRepository
    {
        public Task AddAsync(QuotationApprovalRequest request, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task<QuotationApprovalRequest?> GetByIdAsync(Guid quotationId, Guid approvalRequestId, CancellationToken cancellationToken) => Task.FromResult<QuotationApprovalRequest?>(null);
        public Task<IReadOnlyList<QuotationApprovalRequest>> ListByQuotationIdAsync(Guid quotationId, CancellationToken cancellationToken) => Task.FromResult<IReadOnlyList<QuotationApprovalRequest>>([]);
        public Task UpdateAsync(QuotationApprovalRequest request, CancellationToken cancellationToken) => Task.CompletedTask;
    }

    private sealed class AllowAllFeatureGate : IFeatureGate
    {
        public Task EnsureEnabledAsync(string featureKey, Guid tenantId, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task EnsureEnabledAsync(string featureKey, Guid tenantId, Guid? userId, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task<bool> IsEnabledAsync(string featureKey, Guid tenantId, CancellationToken cancellationToken) => Task.FromResult(true);
        public Task<bool> IsEnabledAsync(string featureKey, Guid tenantId, Guid? userId, CancellationToken cancellationToken) => Task.FromResult(true);
        public Task<int?> GetLimitAsync(string featureKey, Guid tenantId, CancellationToken cancellationToken) => Task.FromResult<int?>(null);
        public Task<int?> GetLimitAsync(string featureKey, Guid tenantId, Guid? userId, CancellationToken cancellationToken) => Task.FromResult<int?>(null);
    }

    private sealed class NoOpActivityWriter : IActivityWriter
    {
        public Task WriteAsync(ActivityEntry entry, CancellationToken cancellationToken) => Task.CompletedTask;
    }

    private sealed class FakeActorContext(Guid tenantId) : IActorContext
    {
        public Guid? UserId { get; } = Guid.NewGuid();
        public Guid TenantId { get; } = tenantId;
        public string? IpAddress { get; } = "127.0.0.1";
        public string? UserAgent { get; } = "tests";
    }

    private sealed class NoOpUnitOfWork : IUnitOfWork
    {
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken) => Task.FromResult(1);
    }


    private sealed class FakeTenantContext : TravelService.Api.ITenantContext
    {
        public Guid TenantId { get; } = Guid.NewGuid();
        public Guid? UserId { get; } = Guid.NewGuid();
    }
}
