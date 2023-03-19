using System.Text.Json.Nodes;

namespace WebHookDemo;

public class SignerSigned : IWebHookData<SignerSigned>, IContractSigner
{
    public Guid ContractId { get; set; }
    public Metadata? Metadata { get; set; }
    public Signer? Signer { get; set; }

    public WebHookDataKind Kind => WebHookDataKind.SignerSigned;

    public static SignerSigned Create(JsonNode? node) =>
        BasicParser<SignerSigned>.Parse(node, "signer-signed");
}