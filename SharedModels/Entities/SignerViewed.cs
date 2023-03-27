namespace SharedModels;

public class SignerViewed : IWebHook<SignerViewed>, IContractSigner
{
    public Guid ContractId { get; set; }
    public Metadata? Metadata { get; set; }
    public Signer? Signer { get; set; }

    public WebHookKind Kind => WebHookKind.SignerViewed;
}