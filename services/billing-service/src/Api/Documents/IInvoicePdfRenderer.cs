using BillingService.Application.ReadModels;

namespace BillingService.Api.Documents;

public interface IInvoicePdfRenderer
{
    byte[] RenderInvoicePdf(InvoiceReadModel invoice);
}
