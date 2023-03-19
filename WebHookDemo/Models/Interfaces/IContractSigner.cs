namespace WebHookDemo;

internal interface IContractSigner
{
    Guid ContractId { get; set; }
    Metadata? Metadata { get; set; }
    Signer? Signer { get; set; }
}
