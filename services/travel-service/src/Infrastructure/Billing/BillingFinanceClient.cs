using System.Net.Http.Json;
using TravelService.Application.Abstractions;

namespace TravelService.Infrastructure.Billing;

public sealed class BillingFinanceClient(HttpClient httpClient) : IBillingFinanceClient
{
    public async Task<IReadOnlyList<BookingInvoiceDto>> GetInvoicesAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        var response = await httpClient.GetFromJsonAsync<IReadOnlyList<BookingInvoiceDto>>($"billing/invoices/tenant/{tenantId}", cancellationToken);
        return response ?? [];
    }
}
