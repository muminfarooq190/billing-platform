using IdentityService.Domain.Aggregates;
using FluentAssertions;

namespace IdentityService.Tests;

public sealed class TenantTemplateThemeSmokeTests
{
    [Fact]
    public void Create_ShouldInitializeTemplateTheme()
    {
        var tenantId = Guid.NewGuid();

        var theme = TenantTemplateTheme.Create(tenantId, "QuotationPublicView");

        theme.TenantId.Should().Be(tenantId);
        theme.TemplateScope.Should().Be("QuotationPublicView");
        theme.Id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void Update_ShouldPersistOverrideFields()
    {
        var theme = TenantTemplateTheme.Create(Guid.NewGuid(), "Email");
        var logoAssetId = Guid.NewGuid();
        var backgroundAssetId = Guid.NewGuid();

        theme.Update("<h1>Hello</h1>", "<footer>Bye</footer>", ".hero{color:red;}", logoAssetId, backgroundAssetId, "{\"mode\":\"rich\"}");

        theme.HeaderHtml.Should().Be("<h1>Hello</h1>");
        theme.FooterHtml.Should().Be("<footer>Bye</footer>");
        theme.CustomCss.Should().Contain("color:red");
        theme.LogoAssetId.Should().Be(logoAssetId);
        theme.BackgroundAssetId.Should().Be(backgroundAssetId);
        theme.SettingsJson.Should().Contain("rich");
    }
}
