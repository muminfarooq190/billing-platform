using Dapper;
using MediatR;
using TravelService.Application.Abstractions;

namespace TravelService.Application.Queries.TravelInquiries;

public sealed class GetTravelInquiryByIdQueryHandler(IReadDbConnectionFactory connectionFactory) : IRequestHandler<GetTravelInquiryByIdQuery, TravelInquiryDetailReadModel?>
{
    public async Task<TravelInquiryDetailReadModel?> Handle(GetTravelInquiryByIdQuery request, CancellationToken cancellationToken)
    {
        await using var connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken) as IAsyncDisposable;
        var dbConnection = (System.Data.IDbConnection)connection!;

        return await dbConnection.QuerySingleOrDefaultAsync<TravelInquiryDetailReadModel>(@"
SELECT id, tenant_id AS TenantId, source, status, full_name AS FullName, email, phone, whatsapp_number AS WhatsappNumber,
       departure_city AS DepartureCity, destination, travel_date AS TravelDate, return_date AS ReturnDate,
       is_date_flexible AS IsDateFlexible, travellers, budget_amount AS BudgetAmount, budget_currency AS BudgetCurrency,
       customer_message AS CustomerMessage, assigned_to_user_id AS AssignedToUserId, converted_contact_id AS ConvertedContactId,
       converted_quotation_id AS ConvertedQuotationId, qualified_at AS QualifiedAt, contacted_at AS ContactedAt,
       disqualified_at AS DisqualifiedAt, converted_at AS ConvertedAt, created_at AS CreatedAt, updated_at AS UpdatedAt
FROM travel_inquiries
WHERE tenant_id = @TenantId AND id = @InquiryId AND deleted_at IS NULL", new { request.TenantId, request.InquiryId });
    }
}
