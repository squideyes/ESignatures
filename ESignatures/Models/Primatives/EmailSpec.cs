using SquidEyes.Fundamentals;

namespace ESignatures;

public class EmailSpec
{
    public EmailSpec(string subject, string bodyText)
    {
        Subject = subject.Must().Be(v => v.IsNonEmptyAndTrimmed());
        BodyText = bodyText.MayNot().BeNullOrWhitespace();
    }

    public string Subject { get; }
    public string BodyText { get; }
}
