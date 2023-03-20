namespace WebHookDemo;

public readonly struct ClientId : IEquatable<ClientId>
{
    private static readonly char[] charSet =
        "ABCDEFGHJKLMNPQRSTUVWXYZ23456789".ToCharArray();

    private ClientId(string value)
    {
        Value = value;
    }

    private string Value { get; }

    public override string ToString() => AsString();

    public string AsString() => Value;

    public bool Equals(ClientId other) => Value == other.Value;

    public override bool Equals(object? other) =>
        other is ClientId clientId && Equals(clientId);

    public override int GetHashCode() => Value.GetHashCode();

    public static bool IsValue(string value)
    {
        if (value is null)
            return false;

        if (value.Length != 8)
            return false;

        return value.All(c => charSet.Contains(c));
    }

    public static ClientId From(string value)
    {
        if (!IsValue(value))
            throw new ArgumentOutOfRangeException(nameof(value));

        return new ClientId(value);
    }

    public static bool operator ==(ClientId left, ClientId right) =>
        left.Equals(right);

    public static bool operator !=(ClientId left, ClientId right) =>
        !(left == right);
}
