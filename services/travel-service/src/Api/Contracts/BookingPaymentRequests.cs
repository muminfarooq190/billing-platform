namespace TravelService.Api.Contracts;

public sealed record ScheduleBookingPaymentRequest(string? MilestoneLabel, DateTimeOffset DueDate, decimal Amount, string Currency, string? Notes);

public sealed record UpdateBookingPaymentRequest(string? MilestoneLabel, DateTimeOffset DueDate, decimal Amount, string Currency, string? Notes);

public sealed record MarkBookingPaymentPaidRequest(DateTimeOffset? PaidAt, string PaymentMethod, string? ProviderReference, string? Notes);

public sealed record RefundBookingPaymentRequest(string? Notes);

public sealed record WaiveBookingPaymentRequest(string? Reason);
