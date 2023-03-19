using System.Collections;

namespace WebHookDemo;

public class Metadata
{
    public required string ClientId { get; init; }
    public required string ContractKind { get; init; }

    public static Metadata Parse(string value)
    {
        if (value is null)
            throw new ArgumentOutOfRangeException(nameof(value));

        var keyValues = value.Split('|').Select(v => v.Split('=')
            .Get(v => new KeyValuePair<string, string>(v[0], v[1])));

        var dict = new Dictionary<string, string>(keyValues);

        return new Metadata()
        {
            ClientId = dict["ClientId"],
            ContractKind = dict["ContractKind"]
        };
    }
}
