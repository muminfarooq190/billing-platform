namespace GeoLeadsService.Application.Abstractions;

public interface IGeoLeadSourceAdapter
{
    string SourceName { get; }
    Task<IReadOnlyList<GeoLeadSourceRecordInput>> FetchAsync(CancellationToken cancellationToken);
}

public sealed record GeoLeadSourceRecordInput(
    string SourceRecordId,
    string RawName,
    string RawCategory,
    string? RawAddress,
    string? RawPhone,
    string? RawEmail,
    string? RawWebsite,
    decimal? RawLatitude,
    decimal? RawLongitude,
    string RawPayloadJson);
