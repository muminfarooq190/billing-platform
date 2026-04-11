using BillingService.Api.Contracts;
using BillingService.Application.Abstractions;
using BillingService.Domain.Aggregates;
using BillingService.Domain.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace BillingService.Api.Controllers;

[ApiController]
[Route("billing/packages")]
public sealed class PackagesController(ICommercialPackageRepository commercialPackageRepository, IUnitOfWork unitOfWork) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        var packages = await commercialPackageRepository.ListAsync(cancellationToken);
        var features = packages.Count == 0
            ? []
            : await commercialPackageRepository.ListFeaturesByPackageIdsAsync(packages.Select(x => x.Id).ToArray(), cancellationToken);

        return Ok(packages.Select(x => Map(x, features.Where(f => f.CommercialPackageId == x.Id).ToList())));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var package = await commercialPackageRepository.GetByIdAsync(id, cancellationToken);
        if (package is null)
            return NotFound();

        var features = await commercialPackageRepository.ListFeaturesByPackageIdAsync(id, cancellationToken);
        return Ok(Map(package, features));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] UpsertCommercialPackageRequest request, CancellationToken cancellationToken)
    {
        var package = CommercialPackage.Create(request.Code, request.Name, request.Category, request.BillingModel, request.Description, request.IsActive, request.MetadataJson);
        await commercialPackageRepository.AddAsync(package, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = package.Id }, Map(package, []));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpsertCommercialPackageRequest request, CancellationToken cancellationToken)
    {
        var package = await commercialPackageRepository.GetByIdAsync(id, cancellationToken);
        if (package is null)
            return NotFound();

        package.Update(request.Code, request.Name, request.Category, request.BillingModel, request.Description, request.IsActive, request.MetadataJson);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        var features = await commercialPackageRepository.ListFeaturesByPackageIdAsync(id, cancellationToken);
        return Ok(Map(package, features));
    }

    [HttpPut("{id:guid}/features")]
    public async Task<IActionResult> ReplaceFeatures(Guid id, [FromBody] ReplaceCommercialPackageFeaturesRequest request, CancellationToken cancellationToken)
    {
        var package = await commercialPackageRepository.GetByIdAsync(id, cancellationToken);
        if (package is null)
            return NotFound();

        var existing = await commercialPackageRepository.ListFeaturesByPackageIdAsync(id, cancellationToken);
        if (existing.Count > 0)
            await commercialPackageRepository.RemoveFeaturesAsync(existing, cancellationToken);

        var features = request.Features.Select(x => CommercialPackageFeature.Create(id, x.FeatureKey, x.Granted, x.LimitValue, x.LimitMergePolicy, x.MetadataJson)).ToArray();
        if (features.Length > 0)
            await commercialPackageRepository.AddFeaturesAsync(features, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Ok(Map(package, features));
    }

    private static object Map(CommercialPackage package, IReadOnlyCollection<CommercialPackageFeature> features) => new
    {
        package.Id,
        package.Code,
        package.Name,
        package.Category,
        package.BillingModel,
        package.Description,
        package.IsActive,
        package.MetadataJson,
        package.CreatedAt,
        package.UpdatedAt,
        Features = features.Select(f => new
        {
            f.Id,
            f.FeatureKey,
            f.Granted,
            f.LimitValue,
            LimitMergePolicy = f.LimitMergePolicy.ToString(),
            f.MetadataJson,
            f.CreatedAt,
            f.UpdatedAt
        })
    };
}
