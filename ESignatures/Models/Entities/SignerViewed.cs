namespace ESignatures;

public class SignerViewed : IWebHook<SignerViewed>, IBasicWebHook
{
    public Guid ContractId { get; set; }
    public Metadata? Metadata { get; set; }
    public Signer? Signer { get; set; }

    public WebHookKind WebHookKind => WebHookKind.SignerViewed;
}