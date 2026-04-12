using CommunicationService.Application.Abstractions;
using CommunicationService.Application.Queries.GetUnreadNotificationCount;
using CommunicationService.Application.Queries.ListNotificationsByRecipient;
using CommunicationService.Application.Queries.ListTemplatesByTenant;
using CommunicationService.Domain.Exceptions;
using FluentAssertions;

namespace CommunicationService.Tests;

public sealed class EntitlementEnforcementTests
{
    [Fact]
    public async Task ListTemplatesByTenant_ShouldFail_WhenTemplatesFeatureDisabled()
    {
        var handler = new ListTemplatesByTenantQueryHandler(new ThrowingReadDbConnectionFactory(), new DenyFeatureGate());

        var act = async () => await handler.Handle(new ListTemplatesByTenantQuery(Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>().WithMessage("*not enabled*");
    }

    [Fact]
    public async Task ListNotificationsByRecipient_ShouldFail_WhenLogsFeatureDisabled()
    {
        var handler = new ListNotificationsByRecipientQueryHandler(new ThrowingReadDbConnectionFactory(), new DenyFeatureGate());

        var act = async () => await handler.Handle(new ListNotificationsByRecipientQuery(Guid.NewGuid(), Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>().WithMessage("*not enabled*");
    }

    [Fact]
    public async Task GetUnreadNotificationCount_ShouldFail_WhenLogsFeatureDisabled()
    {
        var handler = new GetUnreadNotificationCountQueryHandler(new ThrowingReadDbConnectionFactory(), new DenyFeatureGate());

        var act = async () => await handler.Handle(new GetUnreadNotificationCountQuery(Guid.NewGuid(), Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>().WithMessage("*not enabled*");
    }

    private sealed class DenyFeatureGate : IFeatureGate
    {
        public Task EnsureEnabledAsync(string featureKey, Guid tenantId, Guid? userId, CancellationToken cancellationToken)
            => throw new DomainException($"Feature '{featureKey}' is not enabled for tenant '{tenantId}' and user '{userId}'.");

        public Task<bool> IsEnabledAsync(string featureKey, Guid tenantId, Guid? userId, CancellationToken cancellationToken) => Task.FromResult(false);

        public Task<int?> GetLimitAsync(string featureKey, Guid tenantId, Guid? userId, CancellationToken cancellationToken) => Task.FromResult<int?>(null);
    }

    private sealed class ThrowingReadDbConnectionFactory : IReadDbConnectionFactory
    {
        public Task<System.Data.IDbConnection> CreateOpenConnectionAsync(CancellationToken cancellationToken)
            => throw new InvalidOperationException("The feature gate should block before any DB call is made.");
    }
}
