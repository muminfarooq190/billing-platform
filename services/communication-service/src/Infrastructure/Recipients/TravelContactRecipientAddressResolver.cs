using System.Net.Http.Json;
using CommunicationService.Domain.Aggregates;
using CommunicationService.Domain.Enums;

namespace CommunicationService.Infrastructure.Recipients;

public sealed class TravelContactRecipientAddressResolver(HttpClient httpClient) : IRecipientAddressResolver
{
    public async Task<string?> ResolveAsync(Notification notification, RecipientPreferences? preferences, CancellationToken cancellationToken)
    {
        if (preferences is not null)
        {
            var fromPreferences = notification.Channel switch
            {
                ChannelType.Email => preferences.Email,
                ChannelType.Sms => preferences.Phone,
                ChannelType.WhatsApp => preferences.Phone,
                ChannelType.PushNotification => preferences.DeviceToken,
                _ => null
            };

            if (!string.IsNullOrWhiteSpace(fromPreferences))
                return fromPreferences;
        }

        if (notification.RecipientType != RecipientType.EndUser)
            return null;

        using var request = new HttpRequestMessage(HttpMethod.Get, $"travel/contacts/{notification.RecipientId:D}");
        request.Headers.Add("x-tenant-id", notification.TenantId.ToString());
        using var response = await httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
            return null;

        var contact = await response.Content.ReadFromJsonAsync<ContactReadModel>(cancellationToken: cancellationToken);
        if (contact is null)
            return null;

        return notification.Channel switch
        {
            ChannelType.Email => string.IsNullOrWhiteSpace(contact.Email) ? null : contact.Email,
            ChannelType.Sms => string.IsNullOrWhiteSpace(contact.Phone) ? null : contact.Phone,
            ChannelType.WhatsApp => string.IsNullOrWhiteSpace(contact.Phone) ? null : contact.Phone,
            _ => null
        };
    }

    private sealed record ContactReadModel(
        Guid Id,
        Guid TenantId,
        string FirstName,
        string LastName,
        string Email,
        string Phone,
        string? Company,
        string? Notes,
        IReadOnlyList<string>? Tags,
        DateTimeOffset CreatedAt,
        DateTimeOffset UpdatedAt);
}
