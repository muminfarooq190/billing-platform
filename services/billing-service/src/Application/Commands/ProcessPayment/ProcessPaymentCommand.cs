using MediatR;

namespace BillingService.Application.Commands.ProcessPayment;

public sealed record ProcessPaymentCommand(Guid InvoiceId) : IRequest<string>;
