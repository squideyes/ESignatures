namespace SharedModels;

public class SignerSigned : IWebHook<SignerSigned>, IContractSigner
{
    public Guid ContractId { get; set; }
    public Metadata? Metadata { get; set; }
    public Signer? Signer { get; set; }

    public WebHookKind Kind => WebHookKind.SignerSigned;
}