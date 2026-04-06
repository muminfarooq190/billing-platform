using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TravelService.Infrastructure.Persistence;

namespace TravelService.Api.Controllers;

[ApiController]
[Route("travel/files")]
public sealed class FilesController(
    IWebHostEnvironment environment,
    TravelDbContext dbContext,
    ITenantContext tenantContext) : ControllerBase
{
    [HttpGet("{**storageKey}")]
    public async Task<IActionResult> Read(string storageKey, CancellationToken cancellationToken)
    {
        var attachment = await dbContext.QuotationAttachments
            .AsNoTracking()
            .SingleOrDefaultAsync(
                x => x.StorageKey == storageKey && x.TenantId == tenantContext.TenantId && x.DeletedAt == null,
                cancellationToken);

        if (attachment is null)
            return NotFound();

        var normalizedKey = storageKey.Replace('/', Path.DirectorySeparatorChar);
        var fullPath = Path.Combine(environment.ContentRootPath, "storage", normalizedKey);

        if (!System.IO.File.Exists(fullPath))
            return NotFound();

        return PhysicalFile(fullPath, attachment.ContentType, attachment.OriginalFileName, enableRangeProcessing: true);
    }
}
