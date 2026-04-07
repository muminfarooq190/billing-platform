namespace TravelService.Api.Contracts;

public sealed record CreateQuotationRevisionRequest(
    string Title,
    string Destination,
    DateTimeOffset TravelDate,
    DateTimeOffset ReturnDate,
    int Travellers,
    string Currency,
    string VisibleNotes,
    string InternalNotes,
    DateTimeOffset ValidUntil,
    List<QuotationRevisionLineItemRequest> LineItems);

public sealed record QuotationRevisionLineItemRequest(string Description, decimal UnitPrice, int Quantity, string Currency);
