using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using TravelService.Domain.Aggregates;
using TravelService.Domain.Enums;

namespace TravelService.Infrastructure.Persistence;

internal static class TravelTemplateSeedData
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    internal static readonly Guid SeedTenantId = Guid.Parse("00000000-0000-0000-0000-000000000001");

    public static IReadOnlyList<TravelTemplateSeedRecord> BuiltIns { get; } =
    [
        new(
            Guid.Parse("11111111-1111-1111-1111-111111111111"),
            TravelTemplateContext.Quote,
            "Adventure",
            "adventure",
            "https://images.unsplash.com/photo-1551632811-561732d1e306?w=1200&q=60&auto=format&fit=crop",
            "#f97316",
            "High-energy trips for outdoor & thrill seekers",
            SerializeSections([
                new("hero", "Hero overview", "Trip story, destination, traveller hook"),
                new("highlights", "Key highlights", "Bucket-list moments"),
                new("itinerary", "Day-by-day plan", "Trekking, expeditions, wildlife"),
                new("gear", "Gear & fitness", "Packing list, fitness notes"),
                new("inclusions", "What is included", "Guides, permits, transport"),
                new("pricing", "Investment", "Per-person pricing, deposits")
            ]),
            SerializeSeed(
                [
                    new("highlight", "Himalayan Base Camp Trek"),
                    new("activity", "White-water rafting - Grade IV"),
                    new("activity", "Jungle safari at dawn"),
                    new("accommodation", "Glamping dome camp"),
                    new("transport", "Private 4x4 expedition convoy")
                ],
                [
                    new("accommodation", "Mountain Eco-Lodge", "6 nights - shared twin", 900m),
                    new("activities", "Guided Trek & Rafting", "Certified guides, permits", 1250m),
                    new("transport", "4x4 Expedition Convoy", "Full trip, driver inc.", 650m),
                    new("meals", "Camp & Trail Meals", "All meals, energy packs", 280m)
                ],
                [
                    new("Arrival & acclimatisation", [
                        new("transport", "Airport to basecamp transfer", "14:00", null),
                        new("accommodation", "Mountain Eco-Lodge check-in", "16:30", null),
                        new("meal", "Welcome dinner + briefing", "19:30", null)
                    ]),
                    new("Rafting expedition", [
                        new("activity", "Grade IV white-water rafting", "09:00", null),
                        new("meal", "Riverside picnic lunch", "13:00", null)
                    ]),
                    new("Trek to ridge viewpoint", [
                        new("activity", "Guided alpine trek (6 hrs)", "07:30", null),
                        new("activity", "Sunset at ridge camp", "18:00", null)
                    ])
                ])),
        new(
            Guid.Parse("22222222-2222-2222-2222-222222222222"),
            TravelTemplateContext.Quote,
            "Honeymoon",
            "romance",
            "https://images.unsplash.com/photo-1520250497591-112f2f40a3f4?w=1200&q=60&auto=format&fit=crop",
            "#ec4899",
            "Private, romantic, and unforgettable",
            SerializeSections([
                new("hero", "Welcome", "Personalised intro for the couple"),
                new("moments", "Signature moments", "Sunsets, private dining, spa"),
                new("itinerary", "Day-by-day romance", "Slow pacing, curated experiences"),
                new("inclusions", "Honeymoon inclusions", "Upgrades, welcome amenities"),
                new("pricing", "Investment", "Couple pricing, add-ons"),
                new("terms", "Terms", "Cancellation, travel insurance")
            ]),
            SerializeSeed(
                [
                    new("accommodation", "Overwater villa with private pool"),
                    new("dining", "Private sunset dinner on beach"),
                    new("highlight", "Couples spa ritual"),
                    new("activity", "Dolphin-spotting dhoni cruise"),
                    new("transport", "Private seaplane transfer")
                ],
                [
                    new("accommodation", "Overwater Villa", "7 nights - half-board", 5600m),
                    new("flights", "Round-trip flights", "Business class - 2 travellers", 4800m),
                    new("transport", "Private Seaplane", "Arrival & departure transfers", 1200m),
                    new("activities", "Romance Experiences", "Spa, sunset dinner, cruise", 980m)
                ],
                [
                    new("Arrival in paradise", [
                        new("transport", "Seaplane to resort", "12:00", null),
                        new("accommodation", "Overwater villa check-in", "13:30", null),
                        new("meal", "Welcome champagne & canapes", "15:00", null)
                    ]),
                    new("Wellness & wonder", [
                        new("activity", "Couples spa ritual", "10:00", null),
                        new("activity", "Dolphin-spotting dhoni cruise", "15:30", null),
                        new("meal", "Private beach dinner", "19:30", null)
                    ]),
                    new("Island escape", [
                        new("activity", "Sandbank picnic & snorkelling", "09:30", null)
                    ])
                ])),
        new(
            Guid.Parse("33333333-3333-3333-3333-333333333333"),
            TravelTemplateContext.Quote,
            "Family",
            "family",
            "https://images.unsplash.com/photo-1502920917128-1aa500764cbd?w=1200&q=60&auto=format&fit=crop",
            "#2563eb",
            "Multi-generational trips that work for everyone",
            SerializeSections([
                new("hero", "Family overview", "Ages, pace, must-sees"),
                new("highlights", "Family highlights", "Kid-friendly moments"),
                new("itinerary", "Day plan", "Balanced, flexible pacing"),
                new("inclusions", "Inclusions", "Family rooms, kids clubs"),
                new("pricing", "Pricing", "Per-family breakdown")
            ]),
            SerializeSeed(
                [
                    new("accommodation", "Family-sized villa with kids club"),
                    new("activity", "Theme park day with fast-track passes"),
                    new("activity", "Hands-on cultural workshop"),
                    new("dining", "Kid-friendly local cooking class"),
                    new("transport", "Private minivan with child seats")
                ],
                [
                    new("accommodation", "Family Villa", "8 nights - 2 bedrooms", 3200m),
                    new("flights", "Flights (4 pax)", "Economy - direct", 2800m),
                    new("transport", "Private Minivan", "Full trip - child seats", 680m),
                    new("activities", "Family Experiences", "Theme park + workshops", 1450m)
                ],
                [
                    new("Arrive & settle", [
                        new("transport", "Airport transfer", "11:00", null),
                        new("accommodation", "Villa check-in", "14:00", null),
                        new("meal", "Family dinner", "19:00", null)
                    ]),
                    new("Theme park adventure", [
                        new("activity", "Full-day theme park + passes", "09:00", null)
                    ]),
                    new("Culture & cooking", [
                        new("activity", "Kids craft workshop", "10:00", null),
                        new("meal", "Local cooking class", "15:00", null)
                    ])
                ]))
    ];

    public static async Task SeedAsync(TravelDbContext dbContext, CancellationToken cancellationToken)
    {
        foreach (var record in BuiltIns)
        {
            var exists = await dbContext.TravelTemplates.AnyAsync(x => x.Id == record.Id, cancellationToken);
            if (exists)
                continue;

            var template = TravelTemplate.Create(
                SeedTenantId,
                record.Context,
                record.Name,
                null,
                record.Category,
                record.Banner,
                record.AccentColor,
                record.Tagline,
                record.SectionsJson,
                record.SeedJson,
                true,
                null);

            typeof(TravelTemplate).GetProperty(nameof(TravelTemplate.Id))!.SetValue(template, record.Id);
            dbContext.TravelTemplates.Add(template);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static string SerializeSections(List<TemplateSectionSeed> sections)
        => JsonSerializer.Serialize(new { sections }, JsonOptions);

    private static string SerializeSeed(
        List<TemplateConceptSeed> conceptSeed,
        List<TemplateQuoteSeed> quoteSeed,
        List<TemplateItineraryDaySeed> itineraryDays)
        => JsonSerializer.Serialize(new { conceptSeed, quoteSeed, itineraryDays }, JsonOptions);

    public sealed record TravelTemplateSeedRecord(
        Guid Id,
        TravelTemplateContext Context,
        string Name,
        string Category,
        string Banner,
        string AccentColor,
        string Tagline,
        string SectionsJson,
        string SeedJson);

    public sealed record TemplateSectionSeed(string Id, string Label, string Hint);
    public sealed record TemplateConceptSeed(string Type, string Content);
    public sealed record TemplateQuoteSeed(string Type, string Title, string Description, decimal Amount);
    public sealed record TemplateItineraryItemSeed(string Type, string Title, string? Time, string? Notes);
    public sealed record TemplateItineraryDaySeed(string Title, List<TemplateItineraryItemSeed> Items);
}
