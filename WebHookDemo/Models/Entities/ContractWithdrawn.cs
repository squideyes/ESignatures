using System.Text.Json.Nodes;

namespace WebHookDemo;

public class ContractWithdrawn : IWebHookData<ContractWithdrawn>
{
    public Guid ContractId { get; set; }
    public Metadata? Metadata { get; set; }
    public Signer[]? Signers { get; set; }

    public WebHookDataKind Kind => WebHookDataKind.ContractWithdrawn;

    public static ContractWithdrawn Create(JsonNode? node)
    {
        var data = node.GetData("contract-withdrawn");

        var contract = data!["contract"]!;

        var signers = new List<Signer>();

        foreach (var s in contract!["signers"]!.AsArray())
        {
            signers.Add(new Signer()
            {
                SignerId = Guid.Parse(s.GetString("id")),
                Name = s.GetString("name"),
                Email = s.GetString("email"),
                Mobile = s.GetString("mobile"),
            });
        }

        return new ContractWithdrawn()
        {
            ContractId = Guid.Parse(data.GetString("contract_id")),
            Metadata = Metadata.Parse(contract.GetString("metadata")),
            Signers = signers.ToArray()
        };
    }
}