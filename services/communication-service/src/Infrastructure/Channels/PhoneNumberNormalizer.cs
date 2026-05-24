using System.Text.RegularExpressions;

namespace CommunicationService.Infrastructure.Channels;

/// <summary>
/// Lightweight E.164 normalizer used before handing phone numbers to
/// Twilio. Twilio's API rejects unformatted numbers (error 21211) and
/// surfaces the original string in the error message — operators see
/// raw failures with no actionable fix.
///
/// Rules (in order):
///   1. Already E.164 (`+` + 8-15 digits) → return as-is
///   2. Leading `00` → replace with `+` (international access code)
///   3. WhatsApp-prefixed `whatsapp:+...` → preserve prefix, normalize body
///   4. Bare 10-digit US-shaped number when defaultCountry=="US" → prepend `+1`
///   5. Anything else falls back to the raw input so Twilio still surfaces
///      its real error (better than silently sending garbage)
///
/// Not a replacement for <c>libphonenumber-csharp</c> — country-specific
/// validity (e.g. UK mobile vs landline ranges) is out of scope. Good
/// enough for the common pasted-from-form / quick-add flows we see today.
/// </summary>
public static class PhoneNumberNormalizer
{
    private static readonly Regex E164 = new(@"^\+[1-9]\d{7,14}$", RegexOptions.Compiled);
    private static readonly Regex DigitsOnly = new(@"\D", RegexOptions.Compiled);

    public static string Normalize(string raw, string? defaultCountry = "US")
    {
        if (string.IsNullOrWhiteSpace(raw)) return raw;
        var trimmed = raw.Trim();

        // Preserve WhatsApp routing prefix and normalize the body.
        if (trimmed.StartsWith("whatsapp:", StringComparison.OrdinalIgnoreCase))
        {
            var body = trimmed[9..];
            var normalizedBody = Normalize(body, defaultCountry);
            return $"whatsapp:{normalizedBody}";
        }

        if (E164.IsMatch(trimmed)) return trimmed;

        // International access code → `+`
        if (trimmed.StartsWith("00"))
        {
            var digits = DigitsOnly.Replace(trimmed[2..], string.Empty);
            return digits.Length >= 8 ? $"+{digits}" : trimmed;
        }

        if (trimmed.StartsWith("+"))
        {
            // Has `+` but had separators; strip non-digits from the body.
            var digits = DigitsOnly.Replace(trimmed[1..], string.Empty);
            return digits.Length >= 8 ? $"+{digits}" : trimmed;
        }

        // No `+` — fall back on default country mapping for the common case.
        var bareDigits = DigitsOnly.Replace(trimmed, string.Empty);
        if (string.Equals(defaultCountry, "US", StringComparison.OrdinalIgnoreCase) && bareDigits.Length == 10)
        {
            return $"+1{bareDigits}";
        }
        if (string.Equals(defaultCountry, "US", StringComparison.OrdinalIgnoreCase) && bareDigits.Length == 11 && bareDigits.StartsWith("1"))
        {
            return $"+{bareDigits}";
        }

        return raw;
    }
}
