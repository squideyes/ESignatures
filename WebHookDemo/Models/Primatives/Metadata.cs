using DemoCommon;

namespace WebHookDemo;

public class Metadata
{
    public required ClientId ClientId { get; init; }
    public required ContractKind ContractKind { get; init; }

    public static Metadata Parse(string value)
    {
        if (value is null)
            throw new ArgumentOutOfRangeException(nameof(value));

        var keyValues = value.Split('|').Select(v => v.Split('=')
            .Get(v => new KeyValuePair<string, string>(v[0], v[1])));

        var dict = new Dictionary<string, string>(keyValues);

        return new Metadata()
        {
            ClientId = ClientId.From(dict["ClientId"]),
            ContractKind = Enum.Parse<ContractKind>(dict["ContractKind"], true)
        };
    }
}
