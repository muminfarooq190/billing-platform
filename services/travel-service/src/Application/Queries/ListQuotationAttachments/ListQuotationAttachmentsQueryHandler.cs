using Dapper;
using MediatR;
using TravelService.Application.Abstractions;

namespace TravelService.Application.Queries.ListQuotationAttachments;

public sealed class ListQuotationAttachmentsQueryHandler(
    IReadDbConnectionFactory connectionFactory,
    IFileStorage fileStorage) : IRequestHandler<ListQuotationAttachmentsQuery, IReadOnlyList<QuotationAttachmentReadModel>>
{
    public async Task<IReadOnlyList<QuotationAttachmentReadModel>> Handle(ListQuotationAttachmentsQuery request, CancellationToken cancellationToken)
    {
        await using var connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken) as IAsyncDisposable;
        var dbConnection = (System.Data.IDbConnection)connection!;

        const string sql = @"
SELECT id,
       quotation_id AS QuotationId,
       quotation_revision_id AS QuotationRevisionId,
       tenant_id AS TenantId,
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
WHERE tenant_id = @TenantId
  AND quotation_id = @QuotationId
  AND deleted_at IS NULL
  AND (@CustomerVisibleOnly = FALSE OR is_customer_visible = TRUE)
ORDER BY sort_order, created_at;";

        var rows = await dbConnection.QueryAsync<FlatQuotationAttachmentReadModel>(sql, new
        {
            request.TenantId,
            request.QuotationId,
            request.CustomerVisibleOnly
        });

        var results = new List<QuotationAttachmentReadModel>();
        foreach (var row in rows)
        {
            var readUrl = await fileStorage.GetReadUrlAsync(row.StorageKey, cancellationToken);
            results.Add(new QuotationAttachmentReadModel(
                Guid.Parse(row.Id),
                Guid.Parse(row.QuotationId),
                string.IsNullOrWhiteSpace(row.QuotationRevisionId) ? null : Guid.Parse(row.QuotationRevisionId),
                Guid.Parse(row.TenantId),
                row.OriginalFileName,
                row.ContentType,
                row.SizeBytes,
                row.AttachmentType,
                row.Caption,
                row.IsCustomerVisible,
                row.SortOrder,
                readUrl,
                DateTimeOffset.Parse(row.CreatedAt)));
        }

        return results.AsReadOnly();
    }

    private sealed class FlatQuotationAttachmentReadModel
    {
        public string Id { get; init; } = string.Empty;
        public string QuotationId { get; init; } = string.Empty;
        public string? QuotationRevisionId { get; init; }
        public string TenantId { get; init; } = string.Empty;
        public string OriginalFileName { get; init; } = string.Empty;
        public string ContentType { get; init; } = string.Empty;
        public long SizeBytes { get; init; }
        public string AttachmentType { get; init; } = string.Empty;
        public string? Caption { get; init; }
        public bool IsCustomerVisible { get; init; }
        public int SortOrder { get; init; }
        public string StorageKey { get; init; } = string.Empty;
        public string CreatedAt { get; init; } = string.Empty;
    }
}
