namespace ESignatures;

public class SignerSigned : IWebHook<SignerSigned>, IBasicWebHook
{
    public Guid ContractId { get; set; }
    public Metadata? Metadata { get; set; }
    public Signer? Signer { get; set; }

    public WebHookKind WebHookKind => WebHookKind.SignerSigned;
}