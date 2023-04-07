// ********************************************************
// The use of this source code is licensed under the terms
// of the MIT License (https://opensource.org/licenses/MIT)
// ********************************************************

using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ESignatures;

public static class JsonExtenders
{
    private static readonly JsonSerializerOptions options;

    static JsonExtenders()
    {
        options = new JsonSerializerOptions()
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        options.Converters.Add(new JsonStringClientIdConverter());
        options.Converters.Add(new JsonStringEnumConverter());
        options.Converters.Add(new JsonStringShortIdConverter());
    }

    public static string ToJson(this object? value) =>
        JsonSerializer.Serialize(value, options);
}