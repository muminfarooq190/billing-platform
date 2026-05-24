namespace BillingService.Domain.Enums;

/// <summary>
/// Invoice lifecycle states.
///
///   Draft     — created but not yet issued to the customer.
///   Issued    — sent to customer, awaiting payment.
///   Paid      — collected via gateway (Stripe) or manual capture.
///   Overdue   — issued but past due date.
///   Refunded  — was paid then fully reversed via gateway (`charge.refunded`).
///   Void      — admin-cancelled; never collectable.
/// </summary>
public enum InvoiceStatus { Draft, Issued, Paid, Overdue, Refunded, Void }
