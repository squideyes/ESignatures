using System.Text;

namespace SquidEyes.ESignatures;

public class Metadata
{
    private readonly Dictionary<string, object> keyValues = new();

    public void Add(string key, object value)
    {
        if (!key.IsMetadataKey())
            throw new ArgumentOutOfRangeException(nameof(key));

        if (value == null)
            throw new ArgumentNullException(nameof(value));

        keyValues[key] = value.ToString()!;
    }

    public int Count => keyValues.Count;

    public override string ToString()
    {
        var sb = new StringBuilder();

        foreach (var kv in keyValues)
        {
            if (sb.Length > 0)
                sb.Append("|");

            sb.Append(kv.Key);
            sb.Append('=');
            sb.Append(kv.Value);
        }

        return sb.ToString();
    }
}
