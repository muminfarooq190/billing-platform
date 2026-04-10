using FluentAssertions;
using IdentityService.Domain.Aggregates;

namespace IdentityService.Tests.Domain;

public sealed class RoleDefinitionTests
{
    [Fact]
    public void SetPermissions_ShouldReplaceDistinctPermissionSet()
    {
        var role = RoleDefinition.Create(Guid.NewGuid(), "Finance Admin", "Finance-only admin role");

        role.SetPermissions(["billing.invoices.read", "billing.invoices.read", "identity.audit.read"]);

        role.Permissions.Select(x => x.PermissionKey).Should().BeEquivalentTo(["billing.invoices.read", "identity.audit.read"]);
    }
}
