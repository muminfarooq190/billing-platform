using MediatR;

namespace BillingService.Application.Commands.MarkInvoiceOverdue;

public sealed record MarkInvoiceOverdueCommand(Guid InvoiceId) : IRequest;
