using System.Text;
using System.Text.RegularExpressions;

namespace SquidEyes.ESignatures;

internal static partial class MiscExtenders
{

    private static readonly Regex tokenValidator = GetTokenValidator();
    private static readonly Regex apiKeyValidator = GetApiKeyValidator();

    public static bool IsToken(this string value) =>
        !string.IsNullOrWhiteSpace(value) && tokenValidator.IsMatch(value);

    public static bool IsEmptyOrTrimmed(this string value) =>
        value is not null && (value == "" || value.IsTrimmed());

    public static bool IsNonEmptyAndTrimmed(this string value) =>
        !string.IsNullOrEmpty(value) && value.IsTrimmed();

    private static readonly PhoneNumbers.PhoneNumberUtil pnu =
        PhoneNumbers.PhoneNumberUtil.GetInstance();

    public static bool IsApiKey(this string value) =>
        !string.IsNullOrWhiteSpace(value) && apiKeyValidator.IsMatch(value);

    public static string ToCode(this Locale value)
    {
        return value switch
        {
            Locale.ENGB => "enGB",
            _ => value.ToString().ToLower()
        };
    }

    public static bool IsPhoneNumber(this string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        if (!value.StartsWith("+") && value.Length > 2)
            return false;

        try
        {
            return pnu.IsValidNumber(pnu.Parse(value, "US"));
        }
        catch
        {
            return false;
        }
    }

    public static string ToPlusAndDigits(this string value) =>
        "+" + string.Join("", value.Skip(1).Where(char.IsDigit));

    public static void AppendDelimited(
        this StringBuilder sb, string value, string delimiter)
    {
        if (sb.Length > 0)
            sb.Append(delimiter);

        sb.Append(value);
    }
    private static bool IsTrimmed(this string value) =>
        !char.IsWhiteSpace(value[0]) && !char.IsWhiteSpace(value[^1]);

    [GeneratedRegex("^[A-Z][A-Za-z0-9]{0,23}$")]
    private static partial Regex GetTokenValidator();

    [GeneratedRegex("^(?!-)(?!.*--)[a-z0-9-]{2,32}(?<!-)$")]
    private static partial Regex GetApiKeyValidator();
}