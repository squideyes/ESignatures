namespace ESignatures;

public class MobileUpdate<M> : IWebHook<MobileUpdate<M>>
    where M : class
{
    public Guid ContractId { get; set; }
    public M? Metadata { get; set; }
    public Signer? Signer { get; set; }
    public string? NewMobile { get; set; }

    public WebHookKind WebHookKind => WebHookKind.MobileUpdate;
}