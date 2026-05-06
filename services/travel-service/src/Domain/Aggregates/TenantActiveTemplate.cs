using TravelService.Domain.Enums;
using TravelService.Domain.Exceptions;

namespace TravelService.Domain.Aggregates;

public sealed class TenantActiveTemplate
{
    private TenantActiveTemplate() { }

    private TenantActiveTemplate(Guid tenantId, TravelTemplateContext context, Guid? templateId)
    {
        if (tenantId == Guid.Empty)
            throw new DomainException("TenantId is required.");

        TenantId = tenantId;
        Context = context;
        TemplateId = templateId;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public Guid TenantId { get; private set; }
    public TravelTemplateContext Context { get; private set; }
    public Guid? TemplateId { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public static TenantActiveTemplate Create(Guid tenantId, TravelTemplateContext context, Guid? templateId)
        => new(tenantId, context, templateId);

    public void SetTemplate(Guid? templateId)
    {
        TemplateId = templateId;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
