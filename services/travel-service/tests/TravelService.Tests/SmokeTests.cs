using FluentAssertions;

namespace TravelService.Tests;

public sealed class SmokeTests
{
    [Fact]
    public void True_ShouldBeTrue()
    {
        true.Should().BeTrue();
    }
}
