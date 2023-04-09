// ********************************************************
// The use of this source code is licensed under the terms
// of the MIT License (https://opensource.org/licenses/MIT)
// ********************************************************

//using SquidEyes.Fundamentals;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace SquidEyes.ESignatures.Json;

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

        //options.Converters.Add(new JsonStringClientIdConverter());
        options.Converters.Add(new JsonStringEnumConverter());
        //options.Converters.Add(new JsonStringShortIdConverter());
    }

    public static string ToJson<T>(this T value) =>
        JsonSerializer.Serialize(value, options);

    internal static JsonNode? GetDataIfStatusIs(this JsonNode? node, string status)
    {
        var actual = node.GetString("status");

        if (actual != status)
        {
            throw new InvalidDataException(
                $"\"{actual}\" JSON received when \"{status}\" was expected!");
        }

        return node!["data"];
    }

    internal static string GetString(this JsonNode? node, string propertyName) =>
        (string)node![propertyName]!;
}