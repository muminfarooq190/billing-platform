using MediatR;

namespace TravelService.Application.Commands.PublicQuotationActions;

public sealed record AcceptPublicQuotationCommand(string Token, string? Reason) : IRequest<bool>;
