using TravelService.Domain.Enums;
using TravelService.Domain.Exceptions;

namespace TravelService.Domain.ValueObjects;

public sealed record ItineraryItem
{
    public ItineraryItem(int dayNumber, ItineraryItemType itemType, string title, string description, string location, DateTimeOffset? startTime, DateTimeOffset? endTime, decimal cost, string currency)
    {
        if (dayNumber <= 0)
            throw new DomainException("Itinerary item day number must be greater than zero.");
        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException("Itinerary item title is required.");
        if (cost < 0)
            throw new DomainException("Itinerary item cost cannot be negative.");
        if (startTime.HasValue && endTime.HasValue && endTime.Value < startTime.Value)
            throw new DomainException("Itinerary item end time must be on or after the start time.");
        if (string.IsNullOrWhiteSpace(currency))
            throw new DomainException("Itinerary item currency is required.");

        DayNumber = dayNumber;
        ItemType = itemType;
        Title = title.Trim();
        Description = description.Trim();
        Location = location.Trim();
        StartTime = startTime;
        EndTime = endTime;
        Cost = cost;
        Currency = currency.Trim().ToUpperInvariant();
    }

    private ItineraryItem() { }

    public int DayNumber { get; init; }
    public ItineraryItemType ItemType { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Location { get; init; } = string.Empty;
    public DateTimeOffset? StartTime { get; init; }
    public DateTimeOffset? EndTime { get; init; }
    public decimal Cost { get; init; }
    public string Currency { get; init; } = string.Empty;
}
