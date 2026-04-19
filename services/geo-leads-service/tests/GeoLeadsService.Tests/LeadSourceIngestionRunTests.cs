using FluentAssertions;
using GeoLeadsService.Domain.Aggregates;
using Xunit;

namespace GeoLeadsService.Tests;

public sealed class LeadSourceIngestionRunTests
{
    [Fact]
    public void LeadSourceIngestionRun_ShouldCompleteSuccessfully()
    {
        var run = new LeadSourceIngestionRun("public-directory-snapshot");

        run.Complete(12);

        run.Status.Should().Be("Completed");
        run.FetchedCount.Should().Be(12);
        run.CompletedAt.Should().NotBeNull();
        run.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public void LeadSourceIngestionRun_ShouldCaptureFailure()
    {
        var run = new LeadSourceIngestionRun("public-directory-snapshot");

        run.Fail("boom");

        run.Status.Should().Be("Failed");
        run.ErrorMessage.Should().Be("boom");
        run.CompletedAt.Should().NotBeNull();
    }
}
