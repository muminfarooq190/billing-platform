namespace GeoLeadsService.Domain.Aggregates;

public sealed record LeadSourceRecord(
    Guid Id,
    string SourceName,
    string SourceRecordId,
    string RawName,
    string RawCategory,
    string? RawAddress,
    string? RawPhone,
    string? RawEmail,
    string? RawWebsite,
    decimal? RawLatitude,
    decimal? RawLongitude,
    string RawPayloadJson,
    DateTimeOffset FirstSeenAt,
    DateTimeOffset LastSeenAt);
