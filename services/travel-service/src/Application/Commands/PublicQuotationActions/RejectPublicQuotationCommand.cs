using MediatR;

namespace TravelService.Application.Commands.PublicQuotationActions;

public sealed record RejectPublicQuotationCommand(string Token, string? Reason) : IRequest<bool>;
