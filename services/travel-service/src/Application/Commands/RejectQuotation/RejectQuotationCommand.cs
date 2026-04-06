using MediatR;

namespace TravelService.Application.Commands.RejectQuotation;

public sealed record RejectQuotationCommand(Guid TenantId, Guid QuotationId, string? Reason) : IRequest;
