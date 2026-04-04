using CommunicationService.Application.Abstractions;
using CommunicationService.Domain.Aggregates;
using CommunicationService.Domain.Enums;
using CommunicationService.Domain.Repositories;
using CommunicationService.Domain.ValueObjects;
using MediatR;

namespace CommunicationService.Application.Commands.UpdateRecipientPreferences;

public sealed class UpdateRecipientPreferencesCommandHandler(IRecipientPreferencesRepository preferencesRepository, IUnitOfWork unitOfWork) : IRequestHandler<UpdateRecipientPreferencesCommand, Guid>
{
    public async Task<Guid> Handle(UpdateRecipientPreferencesCommand request, CancellationToken cancellationToken)
    {
        var recipientType = Enum.Parse<RecipientType>(request.RecipientType, true);
        var existing = await preferencesRepository.GetByRecipientIdAsync(request.RecipientId, request.TenantId, cancellationToken);

        if (existing is null)
        {
            existing = RecipientPreferences.Create(request.TenantId, request.RecipientId, recipientType, request.Email, request.Phone, request.DeviceToken);
            await preferencesRepository.AddAsync(existing, cancellationToken);
        }
        else
        {
            existing.UpdateContactInfo(request.Email, request.Phone, request.DeviceToken);
        }

        if (!string.IsNullOrWhiteSpace(request.Timezone))
            existing.SetTimezone(request.Timezone);

        if (request.ChannelPreferences is { Count: > 0 })
        {
            var prefs = request.ChannelPreferences.Select(p => new ChannelPreference(
                Enum.Parse<ChannelType>(p.Channel, true),
                p.Enabled,
                p.QuietHoursEnabled,
                string.IsNullOrWhiteSpace(p.QuietStart) ? null : TimeOnly.Parse(p.QuietStart),
                string.IsNullOrWhiteSpace(p.QuietEnd) ? null : TimeOnly.Parse(p.QuietEnd)
            )).ToList();
            existing.SetChannelPreferences(prefs);
        }

        await preferencesRepository.UpdateAsync(existing, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return existing.Id;
    }
}
