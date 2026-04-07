using Dapper;
using MediatR;
using TravelService.Application.Abstractions;

namespace TravelService.Application.Queries.GetPublicQuotationByToken;

public sealed class GetPublicQuotationByTokenQueryHandler(
    IReadDbConnectionFactory connectionFactory,
    IFileStorage fileStorage) : IRequestHandler<GetPublicQuotationByTokenQuery, PublicQuotationReadModel?>
{
    public async Task<PublicQuotationReadModel?> Handle(GetPublicQuotationByTokenQuery request, CancellationToken cancellationToken)
    {
        await using var connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken) as IAsyncDisposable;
        var db = (System.Data.IDbConnection)connection!;

        const string quotationSql = @"
SELECT q.id AS QuotationId,
       r.id AS RevisionId,
       r.customer_name AS CustomerName,
       r.title AS Title,
       r.destination AS Destination,
       r.travel_date AS TravelDate,
       r.return_date AS ReturnDate,
       r.travellers AS Travellers,
       r.currency AS Currency,
       r.visible_notes AS VisibleNotes,
       r.valid_until AS ValidUntil,
       r.total_amount AS TotalAmount,
       q.last_sent_at AS SentAt,
       q.last_viewed_at AS LastViewedAt
FROM quotation_share_links sl
INNER JOIN quotations q ON q.id = sl.quotation_id
INNER JOIN quotation_revisions r ON r.id = sl.quotation_revision_id
WHERE sl.token = @Token
  AND sl.revoked_at IS NULL
  AND (sl.expires_at IS NULL OR sl.expires_at >= NOW())
  AND q.deleted_at IS NULL;";

        var quotation = await db.QuerySingleOrDefaultAsync<PublicQuotationFlatRow>(quotationSql, new { request.Token });
        if (quotation is null)
            return null;

        const string lineItemsSql = @"
SELECT description AS Description,
       quantity AS Quantity,
       unit_price_amount AS UnitPriceAmount,
       currency AS Currency,
       sort_order AS SortOrder,
       (unit_price_amount * quantity) AS LineTotal
FROM quotation_revision_line_items
WHERE quotation_revision_id = @RevisionId
ORDER BY sort_order, id;";

        const string attachmentsSql = @"
SELECT id AS Id,
       original_file_name AS OriginalFileName,
       content_type AS ContentType,
       size_bytes AS SizeBytes,
       attachment_type AS AttachmentType,
       caption AS Caption,
       sort_order AS SortOrder,
       storage_key AS StorageKey
FROM quotation_attachments
WHERE quotation_id = @QuotationId
  AND deleted_at IS NULL
  AND is_customer_visible = TRUE
  AND (quotation_revision_id IS NULL OR quotation_revision_id = @RevisionId)
ORDER BY sort_order, created_at;";

        var lineItems = (await db.QueryAsync<PublicQuotationLineItemReadModel>(lineItemsSql, new { quotation.RevisionId })).ToList();
        var attachmentRows = await db.QueryAsync<PublicQuotationAttachmentFlatRow>(attachmentsSql, new { quotation.QuotationId, quotation.RevisionId });
        var attachments = new List<PublicQuotationAttachmentReadModel>();
        foreach (var attachment in attachmentRows)
        {
            var readUrl = await fileStorage.GetSignedReadUrlAsync(attachment.StorageKey, TimeSpan.FromHours(1), cancellationToken);
            attachments.Add(new PublicQuotationAttachmentReadModel(
                attachment.Id,
                attachment.OriginalFileName,
                attachment.ContentType,
                attachment.SizeBytes,
                attachment.AttachmentType,
                attachment.Caption,
                attachment.SortOrder,
                readUrl));
        }

        return new PublicQuotationReadModel(
            quotation.QuotationId,
            quotation.RevisionId,
            quotation.CustomerName,
            quotation.Title,
            quotation.Destination,
            quotation.TravelDate,
            quotation.ReturnDate,
            quotation.Travellers,
            quotation.Currency,
            quotation.VisibleNotes,
            quotation.ValidUntil,
            quotation.TotalAmount,
            quotation.SentAt,
            quotation.LastViewedAt,
            lineItems,
            attachments);
    }

    private sealed record PublicQuotationFlatRow(
        Guid QuotationId,
        Guid RevisionId,
        string CustomerName,
        string Title,
        string Destination,
        DateTimeOffset TravelDate,
        DateTimeOffset ReturnDate,
        int Travellers,
        string Currency,
        string VisibleNotes,
        DateTimeOffset ValidUntil,
        decimal TotalAmount,
        DateTimeOffset? SentAt,
        DateTimeOffset? LastViewedAt);

    private sealed record PublicQuotationAttachmentFlatRow(
        Guid Id,
        string OriginalFileName,
        string ContentType,
        long SizeBytes,
        string AttachmentType,
        string? Caption,
        int SortOrder,
        string StorageKey);
}
