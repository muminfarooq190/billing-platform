using Dapper;
using MediatR;
using TravelService.Application.Abstractions;
using TravelService.Application.Queries.QuotationRevisions;

namespace TravelService.Application.Queries.GetQuotationRevisionById;

public sealed class GetQuotationRevisionByIdQueryHandler(
    IReadDbConnectionFactory connectionFactory,
    IFileStorage fileStorage) : IRequestHandler<GetQuotationRevisionByIdQuery, QuotationRevisionReadModel?>
{
    public async Task<QuotationRevisionReadModel?> Handle(GetQuotationRevisionByIdQuery request, CancellationToken cancellationToken)
    {
        await using var connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken) as IAsyncDisposable;
        var dbConnection = (System.Data.IDbConnection)connection!;

        const string revisionSql = @"
SELECT id,
       quotation_id AS QuotationId,
       tenant_id AS TenantId,
       revision_number AS RevisionNumber,
       status,
       customer_contact_id AS CustomerContactId,
       customer_name AS CustomerName,
       title,
       destination,
       travel_date AS TravelDate,
       return_date AS ReturnDate,
       travellers,
       currency,
       notes,
       visible_notes AS VisibleNotes,
       internal_notes AS InternalNotes,
       valid_until AS ValidUntil,
       subtotal_amount AS SubtotalAmount,
       tax_amount AS TaxAmount,
       total_amount AS TotalAmount,
       created_by_user_id AS CreatedByUserId,
       created_at AS CreatedAt,
       COALESCE(inclusions_json, '[]'::jsonb)::text AS InclusionsJson,
       COALESCE(exclusions_json, '[]'::jsonb)::text AS ExclusionsJson,
       COALESCE(payment_terms, '') AS PaymentTerms,
       COALESCE(cancellation_policy, '') AS CancellationPolicy
FROM quotation_revisions
WHERE tenant_id = @TenantId AND quotation_id = @QuotationId AND id = @RevisionId;";

        var revision = await dbConnection.QuerySingleOrDefaultAsync<FlatQuotationRevisionReadModel>(revisionSql, new { request.TenantId, request.QuotationId, request.RevisionId });
        if (revision is null)
            return null;

        const string lineItemsSql = @"
SELECT id,
       description,
       quantity,
       unit_price_amount AS UnitPriceAmount,
       currency,
       sort_order AS SortOrder,
       unit_price_amount * quantity AS LineTotal
FROM quotation_revision_line_items
WHERE quotation_revision_id = @RevisionId
ORDER BY sort_order;";

        var lineItems = (await dbConnection.QueryAsync<QuotationRevisionLineItemReadModel>(lineItemsSql, new { request.RevisionId })).ToList().AsReadOnly();

        const string attachmentsSql = @"
SELECT id,
       original_file_name AS OriginalFileName,
       content_type AS ContentType,
       size_bytes AS SizeBytes,
       attachment_type AS AttachmentType,
       caption,
       is_customer_visible AS IsCustomerVisible,
       sort_order AS SortOrder,
       storage_key AS StorageKey,
       created_at AS CreatedAt
FROM quotation_attachments
WHERE quotation_id = @QuotationId
  AND quotation_revision_id = @RevisionId
  AND tenant_id = @TenantId
  AND deleted_at IS NULL
ORDER BY sort_order, created_at;";

        var attachmentRows = await dbConnection.QueryAsync<FlatQuotationRevisionAttachmentReadModel>(attachmentsSql, new { request.QuotationId, request.RevisionId, request.TenantId });
        var attachments = new List<QuotationRevisionAttachmentReadModel>();
        foreach (var attachment in attachmentRows)
        {
            var readUrl = await fileStorage.GetReadUrlAsync(attachment.StorageKey, cancellationToken);
            attachments.Add(new QuotationRevisionAttachmentReadModel
            {
                Id = attachment.Id,
                OriginalFileName = attachment.OriginalFileName,
                ContentType = attachment.ContentType,
                SizeBytes = attachment.SizeBytes,
                AttachmentType = attachment.AttachmentType,
                Caption = attachment.Caption,
                IsCustomerVisible = attachment.IsCustomerVisible,
                SortOrder = attachment.SortOrder,
                ReadUrl = readUrl,
                CreatedAt = attachment.CreatedAt,
            });
        }

        return new QuotationRevisionReadModel
        {
            Id = revision.Id,
            QuotationId = revision.QuotationId,
            TenantId = revision.TenantId,
            RevisionNumber = revision.RevisionNumber,
            Status = revision.Status,
            CustomerContactId = revision.CustomerContactId,
            CustomerName = revision.CustomerName,
            Title = revision.Title,
            Destination = revision.Destination,
            TravelDate = revision.TravelDate,
            ReturnDate = revision.ReturnDate,
            Travellers = revision.Travellers,
            Currency = revision.Currency,
            Notes = revision.Notes,
            VisibleNotes = revision.VisibleNotes,
            InternalNotes = revision.InternalNotes,
            ValidUntil = revision.ValidUntil,
            SubtotalAmount = revision.SubtotalAmount,
            TaxAmount = revision.TaxAmount,
            TotalAmount = revision.TotalAmount,
            CreatedByUserId = revision.CreatedByUserId,
            CreatedAt = revision.CreatedAt,
            InclusionsJson = revision.InclusionsJson,
            ExclusionsJson = revision.ExclusionsJson,
            PaymentTerms = revision.PaymentTerms,
            CancellationPolicy = revision.CancellationPolicy,
            LineItems = lineItems,
            Attachments = attachments.AsReadOnly(),
        };
    }

    private sealed class FlatQuotationRevisionReadModel
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
    }

    private sealed class FlatQuotationRevisionAttachmentReadModel
    {
        public Guid Id { get; set; }
        public string OriginalFileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public long SizeBytes { get; set; }
        public string AttachmentType { get; set; } = string.Empty;
        public string? Caption { get; set; }
        public bool IsCustomerVisible { get; set; }
        public int SortOrder { get; set; }
        public string StorageKey { get; set; } = string.Empty;
        public DateTimeOffset CreatedAt { get; set; }
    }
}
