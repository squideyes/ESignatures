namespace ESignatures;

public interface IBasicWebHook<M>
    where M : class
{
    Guid ContractId { get; set; }
    M? Metadata { get; set; }
    Signer? Signer { get; set; }
}
