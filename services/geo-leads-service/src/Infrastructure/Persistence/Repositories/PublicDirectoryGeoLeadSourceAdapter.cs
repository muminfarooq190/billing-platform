using System.Text.Json;
using GeoLeadsService.Application.Abstractions;
using Microsoft.Extensions.Configuration;

namespace GeoLeadsService.Infrastructure.Persistence.Repositories;

public sealed class PublicDirectoryGeoLeadSourceAdapter(IConfiguration configuration) : IConfigurableGeoLeadSourceAdapter
{
    public string SourceName => "public-directory-snapshot";
    public bool IsEnabled => configuration.GetValue<bool?>("GeoLeadSources:PublicDirectory:Enabled") ?? true;

    public Task<IReadOnlyList<GeoLeadSourceRecordInput>> FetchAsync(CancellationToken cancellationToken)
    {
        var configured = configuration.GetSection("GeoLeadSources:PublicDirectory:Records").Get<List<PublicDirectoryRecord>>() ?? [];
        var records = configured.Select(record => new GeoLeadSourceRecordInput(
            record.SourceRecordId,
            record.RawName,
            record.RawCategory,
            record.RawAddress,
            record.RawPhone,
            record.RawEmail,
            record.RawWebsite,
            record.RawLatitude,
            record.RawLongitude,
            JsonSerializer.Serialize(new
            {
                record.City,
                record.Region,
                record.Country,
                tags = record.Tags ?? [],
                source = SourceName
            }))).ToList();

        return Task.FromResult<IReadOnlyList<GeoLeadSourceRecordInput>>(records);
    }

    public sealed class PublicDirectoryRecord
    {
        public string SourceRecordId { get; set; } = string.Empty;
        public string RawName { get; set; } = string.Empty;
        public string RawCategory { get; set; } = string.Empty;
        public string? RawAddress { get; set; }
        public string? RawPhone { get; set; }
        public string? RawEmail { get; set; }
        public string? RawWebsite { get; set; }
        public decimal? RawLatitude { get; set; }
        public decimal? RawLongitude { get; set; }
        public string? City { get; set; }
        public string? Region { get; set; }
        public string? Country { get; set; }
        public List<string>? Tags { get; set; }
    }
}
