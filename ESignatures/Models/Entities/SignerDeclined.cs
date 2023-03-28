namespace ESignatures;

public class SignerDeclined : IWebHook<SignerDeclined>, IBasicWebHook
{
    public Guid ContractId { get; set; }
    public Metadata? Metadata { get; set; }
    public Signer? Signer { get; set; }

    public WebHookKind WebHookKind => WebHookKind.SignerDeclined;
}