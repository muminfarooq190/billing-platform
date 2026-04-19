using FluentAssertions;
using GeoLeadsService.Domain.Aggregates;
using Xunit;

namespace GeoLeadsService.Tests;

public sealed class LeadSourceRecordTests
{
    [Fact]
    public void LeadSourceRecord_Refresh_ShouldUpdateLastSeenAndFields()
    {
        var record = new LeadSourceRecord("source", "record-1", "Old Name", "hotel", "Old Address", null, null, null, 18.90m, 72.80m, "{\"old\":true}");
        var firstSeenAt = record.FirstSeenAt;
        var originalLastSeenAt = record.LastSeenAt;

        Thread.Sleep(5);
        record.Refresh("New Name", "tour_operator", "New Address", "+91123", "hello@example.com", "https://example.com", 18.91m, 72.81m, "{\"new\":true}");

        record.RawName.Should().Be("New Name");
        record.RawCategory.Should().Be("tour_operator");
        record.RawAddress.Should().Be("New Address");
        record.FirstSeenAt.Should().Be(firstSeenAt);
        record.LastSeenAt.Should().BeAfter(originalLastSeenAt);
    }
}
