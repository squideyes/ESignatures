using System.Text;

namespace SquidEyes.ESignatures;

public readonly struct ShortId : IEquatable<ShortId>
{
    private static readonly char[] charSet =
        "ABCDEFGHJKLMNPQRSTUVWXYZ23456789".ToCharArray();

    private static readonly Random random = new(9876);

    private ShortId(string value)
    {
        Value = value;
    }

    private string Value { get; }

    public int Length => Value.Length;

    public override string ToString() => Value;

    public bool Equals(ShortId other) => Value == other.Value;

    public override bool Equals(object? other) =>
        other is ShortId clientId && Equals(clientId);

    public override int GetHashCode() => Value.GetHashCode();

    public static ShortId Next(int length = 8)
    {
        AssertLengthIsValid(length);

        var sb = new StringBuilder(length);

        for (var i = 0; i < length; i++)
            sb.Append(charSet[random.Next(charSet.Length)]);

        return From(sb.ToString());
    }

    public static bool IsValue(string value, int length = 8)
    {
        AssertLengthIsValid(length);

        if (value is null)
            return false;

        if (value.Length != length)
            return false;

        return value.All(c => charSet.Contains(c));
    }

    public static ShortId From(string value)
    {
        if (!IsValue(value))
            throw new ArgumentOutOfRangeException(nameof(value));

        return new ShortId(value);
    }

    private static void AssertLengthIsValid(int length)
    {
        if (length < 4 || length > 12)
            throw new ArgumentOutOfRangeException(nameof(length));
    }

    public static bool operator ==(ShortId left, ShortId right) =>
        left.Equals(right);

    public static bool operator !=(ShortId left, ShortId right) =>
        !(left == right);
}
