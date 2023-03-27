namespace SharedModels;

public class SignerDeclined : IWebHook<SignerDeclined>, IContractSigner
{
    public Guid ContractId { get; set; }
    public Metadata? Metadata { get; set; }
    public Signer? Signer { get; set; }

    public WebHookKind Kind => WebHookKind.SignerDeclined;
}