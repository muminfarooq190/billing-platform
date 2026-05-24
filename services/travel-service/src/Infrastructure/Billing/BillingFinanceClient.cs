using System.Net.Http.Json;
using TravelService.Application.Abstractions;

namespace TravelService.Infrastructure.Billing;

public sealed class BillingFinanceClient(HttpClient httpClient, ILogger<BillingFinanceClient> logger) : IBillingFinanceClient
{
    public async Task<IReadOnlyList<BookingInvoiceDto>> GetInvoicesAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        var response = await httpClient.GetFromJsonAsync<IReadOnlyList<BookingInvoiceDto>>($"billing/invoices/tenant/{tenantId}", cancellationToken);
        return response ?? [];
    }

    /// <inheritdoc />
    /// <remarks>
    /// Posts to <c>POST /billing/refunds</c> in billing-service which in turn
    /// calls Stripe. Two failure modes — both return null + log warning so
    /// the caller can decide whether to still mark the local payment as
    /// refunded (offline-only) or surface the error to the user:
    ///   1. Network / 5xx — billing-service unreachable
    ///   2. 503 — STRIPE_SECRET_KEY missing
    /// Any 4xx other than 503 throws (it indicates a programmer error in
    /// the calling handler — e.g. wrong ProviderReference shape).
    /// </remarks>
    public async Task<BookingRefundResult?> RefundAsync(BookingRefundRequest request, CancellationToken cancellationToken)
    {
        try
        {
            using var response = await httpClient.PostAsJsonAsync("billing/refunds", new
            {
                providerReference = request.ProviderReference,
                amount = request.Amount,
                currency = request.Currency,
                reason = request.Reason,
                tenantId = request.TenantId.ToString("D"),
            }, cancellationToken);

            if ((int)response.StatusCode == 503)
            {
                logger.LogWarning("Billing-service refund returned 503 — Stripe is not configured. Falling back to offline refund.");
                return null;
            }

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync(cancellationToken);
                logger.LogError("Billing-service refund returned {Status}: {Body}", (int)response.StatusCode, body);
                throw new InvalidOperationException($"Refund failed at billing-service ({(int)response.StatusCode}): {body}");
            }

            return await response.Content.ReadFromJsonAsync<BookingRefundResult>(cancellationToken: cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            logger.LogWarning(ex, "Billing-service unreachable for refund. Falling back to offline refund.");
            return null;
        }
    }
}
