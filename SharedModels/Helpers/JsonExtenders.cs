using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SharedModels;

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
