using Dapper;
using MediatR;
using TravelService.Application.Abstractions;
using TravelService.Application.Queries.QuotationRevisions;

namespace TravelService.Application.Queries.GetQuotationRevisionById;

public sealed class GetQuotationRevisionByIdQueryHandler(IReadDbConnectionFactory connectionFactory) : IRequestHandler<GetQuotationRevisionByIdQuery, QuotationRevisionReadModel?>
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
            lineItems);
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
}
