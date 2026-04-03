using FluentAssertions;
using IdentityService.Domain.Aggregates;
using IdentityService.Domain.Enums;
using IdentityService.Domain.ValueObjects;

namespace IdentityService.Tests.Domain;

public sealed class TenantAggregateTests
{
    [Fact]
    public void Register_ShouldRaiseTenantCreatedEvent()
    {
        var tenant = Tenant.Register("Acme", new Email("owner@acme.com"));

        tenant.DomainEvents.Should().HaveCount(1);
        tenant.Status.Should().Be(TenantStatus.Active);
    }
}
