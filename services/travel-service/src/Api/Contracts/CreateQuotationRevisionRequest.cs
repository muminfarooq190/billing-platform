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
    List<QuotationRevisionLineItemRequest> LineItems,
    List<string>? Inclusions = null,
    List<string>? Exclusions = null,
    string? PaymentTerms = null,
    string? CancellationPolicy = null);

public sealed record QuotationRevisionLineItemRequest(string Description, decimal UnitPrice, int Quantity, string Currency);
