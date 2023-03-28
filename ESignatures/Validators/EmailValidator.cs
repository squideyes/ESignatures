using static ESignatures.Properties.Resources;
using static System.StringSplitOptions;

namespace ESignatures;

public static class EmailValidator
{
    // DATA
    // "https://raw.githubusercontent.com/disposable-email-domains/disposable-email-domains/master/disposable_email_blocklist.conf";
    // "https://data.iana.org/TLD/tlds-alpha-by-domain.txt";

    private static readonly HashSet<string> blockedDomains;
    private static readonly HashSet<string> topLevelDomains;

    static EmailValidator()
    {
        blockedDomains = Parse(BlockedDomains, "//");
        topLevelDomains = Parse(TopLevelDomains, "#");
    }

    public static bool IsEmailAddress(string value)
    {
        if (!IsWellFormed(value))
            return false;

        var domain = value.Split('@')[1];

        return !blockedDomains.Contains(
            domain, StringComparer.OrdinalIgnoreCase);
    }

    private static HashSet<string> Parse(
        string text, string commentPrefix)
    {
        var lines = text.Split(new[] { "\r\n", "\r", "\n" },
            RemoveEmptyEntries | TrimEntries).Where(
                l => !l.StartsWith(commentPrefix));

        return new HashSet<string>(lines);
    }

    private static bool IsWellFormed(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        var fields = value.Split('@');

        if (fields.Length != 2)
            return false;

        if (string.IsNullOrWhiteSpace(fields[0]))
            return false;

        if (string.IsNullOrWhiteSpace(fields[1]))
            return false;

        bool HasGoodParts(string field, int minParts, bool lastIsTld)
        {
            var parts = field.Split('.');

            if (parts.Length < minParts)
                return false;

            foreach (var part in parts.Take(
                parts.Length - (lastIsTld ? 1 : 0)))
            {
                if (part.Length == 0)
                    return false;

                if (!char.IsAsciiLetter(part[0]))
                    return false;

                for (int i = 1; i < part.Length - 1; i++)
                {
                    var c = part[i];

                    if (c != '-' && !char.IsLetterOrDigit(c))
                        return false;
                }

                if (!char.IsAsciiLetterOrDigit(part[^1]))
                    return false;
            }

            if (lastIsTld)
            {
                return topLevelDomains.Contains(parts[^1],
                    StringComparer.OrdinalIgnoreCase);
            }
            else
            {
                return true;
            }
        }

        if (!HasGoodParts(fields[0], 1, false))
            return false;

        if (!HasGoodParts(fields[1], 2, true))
            return false;

        return true;
    }
}