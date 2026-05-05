namespace TravelService.Api.Contracts;

public sealed record AssignInquiryRequest(Guid? AssignedToUserId);
public sealed record InquiryStatusReasonRequest(string? Reason);

public sealed record CreateTravelInquiryRequest(
    string Source,
    string FullName,
    string? Email,
    string? Phone,
    string? WhatsappNumber,
    string? DepartureCity,
    string Destination,
    DateTimeOffset? TravelDate,
    DateTimeOffset? ReturnDate,
    bool IsDateFlexible,
    int? Travellers,
    decimal? BudgetAmount,
    string? BudgetCurrency,
    string? CustomerMessage,
    Guid? AssignedToUserId);

public sealed record UpdateTravelInquiryRequest(
    string FullName,
    string? Email,
    string? Phone,
    string? WhatsappNumber,
    string? DepartureCity,
    string Destination,
    DateTimeOffset? TravelDate,
    DateTimeOffset? ReturnDate,
    bool IsDateFlexible,
    int? Travellers,
    decimal? BudgetAmount,
    string? BudgetCurrency,
    string? CustomerMessage,
    Guid? AssignedToUserId);
