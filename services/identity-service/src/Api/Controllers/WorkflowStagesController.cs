using IdentityService.Api.Contracts;
using IdentityService.Application.Abstractions;
using IdentityService.Domain.Aggregates;
using IdentityService.Infrastructure.Auth;
using IdentityService.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Api.Controllers;

[ApiController]
[Route("identity/tenant-settings/workflow-stages")]
public sealed class WorkflowStagesController(
    IdentityDbContext dbContext,
    IFeatureGate featureGate,
    ITenantContext tenantContext) : ControllerBase
{
    [HttpGet]
    [RequirePermission(Permissions.Identity.WorkflowRead)]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        var tenantId = ResolveTenantId();
        await featureGate.EnsureEnabledAsync(FeatureKeys.IdentitySettingsManage, tenantId, tenantContext.UserId, cancellationToken);
        var stages = await dbContext.WorkflowStages.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.DeletedAt == null)
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Label)
            .ToListAsync(cancellationToken);
        return Ok(stages.Select(MapResponse));
    }

    [HttpPost]
    [RequirePermission(Permissions.Identity.WorkflowManage)]
    public async Task<IActionResult> Create([FromBody] CreateWorkflowStageRequest request, CancellationToken cancellationToken)
    {
        if (request is null) return BadRequest("Body is required.");
        if (string.IsNullOrWhiteSpace(request.Key)) return BadRequest("Key is required.");
        if (string.IsNullOrWhiteSpace(request.Label)) return BadRequest("Label is required.");

        var tenantId = ResolveTenantId();
        await featureGate.EnsureEnabledAsync(FeatureKeys.IdentitySettingsManage, tenantId, tenantContext.UserId, cancellationToken);

        var existing = await dbContext.WorkflowStages
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Key == request.Key.Trim().ToLowerInvariant() && x.DeletedAt == null, cancellationToken);
        if (existing is not null) return Conflict(new { message = $"Stage with key '{request.Key}' already exists." });

        var sortOrder = request.SortOrder ?? await dbContext.WorkflowStages
            .Where(x => x.TenantId == tenantId && x.DeletedAt == null)
            .Select(x => (int?)x.SortOrder)
            .MaxAsync(cancellationToken) ?? -1;
        var nextOrder = request.SortOrder ?? sortOrder + 1;

        var stage = WorkflowStage.Create(
            tenantId,
            request.Key,
            request.Label,
            request.Color ?? "#041627",
            request.Icon ?? "view_kanban",
            nextOrder,
            request.Required ?? false,
            request.TemplateContext ?? string.Empty,
            request.AutomationType ?? "none",
            request.AutomationPayloadJson ?? "{}");

        dbContext.WorkflowStages.Add(stage);
        await dbContext.SaveChangesAsync(cancellationToken);
        return CreatedAtAction(nameof(GetById), new { stageId = stage.Id }, MapResponse(stage));
    }

    [HttpGet("{stageId:guid}")]
    [RequirePermission(Permissions.Identity.WorkflowRead)]
    public async Task<IActionResult> GetById(Guid stageId, CancellationToken cancellationToken)
    {
        var tenantId = ResolveTenantId();
        var stage = await dbContext.WorkflowStages.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == stageId && x.TenantId == tenantId && x.DeletedAt == null, cancellationToken);
        return stage is null ? NotFound() : Ok(MapResponse(stage));
    }

    [HttpPut("{stageId:guid}")]
    [RequirePermission(Permissions.Identity.WorkflowManage)]
    public async Task<IActionResult> Update(Guid stageId, [FromBody] UpdateWorkflowStageRequest request, CancellationToken cancellationToken)
    {
        if (request is null || string.IsNullOrWhiteSpace(request.Label)) return BadRequest("Label is required.");

        var tenantId = ResolveTenantId();
        await featureGate.EnsureEnabledAsync(FeatureKeys.IdentitySettingsManage, tenantId, tenantContext.UserId, cancellationToken);

        var stage = await dbContext.WorkflowStages
            .FirstOrDefaultAsync(x => x.Id == stageId && x.TenantId == tenantId && x.DeletedAt == null, cancellationToken);
        if (stage is null) return NotFound();

        stage.Update(
            request.Label,
            request.Color ?? stage.Color,
            request.Icon ?? stage.Icon,
            request.SortOrder ?? stage.SortOrder,
            request.Required ?? stage.Required,
            request.TemplateContext ?? stage.TemplateContext,
            request.AutomationType ?? stage.AutomationType,
            request.AutomationPayloadJson ?? stage.AutomationPayloadJson);

        await dbContext.SaveChangesAsync(cancellationToken);
        return Ok(MapResponse(stage));
    }

    [HttpDelete("{stageId:guid}")]
    [RequirePermission(Permissions.Identity.WorkflowManage)]
    public async Task<IActionResult> Delete(Guid stageId, CancellationToken cancellationToken)
    {
        var tenantId = ResolveTenantId();
        await featureGate.EnsureEnabledAsync(FeatureKeys.IdentitySettingsManage, tenantId, tenantContext.UserId, cancellationToken);

        var stage = await dbContext.WorkflowStages
            .FirstOrDefaultAsync(x => x.Id == stageId && x.TenantId == tenantId && x.DeletedAt == null, cancellationToken);
        if (stage is null) return NotFound();

        stage.SoftDelete();
        await dbContext.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpPut("bulk")]
    [RequirePermission(Permissions.Identity.WorkflowManage)]
    public async Task<IActionResult> BulkReplace([FromBody] List<CreateWorkflowStageRequest> requests, CancellationToken cancellationToken)
    {
        if (requests is null) return BadRequest("Body is required.");
        var tenantId = ResolveTenantId();
        await featureGate.EnsureEnabledAsync(FeatureKeys.IdentitySettingsManage, tenantId, tenantContext.UserId, cancellationToken);

        var existing = await dbContext.WorkflowStages
            .Where(x => x.TenantId == tenantId && x.DeletedAt == null)
            .ToListAsync(cancellationToken);

        foreach (var stage in existing)
        {
            stage.SoftDelete();
        }

        var inserted = new List<WorkflowStage>();
        for (var i = 0; i < requests.Count; i++)
        {
            var req = requests[i];
            if (string.IsNullOrWhiteSpace(req.Key) || string.IsNullOrWhiteSpace(req.Label)) continue;
            var stage = WorkflowStage.Create(
                tenantId,
                req.Key,
                req.Label,
                req.Color ?? "#041627",
                req.Icon ?? "view_kanban",
                req.SortOrder ?? i,
                req.Required ?? false,
                req.TemplateContext ?? string.Empty,
                req.AutomationType ?? "none",
                req.AutomationPayloadJson ?? "{}");
            dbContext.WorkflowStages.Add(stage);
            inserted.Add(stage);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return Ok(inserted.Select(MapResponse));
    }

    private static WorkflowStageResponse MapResponse(WorkflowStage s) => new(
        s.Id,
        s.TenantId,
        s.Key,
        s.Label,
        s.Color,
        s.Icon,
        s.SortOrder,
        s.Required,
        s.TemplateContext,
        s.AutomationType,
        s.AutomationPayloadJson,
        s.CreatedAt,
        s.UpdatedAt);

    private Guid ResolveTenantId()
    {
        var raw = User.Claims.FirstOrDefault(x => x.Type == "tenantId" || x.Type == "tenant_id")?.Value
            ?? Request.Headers["X-Tenant-Id"].FirstOrDefault()
            ?? Request.Headers["x-tenant-id"].FirstOrDefault();
        if (!Guid.TryParse(raw, out var tenantId)) throw new InvalidOperationException("Tenant context is missing.");
        return tenantId;
    }
}
