namespace TravelService.Application.Queries.QuotationRevisions;

public sealed record QuotationRevisionLineItemReadModel(
    Guid Id,
    string Description,
    int Quantity,
    decimal UnitPriceAmount,
    string Currency,
    int SortOrder,
    decimal LineTotal);

// Plain class (settable props + parameterless ctor) so Dapper uses property
// binding instead of positional ctor matching. Required because Npgsql 8 returns
// DateTime for timestamptz, which doesn't match a positional DateTimeOffset ctor.
// TypeHandler (registered in Program.cs) converts DateTime to DateTimeOffset on
// individual property writes.
public sealed class QuotationRevisionSummaryReadModel
{
    public Guid Id { get; set; }
    public Guid QuotationId { get; set; }
    public Guid TenantId { get; set; }
    public int RevisionNumber { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Destination { get; set; } = string.Empty;
    public DateTimeOffset TravelDate { get; set; }
    public DateTimeOffset ReturnDate { get; set; }
    public int Travellers { get; set; }
    public string Currency { get; set; } = string.Empty;
    public DateTimeOffset ValidUntil { get; set; }
    public decimal SubtotalAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public string InclusionsJson { get; set; } = "[]";
    public string ExclusionsJson { get; set; } = "[]";
    public string PaymentTerms { get; set; } = string.Empty;
    public string CancellationPolicy { get; set; } = string.Empty;
}

public sealed class QuotationRevisionAttachmentReadModel
{
    public Guid Id { get; set; }
    public string OriginalFileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
    public string AttachmentType { get; set; } = string.Empty;
    public string? Caption { get; set; }
    public bool IsCustomerVisible { get; set; }
    public int SortOrder { get; set; }
    public string ReadUrl { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
}

public sealed class QuotationRevisionReadModel
{
    public Guid Id { get; set; }
    public Guid QuotationId { get; set; }
    public Guid TenantId { get; set; }
    public int RevisionNumber { get; set; }
    public string Status { get; set; } = string.Empty;
    public Guid CustomerContactId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Destination { get; set; } = string.Empty;
    public DateTimeOffset TravelDate { get; set; }
    public DateTimeOffset ReturnDate { get; set; }
    public int Travellers { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public string VisibleNotes { get; set; } = string.Empty;
    public string InternalNotes { get; set; } = string.Empty;
    public DateTimeOffset ValidUntil { get; set; }
    public decimal SubtotalAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public Guid? CreatedByUserId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public string InclusionsJson { get; set; } = "[]";
    public string ExclusionsJson { get; set; } = "[]";
    public string PaymentTerms { get; set; } = string.Empty;
    public string CancellationPolicy { get; set; } = string.Empty;
    public IReadOnlyList<QuotationRevisionLineItemReadModel> LineItems { get; set; } = [];
    public IReadOnlyList<QuotationRevisionAttachmentReadModel> Attachments { get; set; } = [];
}
