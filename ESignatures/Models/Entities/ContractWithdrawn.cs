namespace ESignatures;

public class ContractWithdrawn<M> : IWebHook<ContractWithdrawn<M>>
    where M : class
{
    public Guid ContractId { get; set; }
    public M? Metadata { get; set; }
    public Signer[]? Signers { get; set; }

    public WebHookKind WebHookKind => WebHookKind.ContractWithdrawn;
}