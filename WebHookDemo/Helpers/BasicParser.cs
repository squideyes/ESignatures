using System.Text.Json.Nodes;

namespace WebHookDemo;

internal class BasicParser<T>
    where T : IContractSigner, new()
{
    public static T Parse(JsonNode? node, string status)
    {
        var data = node.GetData(status);

        var contract = data!["contract"]!;

        var signer = data!["signer"]!;

        return new T()
        {
            ContractId = Guid.Parse(contract.GetString("id")),
            Signer = new Signer()
            {
                SignerId = Guid.Parse(signer.GetString("id")),
                Name = signer.GetString("name"),
                Email = signer.GetString("email"),
                Mobile = signer.GetString("mobile")
            },
            Metadata = Metadata.Parse(contract.GetString("metadata"))
        };
    }
}
