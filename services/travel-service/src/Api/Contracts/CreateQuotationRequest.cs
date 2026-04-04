namespace TravelService.Api.Contracts;

public sealed record CreateQuotationRequest(
    Guid TenantId,
    Guid CustomerContactId,
    string CustomerName,
    string Title,
    string Destination,
    DateTimeOffset TravelDate,
    DateTimeOffset ReturnDate,
    int Travellers,
    string Currency,
    string Notes,
    List<QuotationLineItemRequest> LineItems);

public sealed record QuotationLineItemRequest(string Description, decimal UnitPrice, int Quantity, string Currency);
