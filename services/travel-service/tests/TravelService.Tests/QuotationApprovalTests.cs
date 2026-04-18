using FluentAssertions;



using TravelService.Application.Abstractions;



using TravelService.Application.Commands.QuotationApproval;



using TravelService.Application.Commands.SendQuotation;



using TravelService.Domain.Aggregates;



using TravelService.Domain.Enums;



using TravelService.Domain.Repositories;
using TravelService.Tests.TestDoubles;







namespace TravelService.Tests;







public sealed class QuotationApprovalTests



{



    [Fact]



    public async Task CreateApprovalRequest_ShouldPersistPendingRequest()



    {



        var quotation = Quotation.Create(Guid.NewGuid(), Guid.NewGuid(), "Jane Doe", "Europe", "Italy", DateTimeOffset.UtcNow.AddDays(30), DateTimeOffset.UtcNow.AddDays(35), 2, "USD", "notes");



        quotation.AddLineItem("Hotel", 1000m, 1, "USD");



        var revision = quotation.CreateRevision("Visible", "Internal");



        var repository = new InMemoryApprovalRepository();



        var handler = new CreateQuotationApprovalRequestCommandHandler(new SingleQuotationRepository(quotation), new SingleRevisionRepository(revision), repository, new NoOpActivityWriter(), new FakeActorContext(quotation.TenantId), new NoOpUnitOfWork());







        var id = await handler.Handle(new CreateQuotationApprovalRequestCommand(quotation.TenantId, quotation.Id, revision.Id, "Low margin", 9.5m, 12m), CancellationToken.None);







        repository.Items.Should().ContainSingle(x => x.Id == id && x.Status == QuotationApprovalStatus.Pending);



    }







    [Fact]



    public async Task SendQuotation_ShouldFail_WhenApprovalPending()



    {



        var quotation = Quotation.Create(Guid.NewGuid(), Guid.NewGuid(), "Jane Doe", "Europe", "Italy", DateTimeOffset.UtcNow.AddDays(30), DateTimeOffset.UtcNow.AddDays(35), 2, "USD", "notes");



        quotation.AddLineItem("Hotel", 1000m, 1, "USD");



        var revision = quotation.CreateRevision("Visible", "Internal");



        var approvalRequest = QuotationApprovalRequest.Create(quotation.Id, quotation.TenantId, revision.Id, "Needs approval", revision.TotalAmount, 8m, 15m);







        var handler = new SendQuotationCommandHandler(



            new SingleQuotationRepository(quotation),



            new SingleRevisionRepository(revision),



            new InMemoryShareLinkRepository(),



            new InMemoryStatusHistoryRepository(),



            new InMemoryApprovalRepository(approvalRequest),

            new NoOpCommunicationWorkflowClient(),

            new AllowAllFeatureGate(),



            new NoOpActivityWriter(),



            new FakeActorContext(quotation.TenantId),



            new NoOpUnitOfWork(), new FakeTenantContext());







        var act = async () => await handler.Handle(new SendQuotationCommand(quotation.TenantId, quotation.Id, revision.Id, "Email", "traveler@example.com", null, DateTimeOffset.UtcNow.AddDays(3)), CancellationToken.None);







        await act.Should().ThrowAsync<TravelService.Domain.Exceptions.DomainException>().WithMessage("*approval is pending*");



    }







    private sealed class InMemoryApprovalRepository(params QuotationApprovalRequest[] items) : IQuotationApprovalRequestRepository



    {



        public List<QuotationApprovalRequest> Items { get; } = items.ToList();



        public Task AddAsync(QuotationApprovalRequest request, CancellationToken cancellationToken) { Items.Add(request); return Task.CompletedTask; }



        public Task<QuotationApprovalRequest?> GetByIdAsync(Guid quotationId, Guid approvalRequestId, CancellationToken cancellationToken) => Task.FromResult(Items.SingleOrDefault(x => x.QuotationId == quotationId && x.Id == approvalRequestId));



        public Task<IReadOnlyList<QuotationApprovalRequest>> ListByQuotationIdAsync(Guid quotationId, CancellationToken cancellationToken) => Task.FromResult<IReadOnlyList<QuotationApprovalRequest>>(Items.Where(x => x.QuotationId == quotationId).ToList());



        public Task UpdateAsync(QuotationApprovalRequest request, CancellationToken cancellationToken) => Task.CompletedTask;



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







    private sealed class AllowAllFeatureGate : IFeatureGate



    {



        public Task EnsureEnabledAsync(string featureKey, Guid tenantId, CancellationToken cancellationToken) => Task.CompletedTask;


        public Task EnsureEnabledAsync(string featureKey, Guid tenantId, Guid? userId, CancellationToken cancellationToken) => Task.CompletedTask;



        public Task<bool> IsEnabledAsync(string featureKey, Guid tenantId, CancellationToken cancellationToken) => Task.FromResult(true);


        public Task<bool> IsEnabledAsync(string featureKey, Guid tenantId, Guid? userId, CancellationToken cancellationToken) => Task.FromResult(true);



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



