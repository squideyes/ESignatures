namespace ESignatures;

public class ContractSent : IWebHook<ContractSent>, IBasicWebHook
{
    public Guid ContractId { get; set; }
    public Metadata? Metadata { get; set; }
    public Signer? Signer { get; set; }

    public WebHookKind WebHookKind => WebHookKind.ContractSent;
}