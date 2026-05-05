using MediatR;
using TravelService.Application.Abstractions;
using TravelService.Domain.Aggregates;
using TravelService.Domain.Repositories;

namespace TravelService.Application.Commands.TravelTemplates;

public sealed class CreateTravelTemplateCommandHandler(
    ITravelTemplateRepository templateRepository,
    IActivityWriter activityWriter,
    IAuditWriter auditWriter,
    IActorContext actorContext,
    IUnitOfWork unitOfWork) : IRequestHandler<CreateTravelTemplateCommand, Guid>
{
    public async Task<Guid> Handle(CreateTravelTemplateCommand request, CancellationToken cancellationToken)
    {
        var context = TravelService.Application.Queries.TravelTemplates.TravelTemplateQuerySupport.ParseContext(request.Context);
        var template = TravelTemplate.Create(
            request.TenantId,
            context,
            request.Name,
            request.Description,
            request.Category,
            request.Banner,
            request.AccentColor,
            request.Tagline,
            TravelTemplateCommandSupport.SerializeSections(request.Sections),
            TravelTemplateCommandSupport.SerializeSeed(request.Seed),
            false,
            request.CreatedByUserId);

        await templateRepository.AddAsync(template, cancellationToken);
        await activityWriter.WriteAsync(
            ActivityEntry.Create(
                request.TenantId,
                "TravelTemplate",
                template.Id,
                "TravelTemplateCreated",
                $"Travel template '{template.Name}' created",
                new { template.Context },
                actorContext.UserId),
            cancellationToken);
        await auditWriter.WriteAsync(
            AuditLog.Create(
                request.TenantId,
                "TravelTemplate",
                template.Id,
                "TravelTemplateCreated",
                actorContext.UserId,
                actorContext.IpAddress,
                actorContext.UserAgent,
                null,
                new { template.Name, Context = template.Context.ToString(), template.IsBuiltIn },
                null),
            cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return template.Id;
    }
}
