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
       created_at AS CreatedAt
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
            attachments.Add(new QuotationRevisionAttachmentReadModel(
                attachment.Id,
                attachment.OriginalFileName,
                attachment.ContentType,
                attachment.SizeBytes,
                attachment.AttachmentType,
                attachment.Caption,
                attachment.IsCustomerVisible,
                attachment.SortOrder,
                readUrl,
                attachment.CreatedAt));
        }

        return new QuotationRevisionReadModel(
            revision.Id,
            revision.QuotationId,
            revision.TenantId,
            revision.RevisionNumber,
            revision.Status,
            revision.CustomerContactId,
            revision.CustomerName,
            revision.Title,
            revision.Destination,
            revision.TravelDate,
            revision.ReturnDate,
            revision.Travellers,
            revision.Currency,
            revision.Notes,
            revision.VisibleNotes,
            revision.InternalNotes,
            revision.ValidUntil,
            revision.SubtotalAmount,
            revision.TaxAmount,
            revision.TotalAmount,
            revision.CreatedByUserId,
            revision.CreatedAt,
            lineItems,
            attachments.AsReadOnly());
    }

    private sealed record FlatQuotationRevisionReadModel(
        Guid Id,
        Guid QuotationId,
        Guid TenantId,
        int RevisionNumber,
        string Status,
        Guid CustomerContactId,
        string CustomerName,
        string Title,
        string Destination,
        DateTimeOffset TravelDate,
        DateTimeOffset ReturnDate,
        int Travellers,
        string Currency,
        string Notes,
        string VisibleNotes,
        string InternalNotes,
        DateTimeOffset ValidUntil,
        decimal SubtotalAmount,
        decimal TaxAmount,
        decimal TotalAmount,
        Guid? CreatedByUserId,
        DateTimeOffset CreatedAt);

    private sealed record FlatQuotationRevisionAttachmentReadModel(
        Guid Id,
        string OriginalFileName,
        string ContentType,
        long SizeBytes,
        string AttachmentType,
        string? Caption,
        bool IsCustomerVisible,
        int SortOrder,
        string StorageKey,
        DateTimeOffset CreatedAt);
}
