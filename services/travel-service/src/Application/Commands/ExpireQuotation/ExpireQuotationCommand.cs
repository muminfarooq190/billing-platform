using MediatR;

namespace TravelService.Application.Commands.ExpireQuotation;

public sealed record ExpireQuotationCommand(Guid TenantId, Guid QuotationId, string? Reason) : IRequest;
