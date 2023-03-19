using System.Text.Json.Nodes;

namespace WebHookDemo;

public class ContractSent : IWebHookData<ContractSent>, IContractSigner
{
    public Guid ContractId { get; set; }
    public Metadata? Metadata { get; set; }
    public Signer? Signer { get; set; }

    public WebHookDataKind Kind => WebHookDataKind.ContractSent;

    public static ContractSent Create(JsonNode? node) =>
        BasicParser<ContractSent>.Parse(node, "contract-sent-to-signer");
}