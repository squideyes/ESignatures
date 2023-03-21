using System.Collections;

namespace SquidEyes.ESignatures;

public class Metadata : IEnumerable<TagValue>
{
    private readonly Dictionary<string, TagValue> tagValues = new();

    public void Add(string key, object value) =>
        tagValues[key] = TagValue.Create(key, value);

    public int Count => tagValues.Count;

    public override string ToString() => string.Join("|", tagValues);

    public IEnumerator<TagValue> GetEnumerator() =>
        tagValues.Values.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
