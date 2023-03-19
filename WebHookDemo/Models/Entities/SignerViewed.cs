using System.Text.Json.Nodes;

namespace WebHookDemo;

public class SignerViewed : IWebHookData<SignerViewed>, IContractSigner
{
    public Guid ContractId { get; set; }
    public Metadata? Metadata { get; set; }
    public Signer? Signer { get; set; }

    public WebHookDataKind Kind => WebHookDataKind.SignerViewed;

    public static SignerViewed Create(JsonNode? node) =>
        BasicParser<SignerViewed>.Parse(node, "signer-viewed-the-contract");
}