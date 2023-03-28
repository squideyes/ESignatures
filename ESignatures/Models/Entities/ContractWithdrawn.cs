namespace ESignatures;

public class ContractWithdrawn : IWebHook<ContractWithdrawn>
{
    public Guid ContractId { get; set; }
    public Metadata? Metadata { get; set; }
    public Signer[]? Signers { get; set; }

    public WebHookKind WebHookKind => WebHookKind.ContractWithdrawn;
}