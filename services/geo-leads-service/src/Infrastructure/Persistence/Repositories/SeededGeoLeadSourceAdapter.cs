using System.Text.Json;
using GeoLeadsService.Application.Abstractions;

namespace GeoLeadsService.Infrastructure.Persistence.Repositories;

public sealed class SeededGeoLeadSourceAdapter : IGeoLeadSourceAdapter
{
    public string SourceName => "seeded-public-tourism";

    public Task<IReadOnlyList<GeoLeadSourceRecordInput>> FetchAsync(CancellationToken cancellationToken)
    {
        IReadOnlyList<GeoLeadSourceRecordInput> records =
        [
            new(
                "seeded-1",
                "Sunrise Adventures",
                "tour_operator",
                "Colaba, Mumbai",
                "+919999000001",
                "hello@sunrise.example",
                "https://sunrise.example",
                18.9218m,
                72.8347m,
                JsonSerializer.Serialize(new { tags = new[] { "tourism", "operator" }, source = "seeded" })),
            new(
                "seeded-2",
                "Harbor Stay Boutique",
                "hotel",
                "Fort, Mumbai",
                "+919999000002",
                "stay@harbor.example",
                "https://harbor.example",
                18.9350m,
                72.8355m,
                JsonSerializer.Serialize(new { tags = new[] { "hotel", "hospitality" }, source = "seeded" })),
            new(
                "seeded-3",
                "Coastal Explorer",
                "activity_provider",
                "Gateway area, Mumbai",
                null,
                "contact@coastal.example",
                "https://coastal.example",
                18.9225m,
                72.8333m,
                JsonSerializer.Serialize(new { tags = new[] { "activity", "destination" }, source = "seeded" }))
        ];

        return Task.FromResult(records);
    }
}
