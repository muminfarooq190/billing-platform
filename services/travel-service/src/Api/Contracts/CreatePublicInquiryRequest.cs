namespace TravelService.Api.Contracts;

public sealed record CreatePublicInquiryRequest(
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
    string? Message,
    string? Source,
    string? Honeypot = null);
