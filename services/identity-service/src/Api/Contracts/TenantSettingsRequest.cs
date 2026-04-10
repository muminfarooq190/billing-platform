namespace IdentityService.Api.Contracts;

public sealed record UpdateTenantSettingsRequest(
    string Timezone,
    string Locale,
    string DateFormat,
    string Currency,
    string NumberFormat,
    string DefaultCountry,
    string SettingsJson);
