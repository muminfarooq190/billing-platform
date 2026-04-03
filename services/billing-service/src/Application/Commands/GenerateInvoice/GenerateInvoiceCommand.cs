using MediatR;

namespace BillingService.Application.Commands.GenerateInvoice;

public sealed record GenerateInvoiceCommand(Guid SubscriptionId) : IRequest<Guid>;
