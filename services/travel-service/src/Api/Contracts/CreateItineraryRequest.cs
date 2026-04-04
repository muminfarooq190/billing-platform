namespace TravelService.Api.Contracts;

public sealed record CreateItineraryRequest(
    Guid TenantId,
    Guid CustomerContactId,
    string CustomerName,
    string Title,
    string Destination,
    DateTimeOffset StartDate,
    DateTimeOffset EndDate,
    int Travellers,
    string Currency,
    Guid? QuotationId,
    List<ItineraryItemRequest> Items);

public sealed record ItineraryItemRequest(
    int DayNumber,
    string ItemType,
    string Title,
    string Description,
    string Location,
    DateTimeOffset? StartTime,
    DateTimeOffset? EndTime,
    decimal Cost,
    string Currency);
