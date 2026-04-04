using CommunicationService.Application.Abstractions;
using CommunicationService.Domain.Exceptions;
using CommunicationService.Domain.Repositories;
using MediatR;

namespace CommunicationService.Application.Commands.UpdateTemplate;

public sealed class UpdateTemplateCommandHandler(INotificationTemplateRepository templateRepository, IUnitOfWork unitOfWork) : IRequestHandler<UpdateTemplateCommand>
{
    public async Task Handle(UpdateTemplateCommand request, CancellationToken cancellationToken)
    {
        var template = await templateRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new DomainException($"Template {request.Id} not found.");

        switch (request.Action?.ToLowerInvariant())
        {
            case "activate": template.Activate(); break;
            case "archive": template.Archive(); break;
            default:
                template.Update(request.Name, request.Subject, request.BodyTemplate, request.Description);
                break;
        }

        await templateRepository.UpdateAsync(template, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
