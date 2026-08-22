using System.Text.RegularExpressions;

namespace Portic.Core.Governance;

/// <summary>
/// Default, pattern-based <see cref="IContentRedactor"/>: catches the common, high-confidence PII
/// shapes (email addresses, phone numbers, credit-card-like digit runs). It is deliberately
/// conservative -- false positives (over-redacting) are the safe failure mode here, false negatives
/// are not.
/// </summary>
public sealed partial class RegexPiiRedactor : IContentRedactor
{
    public string Redact(string content)
    {
        ArgumentNullException.ThrowIfNull(content);

        var redacted = EmailPattern().Replace(content, "[redacted:email]");
        redacted = CreditCardPattern().Replace(redacted, "[redacted:card]");
        redacted = PhonePattern().Replace(redacted, "[redacted:phone]");
        return redacted;
    }

    [GeneratedRegex(@"[a-zA-Z0-9.!#$%&'*+/=?^_`{|}~-]+@[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?(?:\.[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?)+")]
    private static partial Regex EmailPattern();

    // 13-19 digits, optionally separated by spaces/dashes into groups -- covers common card lengths.
    [GeneratedRegex(@"\b(?:\d[ -]?){13,19}\b")]
    private static partial Regex CreditCardPattern();

    // A run of 7+ digits with optional separators, loosely covering international phone formats.
    [GeneratedRegex(@"\+?\d[\d\s().-]{6,}\d")]
    private static partial Regex PhonePattern();
}
