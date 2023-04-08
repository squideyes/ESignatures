// ********************************************************
// The use of this source code is licensed under the terms
// of the MIT License (https://opensource.org/licenses/MIT)
// ********************************************************

using System.Text;
using System.Text.RegularExpressions;

namespace ESignatures.Client;

internal static partial class MiscExtenders
{
    private static readonly Regex apiKeyValidator = GetApiKeyValidator();

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


    public static void AppendDelimited(
        this StringBuilder sb, string value, string delimiter)
    {
        if (sb.Length > 0)
            sb.Append(delimiter);

        sb.Append(value);
    }

    [GeneratedRegex("^(?!-)(?!.*--)[a-z0-9-]{2,32}(?<!-)$")]
    private static partial Regex GetApiKeyValidator();
}