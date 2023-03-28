namespace ESignatures;

public interface IBasicWebHook
{
    Guid ContractId { get; set; }
    Metadata? Metadata { get; set; }
    Signer? Signer { get; set; }
}
