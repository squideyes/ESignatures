using SquidEyes.Basics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SharedModels;

internal class JsonStringShortIdConverter : JsonConverter<ShortId>
{
    public override ShortId Read(ref Utf8JsonReader reader,
        Type typeToConvert, JsonSerializerOptions options)
    {
        return ShortId.From(reader.GetString()!);
    }

    public override void Write(Utf8JsonWriter writer,
        ShortId value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}
