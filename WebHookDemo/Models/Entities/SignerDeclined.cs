using System.Text.Json.Nodes;

namespace WebHookDemo;

public class SignerDeclined : IWebHookData<SignerDeclined>, IContractSigner
{
    public Guid ContractId { get; set; }
    public Metadata? Metadata { get; set; }
    public Signer? Signer { get; set; }

    public WebHookDataKind Kind => WebHookDataKind.SignerDeclined;

    public static SignerDeclined Create(JsonNode? node) =>
        BasicParser<SignerDeclined>.Parse(node, "signer-declined");
}