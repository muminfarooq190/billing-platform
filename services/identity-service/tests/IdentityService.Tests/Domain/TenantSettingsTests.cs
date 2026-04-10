using FluentAssertions;
using IdentityService.Domain.Aggregates;

namespace IdentityService.Tests.Domain;

public sealed class TenantSettingsTests
{
    [Fact]
    public void Update_ShouldPersistWorkspaceDefaults()
    {
        var tenantId = Guid.NewGuid();
        var settings = TenantSettings.Create(tenantId, "UTC", "en", "yyyy-MM-dd", "USD", "en-US", "US", "{}");

        settings.Update("Asia/Calcutta", "en-IN", "dd/MM/yyyy", "INR", "en-IN", "IN", "{\"quotePrefix\":\"VOY\"}");

        settings.Timezone.Should().Be("Asia/Calcutta");
        settings.Currency.Should().Be("INR");
        settings.SettingsJson.Should().Contain("quotePrefix");
    }
}
