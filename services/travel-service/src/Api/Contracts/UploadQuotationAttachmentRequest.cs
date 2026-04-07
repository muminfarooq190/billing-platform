using Microsoft.AspNetCore.Mvc;

namespace TravelService.Api.Contracts;

public sealed class UploadQuotationAttachmentRequest
{
    [FromForm(Name = "file")]
    public IFormFile File { get; set; } = null!;

    [FromForm(Name = "attachmentType")]
    public string AttachmentType { get; set; } = string.Empty;

    [FromForm(Name = "caption")]
    public string? Caption { get; set; }

    [FromForm(Name = "quotationRevisionId")]
    public Guid? QuotationRevisionId { get; set; }

    [FromForm(Name = "isCustomerVisible")]
    public bool IsCustomerVisible { get; set; }

    [FromForm(Name = "sortOrder")]
    public int SortOrder { get; set; }
}
