using BillingService.Api.Contracts;
using BillingService.Application.Abstractions;
using BillingService.Domain.Aggregates;
using BillingService.Domain.Enums;
using BillingService.Domain.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace BillingService.Api.Controllers;

[ApiController]
[Route("billing/features")]
public sealed class FeaturesController(IFeatureCatalogRepository featureCatalogRepository, IUnitOfWork unitOfWork) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        var items = await featureCatalogRepository.ListAsync(cancellationToken);
        return Ok(items.Select(Map));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateFeatureCatalogEntryRequest request, CancellationToken cancellationToken)
    {
        var existing = await featureCatalogRepository.GetByFeatureKeyAsync(request.FeatureKey, cancellationToken);
        if (existing is not null)
            return Conflict(new ProblemDetails { Status = 409, Detail = $"Feature '{request.FeatureKey}' already exists." });

        var entry = FeatureCatalogEntry.Create(request.FeatureKey, request.Service, request.Category, request.DisplayName, request.Description, request.IsQuota, request.Unit, request.MetadataJson, ParseAssignmentMode(request.AssignmentMode), request.DefaultAssignmentLimit);
        await featureCatalogRepository.AddAsync(entry, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return CreatedAtAction(nameof(List), new { featureKey = entry.FeatureKey }, Map(entry));
    }

    [HttpPut("{featureKey}")]
    public async Task<IActionResult> Update(string featureKey, [FromBody] UpdateFeatureCatalogEntryRequest request, CancellationToken cancellationToken)
    {
        var entry = await featureCatalogRepository.GetByFeatureKeyAsync(featureKey, cancellationToken);
        if (entry is null)
            return NotFound();

        entry.Update(request.Service, request.Category, request.DisplayName, request.Description, request.IsQuota, request.Unit, request.MetadataJson, ParseAssignmentMode(request.AssignmentMode), request.DefaultAssignmentLimit);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Ok(Map(entry));
    }

    private static object Map(FeatureCatalogEntry x) => new
    {
        x.Id,
        x.FeatureKey,
        x.Service,
        x.Category,
        x.DisplayName,
        x.Description,
        x.IsQuota,
        AssignmentMode = x.AssignmentMode.ToString(),
        x.DefaultAssignmentLimit,
        x.Unit,
        x.MetadataJson,
        x.CreatedAt,
        x.UpdatedAt
    };

    private static FeatureAssignmentMode ParseAssignmentMode(string? assignmentMode)
        => Enum.TryParse<FeatureAssignmentMode>(assignmentMode, true, out var parsed)
            ? parsed
            : FeatureAssignmentMode.TenantWide;
}
