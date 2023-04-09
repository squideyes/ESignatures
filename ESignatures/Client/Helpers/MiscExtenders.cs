// ********************************************************
// The use of this source code is licensed under the terms
// of the MIT License (https://opensource.org/licenses/MIT)
// ********************************************************

using System.Text.RegularExpressions;

namespace SquidEyes.ESignatures.Client;

internal static partial class MiscExtenders
{
    private static readonly Regex apiKeyValidator = GetApiKeyValidator();

    public static bool IsApiKey(this string value) =>
        !string.IsNullOrWhiteSpace(value) && apiKeyValidator.IsMatch(value);

    public static string ToCode(this Locale value)
    {
        return value switch
        {
            Locale.EN => "en",
            _ => value.ToString().ToLower()
        };
    }

    [GeneratedRegex("^(?!-)(?!.*--)[a-z0-9-]{2,32}(?<!-)$")]
    private static partial Regex GetApiKeyValidator();
}