using System.Text.RegularExpressions;

namespace SquidEyes.ESignatures;

public readonly partial struct TagValue
{
    private static readonly Regex tagValidator = GetTagValidator();

    private readonly Type type;
    private readonly object value;

    private TagValue(string tag, object value)
    {
        Tag = tag;
        type = value.GetType();
        this.value = value;
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

    private T GetValue<T>()
    {
        if (typeof(T) != type)
        {
            throw new InvalidOperationException(
                "A value may only be cast to it's original type!");
        }

        return (T)Convert.ChangeType(value, type);
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
            _ => false
        };
    }

    public static TagValue Create<T>(string tag, T value)
    {
        if (!tagValidator.IsMatch(tag))
            throw new ArgumentOutOfRangeException(nameof(tag));

        if (!IsValidType(value!))
            throw new ArgumentOutOfRangeException(nameof(value));

        return new TagValue(tag, value!);
    }

    [GeneratedRegex("^(?!-)(?!.*--)[a-z0-9-]{2,32}(?<!-)$")]
    private static partial Regex GetTagValidator();
}
