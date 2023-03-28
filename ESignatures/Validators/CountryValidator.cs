namespace ESignatures;

public static class CountryValidator
{
    private static readonly HashSet<string> countryCodes = new(
        ISO3166.Country.List.Select(c => c.TwoLetterCode));

    public static bool IsCountryCode(string value) =>
        value is not null && countryCodes.Contains(value);
}
