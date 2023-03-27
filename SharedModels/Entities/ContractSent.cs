namespace SharedModels;

public class ContractSent : IWebHook<ContractSent>, IContractSigner
{
    public Guid ContractId { get; set; }
    public Metadata? Metadata { get; set; }
    public Signer? Signer { get; set; }

    public WebHookKind Kind => WebHookKind.ContractSent;
}