using MediatR;

namespace TravelService.Application.Commands.TravelInquiries;

public sealed record CreatePublicInquiryCommand(
    Guid TenantId,
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
    string Source,
    string? IpAddress,
    string? UserAgent) : IRequest<Guid>;
