using System.Text.Json.Serialization;

namespace ESignatures;

internal class BrandingPoco
{
    [JsonPropertyName("company_name")]
    public required string Company { get; init; }

    [JsonPropertyName("logo_url")]
    public required Uri LogoUri { get; init; }
}
