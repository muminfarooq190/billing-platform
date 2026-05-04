namespace TravelService.Domain.Enums;

public enum BookingPaymentStatus
{
    Scheduled = 1,
    Pending = 2,
    Paid = 3,
    Refunded = 4,
    Failed = 5,
    Waived = 6
}
