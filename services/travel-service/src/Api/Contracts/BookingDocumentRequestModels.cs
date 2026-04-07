using Microsoft.AspNetCore.Http;

namespace TravelService.Api.Contracts;

public sealed class UploadBookingDocumentRequest
{
    public IFormFile? File { get; init; }
    public Guid? TravelerId { get; init; }
    public string DocumentType { get; init; } = string.Empty;
    public string? Description { get; init; }
    public bool IsCustomerVisible { get; init; }
}
