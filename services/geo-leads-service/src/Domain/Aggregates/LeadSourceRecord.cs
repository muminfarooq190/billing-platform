namespace GeoLeadsService.Domain.Aggregates;

public sealed class LeadSourceRecord
{
    private LeadSourceRecord() { }

    public LeadSourceRecord(
        string sourceName,
        string sourceRecordId,
        string rawName,
        string rawCategory,
        string? rawAddress,
        string? rawPhone,
        string? rawEmail,
        string? rawWebsite,
        decimal? rawLatitude,
        decimal? rawLongitude,
        string rawPayloadJson)
    {
        Id = Guid.NewGuid();
        SourceName = sourceName;
        SourceRecordId = sourceRecordId;
        RawName = rawName;
        RawCategory = rawCategory;
        RawAddress = rawAddress;
        RawPhone = rawPhone;
        RawEmail = rawEmail;
        RawWebsite = rawWebsite;
        RawLatitude = rawLatitude;
        RawLongitude = rawLongitude;
        RawPayloadJson = rawPayloadJson;
        FirstSeenAt = DateTimeOffset.UtcNow;
        LastSeenAt = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }
    public string SourceName { get; private set; } = string.Empty;
    public string SourceRecordId { get; private set; } = string.Empty;
    public string RawName { get; private set; } = string.Empty;
    public string RawCategory { get; private set; } = string.Empty;
    public string? RawAddress { get; private set; }
    public string? RawPhone { get; private set; }
    public string? RawEmail { get; private set; }
    public string? RawWebsite { get; private set; }
    public decimal? RawLatitude { get; private set; }
    public decimal? RawLongitude { get; private set; }
    public string RawPayloadJson { get; private set; } = "{}";
    public DateTimeOffset FirstSeenAt { get; private set; }
    public DateTimeOffset LastSeenAt { get; private set; }
}
