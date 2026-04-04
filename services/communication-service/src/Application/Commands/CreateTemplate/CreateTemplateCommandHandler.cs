using CommunicationService.Application.Abstractions;
using CommunicationService.Domain.Aggregates;
using CommunicationService.Domain.Enums;
using CommunicationService.Domain.Repositories;
using MediatR;

namespace CommunicationService.Application.Commands.CreateTemplate;

public sealed class CreateTemplateCommandHandler(INotificationTemplateRepository templateRepository, IUnitOfWork unitOfWork) : IRequestHandler<CreateTemplateCommand, Guid>
{
    public async Task<Guid> Handle(CreateTemplateCommand request, CancellationToken cancellationToken)
    {
        var template = NotificationTemplate.Create(
            request.TenantId,
            request.Name,
            request.Subject,
            request.BodyTemplate,
            Enum.Parse<ChannelType>(request.Channel, true),
            request.Description);

        await templateRepository.AddAsync(template, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return template.Id;
    }
}
