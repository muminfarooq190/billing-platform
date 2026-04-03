using BillingService.Application.ReadModels;
using MediatR;

namespace BillingService.Application.Queries.GetInvoiceById;

public sealed record GetInvoiceByIdQuery(Guid InvoiceId) : IRequest<InvoiceReadModel?>;
