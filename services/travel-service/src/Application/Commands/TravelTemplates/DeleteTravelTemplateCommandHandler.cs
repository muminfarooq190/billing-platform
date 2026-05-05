using MediatR;
using TravelService.Application.Abstractions;
using TravelService.Domain.Aggregates;
using TravelService.Domain.Repositories;

namespace TravelService.Application.Commands.TravelTemplates;

public sealed class DeleteTravelTemplateCommandHandler(
    ITravelTemplateRepository templateRepository,
    ITenantActiveTemplateRepository activeTemplateRepository,
    IActivityWriter activityWriter,
    IAuditWriter auditWriter,
    IActorContext actorContext,
    IUnitOfWork unitOfWork) : IRequestHandler<DeleteTravelTemplateCommand>
{
    public async Task Handle(DeleteTravelTemplateCommand request, CancellationToken cancellationToken)
    {
        var template = await TravelTemplateCommandSupport.LoadTemplateAsync(templateRepository, request.TenantId, request.TemplateId, cancellationToken);
        template.Archive();
        await templateRepository.UpdateAsync(template, cancellationToken);

        var active = await activeTemplateRepository.GetAsync(request.TenantId, template.Context, cancellationToken);
        if (active is not null && active.TemplateId == template.Id)
        {
            active.SetTemplate(null);
            await activeTemplateRepository.UpsertAsync(active, cancellationToken);
        }

        await activityWriter.WriteAsync(
            ActivityEntry.Create(
                request.TenantId,
                "TravelTemplate",
                template.Id,
                "TravelTemplateDeleted",
                $"Travel template '{template.Name}' deleted",
                new { template.Context },
                actorContext.UserId),
            cancellationToken);
        await auditWriter.WriteAsync(
            AuditLog.Create(
                request.TenantId,
                "TravelTemplate",
                template.Id,
                "TravelTemplateDeleted",
                actorContext.UserId,
                actorContext.IpAddress,
                actorContext.UserAgent,
                null,
                new { DeletedAt = template.DeletedAt },
                null),
            cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
