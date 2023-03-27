namespace SharedModels;

public class ContractWithdrawn : IWebHook<ContractWithdrawn>
{
    public Guid ContractId { get; set; }
    public Metadata? Metadata { get; set; }
    public Signer[]? Signers { get; set; }

    public WebHookKind Kind => WebHookKind.ContractWithdrawn;
}