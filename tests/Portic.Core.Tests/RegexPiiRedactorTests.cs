using Portic.Core.Governance;
using Xunit;

namespace Portic.Core.Tests;

public sealed class RegexPiiRedactorTests
{
    private readonly RegexPiiRedactor redactor = new();

    [Fact]
    public void Redacts_email_addresses()
    {
        var result = redactor.Redact("Contact me at jane.doe@example.com for details.");

        Assert.DoesNotContain("jane.doe@example.com", result);
        Assert.Contains("[redacted:email]", result);
    }

    [Fact]
    public void Redacts_phone_numbers()
    {
        var result = redactor.Redact("Call +1 415-555-0199 tomorrow.");

        Assert.DoesNotContain("415-555-0199", result);
        Assert.Contains("[redacted:phone]", result);
    }

    [Fact]
    public void Redacts_credit_card_like_digit_runs()
    {
        var result = redactor.Redact("Card number 4111 1111 1111 1111 was declined.");

        Assert.DoesNotContain("4111 1111 1111 1111", result);
        Assert.Contains("[redacted:card]", result);
    }

    [Fact]
    public void Leaves_ordinary_text_unchanged()
    {
        const string text = "Summarize the quarterly report in three bullet points.";

        Assert.Equal(text, redactor.Redact(text));
    }
}
