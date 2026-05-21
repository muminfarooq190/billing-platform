using MediatR;
using TravelService.Application.Abstractions;
using TravelService.Domain.Aggregates;
using TravelService.Domain.Repositories;

namespace TravelService.Application.Commands.TravelTemplates;

public sealed class SetActiveTravelTemplateCommandHandler(
    ITravelTemplateRepository templateRepository,
    ITenantActiveTemplateRepository activeTemplateRepository,
    IActivityWriter activityWriter,
    IAuditWriter auditWriter,
    IActorContext actorContext,
    IUnitOfWork unitOfWork) : IRequestHandler<SetActiveTravelTemplateCommand>
{
    public async Task Handle(SetActiveTravelTemplateCommand request, CancellationToken cancellationToken)
    {
        var context = TravelService.Application.Queries.TravelTemplates.TravelTemplateQuerySupport.ParseContext(request.Context);

        if (request.TemplateId.HasValue)
        {
            var template = await TravelTemplateCommandSupport.LoadTemplateAsync(templateRepository, request.TenantId, request.TemplateId.Value, cancellationToken);
            if (template.Context != context)
                throw new ArgumentException("Template context does not match the requested active context.", nameof(request.TemplateId));
        }

        var active = await activeTemplateRepository.GetAsync(request.TenantId, context, cancellationToken)
                     ?? TenantActiveTemplate.Create(request.TenantId, context, request.TemplateId);
        active.SetTemplate(request.TemplateId);
        await activeTemplateRepository.UpsertAsync(active, cancellationToken);

        // Activity/audit rows have a NOT-NULL entity_id and the aggregate rejects
        // Guid.Empty ("Entity id is required."). When the user is *clearing*
        // the active template the request.TemplateId is null, so we fall back
        // to the tenant id as the audit-trail anchor — there's no other natural
        // entity to point at (TenantActiveTemplate uses a composite PK with no
        // single id of its own).
        var auditEntityId = request.TemplateId ?? request.TenantId;
        var auditSummary = request.TemplateId.HasValue
            ? $"Active {context} template updated"
            : $"Active {context} template cleared";

        await activityWriter.WriteAsync(
            ActivityEntry.Create(
                request.TenantId,
                "TravelTemplate",
                auditEntityId,
                "TravelTemplateSetActive",
                auditSummary,
                new { Context = context.ToString(), request.TemplateId },
                actorContext.UserId),
            cancellationToken);
        await auditWriter.WriteAsync(
            AuditLog.Create(
                request.TenantId,
                "TenantActiveTemplate",
                auditEntityId,
                "TravelTemplateSetActive",
                actorContext.UserId,
                actorContext.IpAddress,
                actorContext.UserAgent,
                null,
                new { Context = context.ToString(), request.TemplateId },
                null),
            cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
