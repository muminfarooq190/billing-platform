using BillingService.Application.ReadModels;
using MediatR;

namespace BillingService.Application.Queries.ListInvoicesByTenant;

public sealed record ListInvoicesByTenantQuery(Guid TenantId, int Page, int PageSize, string? Status) : IRequest<IReadOnlyList<InvoiceReadModel>>;
