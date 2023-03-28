using System.Text.Json.Serialization;

namespace ESignatures.Internal;

internal class Placeholder
{
    [JsonPropertyName("api_key")]
    public required string ApiKey { get; init; }

    [JsonPropertyName("value")]
    public required string Value { get; init; }
}
