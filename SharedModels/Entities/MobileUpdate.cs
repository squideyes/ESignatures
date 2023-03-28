namespace SharedModels;

public class MobileUpdate : IWebHook<MobileUpdate>
{
    public Guid ContractId { get; set; }
    public Metadata? Metadata { get; set; }
    public Signer? Signer { get; set; }
    public string? NewMobile { get; set; }

    public WebHookKind WebHookKind => WebHookKind.MobileUpdate;
}