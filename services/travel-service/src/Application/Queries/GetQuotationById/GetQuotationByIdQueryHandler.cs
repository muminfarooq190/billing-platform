using Dapper;
using TravelService.Application.Abstractions;
using MediatR;

namespace TravelService.Application.Queries.GetQuotationById;

public sealed class GetQuotationByIdQueryHandler(IReadDbConnectionFactory connectionFactory) : IRequestHandler<GetQuotationByIdQuery, QuotationReadModel?>
{
    public async Task<QuotationReadModel?> Handle(GetQuotationByIdQuery request, CancellationToken cancellationToken)
    {
        await using var connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken) as IAsyncDisposable;
        var dbConnection = (System.Data.IDbConnection)connection!;
        return await dbConnection.QuerySingleOrDefaultAsync<QuotationReadModel>(
            @"SELECT id,
                     tenant_id AS TenantId,
                     customer_contact_id AS CustomerContactId,
                     customer_name AS CustomerName,
                     title,
                     destination,
                     travel_date AS TravelDate,
                     return_date AS ReturnDate,
                     travellers,
                     currency,
                     notes,
                     status,
                     valid_until AS ValidUntil,
                     current_revision_number AS CurrentRevisionNumber,
                     accepted_revision_id AS AcceptedRevisionId,
                     COALESCE((SELECT SUM(unit_price * quantity) FROM quotation_line_items WHERE quotation_id = quotations.id), 0) AS TotalAmount,
                     COALESCE((SELECT COUNT(*) FROM quotation_attachments WHERE quotation_id = quotations.id AND deleted_at IS NULL), 0) AS AttachmentCount,
                     EXISTS(SELECT 1 FROM quotation_attachments WHERE quotation_id = quotations.id AND deleted_at IS NULL AND is_customer_visible = TRUE) AS HasCustomerVisibleAttachments,
                     last_sent_at AS LastSentAt,
                     last_viewed_at AS LastViewedAt,
                     expired_at AS ExpiredAt,
                     rejected_at AS RejectedAt,
                     created_at AS CreatedAt,
                     updated_at AS UpdatedAt
              FROM quotations
              WHERE id = @Id AND deleted_at IS NULL",
            new { request.Id });
    }
}
