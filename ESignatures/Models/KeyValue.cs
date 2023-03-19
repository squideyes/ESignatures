namespace SquidEyes.ESignatures;

public readonly struct KeyValue
{
    private KeyValue(string key, string value)
    {
        Key = key;
        Value = value;
    }

    public string Key { get; }
    public string Value { get; }

    public override string ToString() => $"{Key}={Value}";

    public static KeyValue Create(string key, object value)
    {
        return new KeyValue(key, value.ToString()!);
    }
}
