using TravelService.Domain.Exceptions;
using TravelService.Domain.Enums;

namespace TravelService.Domain.Aggregates;

public sealed class QuotationApprovalRequest
{
    private QuotationApprovalRequest() { }

    private QuotationApprovalRequest(Guid quotationId, Guid tenantId, Guid revisionId, string reason, decimal totalAmount, decimal? marginPercent, decimal? discountPercent, Guid? requestedByUserId)
    {
        if (quotationId == Guid.Empty) throw new DomainException("QuotationId is required.");
        if (tenantId == Guid.Empty) throw new DomainException("TenantId is required.");
        if (revisionId == Guid.Empty) throw new DomainException("RevisionId is required.");
        if (string.IsNullOrWhiteSpace(reason)) throw new DomainException("Approval reason is required.");

        Id = Guid.NewGuid();
        QuotationId = quotationId;
        TenantId = tenantId;
        RevisionId = revisionId;
        Reason = reason.Trim();
        TotalAmount = totalAmount;
        MarginPercent = marginPercent;
        DiscountPercent = discountPercent;
        Status = QuotationApprovalStatus.Pending;
        RequestedByUserId = requestedByUserId;
        RequestedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }
    public Guid QuotationId { get; private set; }
    public Guid TenantId { get; private set; }
    public Guid RevisionId { get; private set; }
    public string Reason { get; private set; } = string.Empty;
    public decimal TotalAmount { get; private set; }
    public decimal? MarginPercent { get; private set; }
    public decimal? DiscountPercent { get; private set; }
    public QuotationApprovalStatus Status { get; private set; }
    public Guid? RequestedByUserId { get; private set; }
    public Guid? ReviewedByUserId { get; private set; }
    public string? DecisionReason { get; private set; }
    public DateTimeOffset RequestedAt { get; private set; }
    public DateTimeOffset? ReviewedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public static QuotationApprovalRequest Create(Guid quotationId, Guid tenantId, Guid revisionId, string reason, decimal totalAmount, decimal? marginPercent, decimal? discountPercent, Guid? requestedByUserId = null)
        => new(quotationId, tenantId, revisionId, reason, totalAmount, marginPercent, discountPercent, requestedByUserId);

    public void Approve(Guid? reviewedByUserId, string? decisionReason)
    {
        if (Status != QuotationApprovalStatus.Pending)
            throw new DomainException("Only pending approval requests can be approved.");

        Status = QuotationApprovalStatus.Approved;
        ReviewedByUserId = reviewedByUserId;
        DecisionReason = string.IsNullOrWhiteSpace(decisionReason) ? null : decisionReason.Trim();
        ReviewedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Reject(Guid? reviewedByUserId, string? decisionReason)
    {
        if (Status != QuotationApprovalStatus.Pending)
            throw new DomainException("Only pending approval requests can be rejected.");

        Status = QuotationApprovalStatus.Rejected;
        ReviewedByUserId = reviewedByUserId;
        DecisionReason = string.IsNullOrWhiteSpace(decisionReason) ? null : decisionReason.Trim();
        ReviewedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
