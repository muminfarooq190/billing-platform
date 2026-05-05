using MediatR;

namespace TravelService.Application.Commands.TravelInquiries;

public sealed record CreateTravelInquiryCommand(
    Guid TenantId,
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
    Guid? AssignedToUserId) : IRequest<Guid>;
