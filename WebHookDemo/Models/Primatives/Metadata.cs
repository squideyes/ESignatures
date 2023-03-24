using SharedModels;
using SquidEyes.Basics;

namespace WebHookDemo;

public class Metadata
{
    private Metadata()
    {
    }

    public ClientId ClientId { get; private set; }
    public ShortId TrackingId { get; private set; }
    public ContractKind ContractKind { get; private set; }

    public static Metadata Parse(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentOutOfRangeException(nameof(value));

        var metadata = new Metadata();

        foreach (var keyValue in value.Split('|'))
        {
            var fields = keyValue.Split('=');

            if (fields.Length != 2)
                throw new ArgumentOutOfRangeException(nameof(value));

            switch(fields[0])
            {
                case nameof(ClientId):
                    metadata.ClientId = ClientId.From(fields[1]);
                    break;
                case nameof(TrackingId):
                    metadata.TrackingId = ShortId.From(fields[1]);
                    break;
                case nameof(ContractKind):
                    metadata.ContractKind = Enum.Parse<ContractKind>(fields[1]);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(value));
            }
        }

        return metadata;
    }
}
