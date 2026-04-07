using MediatR;

namespace TravelService.Application.Commands.AcceptQuotation;

public sealed record AcceptQuotationCommand(Guid TenantId, Guid QuotationId, Guid RevisionId, string? Reason) : IRequest;
