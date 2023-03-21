using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace DemoCommon;

public readonly struct ClientId : IEquatable<ClientId>, IParsable<ClientId>
{
    private static readonly char[] charSet =
        "ABCDEFGHJKLMNPQRSTUVWXYZ23456789".ToCharArray();

    private static readonly Random random = new(9876);

    private ClientId(string value)
    {
        Value = value;
    }

    private string Value { get; }

    public override string ToString() => Value;

    public bool Equals(ClientId other) => Value == other.Value;

    public override bool Equals(object? other) =>
        other is ClientId clientId && Equals(clientId);

    public override int GetHashCode() => Value.GetHashCode();

    public static ClientId Next()
    {
        var sb = new StringBuilder(8);

        for (var i = 0; i < 8; i++)
            sb.Append(charSet[random.Next(8)]);

        return From(sb.ToString());
    }

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

    public static ClientId Parse(string s, IFormatProvider? provider) =>
        From(s);

    public static bool TryParse([NotNullWhen(true)] string? s,
        IFormatProvider? provider,
        [MaybeNullWhen(false)] out ClientId result)
    {
        return Safe.TryGetValue(() => Parse(s!, null), out result);
    }

    public static bool operator ==(ClientId left, ClientId right) =>
        left.Equals(right);

    public static bool operator !=(ClientId left, ClientId right) =>
        !(left == right);
}
