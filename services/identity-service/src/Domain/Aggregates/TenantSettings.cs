using IdentityService.Domain.Common;
using IdentityService.Domain.Exceptions;

namespace IdentityService.Domain.Aggregates;

public sealed class TenantSettings : AggregateRoot
{
    private TenantSettings() { }

    private TenantSettings(Guid tenantId, string timezone, string locale, string dateFormat, string currency, string numberFormat, string defaultCountry, string settingsJson)
    {
        TenantId = tenantId;
        Timezone = timezone;
        Locale = locale;
        DateFormat = dateFormat;
        Currency = currency;
        NumberFormat = numberFormat;
        DefaultCountry = defaultCountry;
        SettingsJson = settingsJson;
        CreatedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public Guid TenantId { get; private set; }
    public string Timezone { get; private set; } = "UTC";
    public string Locale { get; private set; } = "en";
    public string DateFormat { get; private set; } = "yyyy-MM-dd";
    public string Currency { get; private set; } = "USD";
    public string NumberFormat { get; private set; } = "en-US";
    public string DefaultCountry { get; private set; } = string.Empty;
    public string SettingsJson { get; private set; } = "{}";
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public static TenantSettings Create(Guid tenantId, string timezone, string locale, string dateFormat, string currency, string numberFormat, string defaultCountry, string settingsJson)
        => new(tenantId, NormalizeOrDefault(timezone, "UTC"), NormalizeOrDefault(locale, "en"), NormalizeOrDefault(dateFormat, "yyyy-MM-dd"), NormalizeOrDefault(currency, "USD"), NormalizeOrDefault(numberFormat, "en-US"), defaultCountry?.Trim() ?? string.Empty, string.IsNullOrWhiteSpace(settingsJson) ? "{}" : settingsJson);

    public void Update(string timezone, string locale, string dateFormat, string currency, string numberFormat, string defaultCountry, string settingsJson)
    {
        Timezone = NormalizeOrDefault(timezone, "UTC");
        Locale = NormalizeOrDefault(locale, "en");
        DateFormat = NormalizeOrDefault(dateFormat, "yyyy-MM-dd");
        Currency = NormalizeOrDefault(currency, "USD");
        NumberFormat = NormalizeOrDefault(numberFormat, "en-US");
        DefaultCountry = defaultCountry?.Trim() ?? string.Empty;
        SettingsJson = string.IsNullOrWhiteSpace(settingsJson) ? "{}" : settingsJson;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    private static string NormalizeOrDefault(string? value, string fallback)
    {
        var normalized = value?.Trim();
        if (string.IsNullOrWhiteSpace(normalized)) return fallback;
        return normalized;
    }
}
