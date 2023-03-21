using DemoCommon;

namespace WebHookDemo;

public class Metadata
{
    public required ShortId ClientId { get; init; }
    public required ContractKind ContractKind { get; init; }

    public static Metadata Parse(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentOutOfRangeException(nameof(value));

        foreach (var chunk in value.Split('|'))
        {
            var fields = chunk.Split('&');

            //var tagValue = TagValue.Create
            var typeCode = fields[0];
            var tag = fields[1];
            var v = fields[2];
        }

        //var dict = new Dictionary<string, string>(keyValues);

        return null;

        //return new Metadata()
        //{
        //    ClientId = ClientId.From(dict["ClientId"]),
        //    ContractKind = Enum.Parse<ContractKind>(dict["ContractKind"], true)
        //};
    }
}
