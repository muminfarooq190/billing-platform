using System.Text.RegularExpressions;
using IdentityService.Domain.Exceptions;

namespace IdentityService.Domain.ValueObjects;

public readonly partial record struct Email
{
    private static readonly Regex EmailRegex = EmailPattern();

    public string Value { get; }

    public Email(string value)
    {
        var normalized = value.Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(normalized) || !EmailRegex.IsMatch(normalized))
        {
            throw new DomainException("Invalid email format.");
        }

        Value = normalized;
    }

    public override string ToString() => Value;

    [GeneratedRegex("^[^\\s@]+@[^\\s@]+\\.[^\\s@]+$")]
    private static partial Regex EmailPattern();
}
