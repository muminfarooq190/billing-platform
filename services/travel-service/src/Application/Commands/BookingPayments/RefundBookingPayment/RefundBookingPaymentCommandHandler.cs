using MediatR;
using TravelService.Application.Abstractions;
using TravelService.Domain.Aggregates;
using TravelService.Domain.Repositories;

namespace TravelService.Application.Commands.BookingPayments.RefundBookingPayment;

/// <summary>
/// Refund a booking payment.
///
/// Behavior:
///   - Stripe-captured payments (PaymentMethod == "Stripe" with a
///     Stripe-shaped ProviderReference) call billing-service which forwards
///     to Stripe `/v1/refunds`. Money moves back to the customer's card.
///   - Non-Stripe payments (Cash / Cheque / BankTransfer / Other) are
///     ledger-only — we flip the aggregate state and trust the operator
///     to reverse the funds out-of-band.
///   - Stripe call returning null (gateway unconfigured or unreachable) is
///     treated as offline-refund. The activity row records that fallback so
///     finance can reconcile.
///
/// Previous behavior only flipped the local aggregate; Stripe never saw
/// the refund and money stayed captured.
/// </summary>
public sealed class RefundBookingPaymentCommandHandler(
    IBookingRepository bookingRepository,
    IBookingPaymentRepository bookingPaymentRepository,
    IUnitOfWork unitOfWork,
    IActivityWriter activityWriter,
    IBillingFinanceClient billingFinanceClient) : IRequestHandler<RefundBookingPaymentCommand>
{
    public async Task Handle(RefundBookingPaymentCommand request, CancellationToken cancellationToken)
    {
        var booking = await bookingRepository.GetByIdAsync(request.BookingId, cancellationToken) ?? throw new InvalidOperationException("Booking not found.");
        var payment = await bookingPaymentRepository.GetByIdAsync(request.PaymentId, cancellationToken) ?? throw new InvalidOperationException("Payment not found.");
        if (booking.TenantId != request.TenantId || payment.TenantId != request.TenantId || payment.BookingId != booking.Id)
            throw new InvalidOperationException("Payment does not belong to booking.");

        BookingRefundResult? gatewayResult = null;
        var routedToGateway = false;
        if (IsStripeCaptured(payment))
        {
            routedToGateway = true;
            gatewayResult = await billingFinanceClient.RefundAsync(
                new BookingRefundRequest(
                    ProviderReference: payment.ProviderReference!,
                    Amount: payment.Amount,
                    Currency: payment.Currency,
                    Reason: request.Notes,
                    TenantId: request.TenantId),
                cancellationToken);
        }

        payment.Refund(request.Notes, request.ActorUserId);
        await bookingPaymentRepository.UpdateAsync(payment, cancellationToken);
        await activityWriter.WriteAsync(ActivityEntry.Create(
            request.TenantId,
            "Booking",
            booking.Id,
            "PaymentRefunded",
            $"Payment refunded for {booking.BookingNumber}",
            new
            {
                payment.Id,
                payment.Amount,
                payment.Currency,
                Gateway = routedToGateway ? "Stripe" : "Offline",
                gatewayResult?.RefundId,
                GatewayStatus = gatewayResult?.Status,
            },
            request.ActorUserId), cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Heuristic — true when the payment was captured through Stripe and the
    /// stored ProviderReference looks like a Stripe id (ch_ / pi_ / py_ prefix).
    /// Defends against mis-typed manual entries from agents who logged the
    /// payment method as "Stripe" but pasted a non-Stripe reference.
    /// </summary>
    private static bool IsStripeCaptured(BookingPayment payment)
    {
        if (!string.Equals(payment.PaymentMethod, "Stripe", StringComparison.OrdinalIgnoreCase)) return false;
        var reference = payment.ProviderReference;
        if (string.IsNullOrWhiteSpace(reference)) return false;
        return reference.StartsWith("ch_", StringComparison.OrdinalIgnoreCase)
            || reference.StartsWith("pi_", StringComparison.OrdinalIgnoreCase)
            || reference.StartsWith("py_", StringComparison.OrdinalIgnoreCase);
    }
}
