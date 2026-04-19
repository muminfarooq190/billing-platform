using FluentAssertions;
using GeoLeadsService.Infrastructure.Persistence.Repositories;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace GeoLeadsService.Tests;

public sealed class PublicDirectoryGeoLeadSourceAdapterTests
{
    [Fact]
    public async Task PublicDirectoryGeoLeadSourceAdapter_ShouldReadConfiguredRecords()
    {
        var configData = new Dictionary<string, string?>
        {
            ["GeoLeadSources:PublicDirectory:Records:0:SourceRecordId"] = "public-1",
            ["GeoLeadSources:PublicDirectory:Records:0:RawName"] = "Configured Hotel",
            ["GeoLeadSources:PublicDirectory:Records:0:RawCategory"] = "hotel",
            ["GeoLeadSources:PublicDirectory:Records:0:RawAddress"] = "Somewhere",
            ["GeoLeadSources:PublicDirectory:Records:0:RawEmail"] = "hello@example.com",
            ["GeoLeadSources:PublicDirectory:Records:0:RawLatitude"] = "18.93",
            ["GeoLeadSources:PublicDirectory:Records:0:RawLongitude"] = "72.83",
            ["GeoLeadSources:PublicDirectory:Records:0:City"] = "Mumbai",
            ["GeoLeadSources:PublicDirectory:Records:0:Region"] = "Maharashtra",
            ["GeoLeadSources:PublicDirectory:Records:0:Country"] = "India",
            ["GeoLeadSources:PublicDirectory:Records:0:Tags:0"] = "public-directory"
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();

        var adapter = new PublicDirectoryGeoLeadSourceAdapter(configuration);

        var records = await adapter.FetchAsync(CancellationToken.None);

        records.Should().ContainSingle();
        records[0].RawName.Should().Be("Configured Hotel");
        records[0].RawCategory.Should().Be("hotel");
    }
}
