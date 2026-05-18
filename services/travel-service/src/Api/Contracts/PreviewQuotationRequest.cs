namespace TravelService.Api.Contracts;

public sealed class PreviewQuotationRequest
{
    public Guid? QuotationId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? CustomerName { get; set; }
    public string? Destination { get; set; }
    public DateTimeOffset? TravelDate { get; set; }
    public DateTimeOffset? ReturnDate { get; set; }
    public int Travellers { get; set; } = 1;
    public string Currency { get; set; } = "USD";
    public string? Notes { get; set; }
    public string? VisibleNotes { get; set; }
    public string? InternalNotes { get; set; }
    public DateTimeOffset? ValidUntil { get; set; }
    public decimal? TaxAmount { get; set; }
    public decimal? TotalAmount { get; set; }
    public List<PreviewQuotationLineItem>? LineItems { get; set; }

    // Optional template-driven policy fields. Mirrors the persisted
    // QuotationRevision shape so the preview PDF can render inclusions,
    // exclusions, payment terms and cancellation policy the same way the
    // saved PDF does.
    public string? InclusionsJson { get; set; }
    public string? ExclusionsJson { get; set; }
    public string? PaymentTerms { get; set; }
    public string? CancellationPolicy { get; set; }
}

public sealed class PreviewQuotationLineItem
{
    public string Description { get; set; } = string.Empty;
    public int Quantity { get; set; } = 1;
    public decimal UnitPriceAmount { get; set; }
    public string? Currency { get; set; }
}
