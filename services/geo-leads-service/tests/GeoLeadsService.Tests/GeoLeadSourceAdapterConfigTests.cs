using FluentAssertions;
using GeoLeadsService.Application.Abstractions;
using GeoLeadsService.Infrastructure.Persistence.Repositories;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace GeoLeadsService.Tests;

public sealed class GeoLeadSourceAdapterConfigTests
{
    [Fact]
    public void SeededAdapter_ShouldRespectEnabledFlag()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["GeoLeadSources:Seeded:Enabled"] = "false"
            })
            .Build();

        IConfigurableGeoLeadSourceAdapter adapter = new SeededGeoLeadSourceAdapter(config);

        adapter.IsEnabled.Should().BeFalse();
    }

    [Fact]
    public void PublicDirectoryAdapter_ShouldRespectEnabledFlag()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["GeoLeadSources:PublicDirectory:Enabled"] = "false"
            })
            .Build();

        IConfigurableGeoLeadSourceAdapter adapter = new PublicDirectoryGeoLeadSourceAdapter(config);

        adapter.IsEnabled.Should().BeFalse();
    }
}
