namespace TravelService.Application.Queries.TravelInquiries;

public class TravelInquiryListItemReadModel
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Source { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Destination { get; set; }
    public DateTimeOffset? TravelDate { get; set; }
    public DateTimeOffset? ReturnDate { get; set; }
    public int? Travellers { get; set; }
    public decimal? BudgetAmount { get; set; }
    public string? BudgetCurrency { get; set; }
    public Guid? AssignedToUserId { get; set; }
    public Guid? ConvertedQuotationId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}

public sealed class TravelInquiryDetailReadModel : TravelInquiryListItemReadModel
{
    public string? WhatsappNumber { get; set; }
    public string? DepartureCity { get; set; }
    public bool IsDateFlexible { get; set; }
    public string? CustomerMessage { get; set; }
    public Guid? ConvertedContactId { get; set; }
    public DateTimeOffset? QualifiedAt { get; set; }
    public DateTimeOffset? ContactedAt { get; set; }
    public DateTimeOffset? DisqualifiedAt { get; set; }
    public DateTimeOffset? ConvertedAt { get; set; }
}

public sealed class TravelInquiryHistoryReadModel
{
    public Guid Id { get; set; }
    public Guid TravelInquiryId { get; set; }
    public Guid TenantId { get; set; }
    public string? FromStatus { get; set; }
    public string ToStatus { get; set; } = string.Empty;
    public string? Reason { get; set; }
    public Guid? ChangedByUserId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
