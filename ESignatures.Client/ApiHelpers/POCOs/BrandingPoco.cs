// ********************************************************
// The use of this source code is licensed under the terms
// of the MIT License (https://opensource.org/licenses/MIT)
// ********************************************************

using System.Text.Json.Serialization;

namespace ESignatures.Client;

internal class BrandingPoco
{
    [JsonPropertyName("company_name")]
    public string? Company { get; init; }

    [JsonPropertyName("logo_url")]
    public Uri? LogoUri { get; init; }
}