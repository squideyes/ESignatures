using System.Text;
using System.Text.RegularExpressions;

namespace SquidEyes.ESignatures;

internal static partial class InternalExtenders
{
    private static readonly Regex metadataKeyValidator = GetMetadataKeyValidator();

    private static readonly Regex apiKeyValidator = GetApiKeyValidator();

    public static bool IsMetadataKey(this string value) =>
        !string.IsNullOrWhiteSpace(value) && metadataKeyValidator.IsMatch(value);


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

    [GeneratedRegex("^[A-Z][A-Za-z0-9]{0,23}$")]
    private static partial Regex GetMetadataKeyValidator();

    [GeneratedRegex("^(?!-)(?!.*--)[a-z0-9-]{2,32}(?<!-)$")]
    private static partial Regex GetApiKeyValidator();
}