// ********************************************************
// The use of this source code is licensed under the terms
// of the MIT License (https://opensource.org/licenses/MIT)
// ********************************************************

using SquidEyes.Fundamentals;

namespace SquidEyes.ESignatures.Client;

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