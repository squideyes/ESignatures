using System.Text.RegularExpressions;

namespace SquidEyes.ESignatures;

public readonly partial struct TagValue
{
    private static readonly Regex tagValidator = GetTagValidator();

    private readonly object value;
    private readonly Type type;
    private readonly string typeCode;

    private TagValue(string tag, object value)
    {
        Tag = tag;
        this.value = value;

        type = value.GetType();
        typeCode = GetTypeCode();
    }

    public string Tag { get; }

    public bool GetBoolean() => GetValue<bool>();
    public DateOnly GetDateOnly() => GetValue<DateOnly>();
    public DateTime GetDateTime() => GetValue<DateTime>();
    public double GetDouble() => GetValue<double>();
    public T GetEnum<T>() => GetValue<T>();
    public float GetFloat() => GetValue<float>();
    public int GetInt32() => GetValue<int>();
    public long GetInt64() => GetValue<long>();
    public string GetString() => GetValue<string>();
    public TimeOnly GetTimeOnly() => GetValue<TimeOnly>();
    public TimeSpan GetTimeSpan() => GetValue<TimeSpan>();

    public ShortId GetShortId(int length = 8)
    {
        if (type != typeof(ShortId))
            ThrowInvalidCastError();

        var shortId = (ShortId)Convert.ChangeType(value, type);

        if (shortId.Length != length)
            ThrowInvalidCastError();

        return shortId;
    }

    public override string ToString() => $"{typeCode}&{Tag}&{value}";

    private T GetValue<T>()
    {
        if (typeof(T) != type)
            ThrowInvalidCastError();

        return (T)Convert.ChangeType(value, type);
    }

    private string GetTypeCode()
    {
        if (type.IsEnum)
            return type.Name;

        return type.FullName switch
        {
            "System.Boolean" => "Boolean",
            "System.DateOnly" => "DateOnly",
            "System.DateTime" => "DateTime",
            "System.Double" => "Double",
            "System.Int32" => "Int32",
            "System.Int64" => "Int64",
            "System.Single" => "Single",
            "System.String" => "String",
            "System.TimeOnly" => "TimeOnly",
            "System.TimeSpan" => "TimeSpan",
            "DemoCommon.ClientId" => "ClientId",
            _ => throw new ArgumentOutOfRangeException(nameof(type))
        };
    }

    private static void ThrowInvalidCastError()
    {
        throw new InvalidOperationException(
            "A value may only be cast to it's original type!");
    }

    private static bool IsValidType(object value)
    {
        var type = value.GetType();

        if (type.IsEnum)
            return true;

        return type.FullName switch
        {
            "System.Boolean" => true,
            "System.DateOnly" => true,
            "System.DateTime" => true,
            "System.Double" => true,
            "System.Int32" => true,
            "System.Int64" => true,
            "System.Single" => true,
            "System.String" => true,
            "System.TimeOnly" => true,
            "System.TimeSpan" => true,
            "DemoCommon.ClientId" => true,
            _ => false
        };
    }

    public static TagValue Create(string tag, string typeCode, string value)
    {
        return typeCode switch
        {
            "Boolean" => Create(tag, bool.Parse(value)),
            "DateOnly" => Create(tag, DateOnly.Parse(value)),
            "DateTime" => Create(tag, DateTime.Parse(value)),
            "Double" => Create(tag, double.Parse(value)),
            "Int32" => Create(tag, int.Parse(value)),
            "Int64" => Create(tag, long.Parse(value)),
            "Single" => Create(tag, float.Parse(value)),
            "String" => Create(tag, value),
            "TimeOnly" => Create(tag, TimeOnly.Parse(value)),
            "TimeSpan" => Create(tag, TimeSpan.Parse(value)),
            "ClientId" => Create(tag, value),
            _ => throw new ArgumentOutOfRangeException(nameof(value))
        };
    }

    public static TagValue Create<T>(string tag, T value)
    {
        if (!tagValidator.IsMatch(tag))
            throw new ArgumentOutOfRangeException(nameof(tag));

        if (!IsValidType(value!))
            throw new ArgumentOutOfRangeException(nameof(value));

        if (value is string text && text.Contains('&'))
            throw new ArgumentOutOfRangeException(nameof(value));

        return new TagValue(tag, value!);
    }

    [GeneratedRegex("^(?!-)(?!.*--)[a-z0-9-]{2,32}(?<!-)$")]
    private static partial Regex GetTagValidator();
}
