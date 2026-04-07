using MediatR;

namespace TravelService.Application.Commands.MarkPublicQuotationViewed;

public sealed record MarkPublicQuotationViewedCommand(string Token) : IRequest<bool>;
