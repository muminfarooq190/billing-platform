using TravelService.Domain.Enums;

namespace TravelService.Domain.ValueObjects;

public sealed record ItineraryItem(
    int DayNumber,
    ItineraryItemType ItemType,
    string Title,
    string Description,
    string Location,
    DateTimeOffset? StartTime,
    DateTimeOffset? EndTime,
    decimal Cost,
    string Currency);
