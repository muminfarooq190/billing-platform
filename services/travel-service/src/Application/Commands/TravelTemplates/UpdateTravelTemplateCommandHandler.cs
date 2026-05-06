using MediatR;
using TravelService.Application.Abstractions;
using TravelService.Domain.Aggregates;
using TravelService.Domain.Repositories;

namespace TravelService.Application.Commands.TravelTemplates;

public sealed class UpdateTravelTemplateCommandHandler(
    ITravelTemplateRepository templateRepository,
    IActivityWriter activityWriter,
    IAuditWriter auditWriter,
    IActorContext actorContext,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateTravelTemplateCommand>
{
    public async Task Handle(UpdateTravelTemplateCommand request, CancellationToken cancellationToken)
    {
        var template = await TravelTemplateCommandSupport.LoadTemplateAsync(templateRepository, request.TenantId, request.TemplateId, cancellationToken);
        template.Update(
            request.Name,
            request.Description,
            request.Category,
            request.Banner,
            request.AccentColor,
            request.Tagline,
            TravelTemplateCommandSupport.SerializeSections(request.Sections),
            TravelTemplateCommandSupport.SerializeSeed(request.Seed));

        await templateRepository.UpdateAsync(template, cancellationToken);
        await activityWriter.WriteAsync(
            ActivityEntry.Create(
                request.TenantId,
                "TravelTemplate",
                template.Id,
                "TravelTemplateUpdated",
                $"Travel template '{template.Name}' updated",
                new { template.Context },
                actorContext.UserId),
            cancellationToken);
        await auditWriter.WriteAsync(
            AuditLog.Create(
                request.TenantId,
                "TravelTemplate",
                template.Id,
                "TravelTemplateUpdated",
                actorContext.UserId,
                actorContext.IpAddress,
                actorContext.UserAgent,
                null,
                new { template.Name, Context = template.Context.ToString() },
                null),
            cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
