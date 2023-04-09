// ********************************************************
// The use of this source code is licensed under the terms
// of the MIT License (https://opensource.org/licenses/MIT)
// ********************************************************

using System.Text.Json.Serialization;

namespace SquidEyes.ESignatures.Client;

internal class PlaceholderPoco
{
    [JsonPropertyName("api_key")]
    public required string ApiKey { get; init; }

    [JsonPropertyName("value")]
    public required string Value { get; init; }
}