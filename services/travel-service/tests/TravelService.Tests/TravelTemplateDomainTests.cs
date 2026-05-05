using FluentAssertions;
using TravelService.Domain.Aggregates;
using TravelService.Domain.Enums;
using TravelService.Domain.Exceptions;

namespace TravelService.Tests;

public sealed class TravelTemplateDomainTests
{
    [Fact]
    public void Create_ShouldPersistShape_AndStartActive()
    {
        var template = TravelTemplate.Create(
            Guid.NewGuid(),
            TravelTemplateContext.Quote,
            "Adventure",
            "For thrill seekers",
            "adventure",
            "https://example.com/banner.jpg",
            "#f97316",
            "High-energy trips",
            "{\"sections\":[{\"id\":\"hero\",\"label\":\"Hero\",\"hint\":\"Intro\"}]}",
            "{\"conceptSeed\":[],\"quoteSeed\":[],\"itineraryDays\":[]}",
            false,
            Guid.NewGuid());

        template.Context.Should().Be(TravelTemplateContext.Quote);
        template.IsActive.Should().BeTrue();
        template.IsBuiltIn.Should().BeFalse();
        template.Name.Should().Be("Adventure");
    }

    [Fact]
    public void BuiltInTemplate_ShouldRejectUpdate_AndDelete()
    {
        var template = TravelTemplate.Create(
            Guid.NewGuid(),
            TravelTemplateContext.Concept,
            "Built-in",
            null,
            "luxury",
            "https://example.com/banner.jpg",
            "#111111",
            "Tagline",
            "{\"sections\":[{\"id\":\"hero\",\"label\":\"Hero\",\"hint\":\"Intro\"}]}",
            "{\"conceptSeed\":[],\"quoteSeed\":[],\"itineraryDays\":[]}",
            true,
            null);

        var update = () => template.Update("New", null, "luxury", "https://example.com/x.jpg", "#222222", "New tag", "{\"sections\":[{\"id\":\"hero\",\"label\":\"Hero\",\"hint\":\"Intro\"}]}", "{\"conceptSeed\":[],\"quoteSeed\":[],\"itineraryDays\":[]}");
        var archive = () => template.Archive();

        update.Should().Throw<DomainException>();
        archive.Should().Throw<DomainException>();
    }
}
