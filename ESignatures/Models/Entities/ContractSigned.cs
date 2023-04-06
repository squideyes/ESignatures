namespace ESignatures;

public class ContractSigned<M> : IWebHook<ContractSigned<M>>
    where M : class
{
    public Guid ContractId { get; init; }
    public M? Metadata { get; init; }
    public Uri? PdfUri { get; init; }
    public Signer[]? Signers { get; init; }

    public WebHookKind WebHookKind => WebHookKind.ContractSigned;

    public string GetBlobName() => $"Signed/{ContractId:N}.json";

    public Dictionary<string, string> GetMetadata()
    {
        return new Dictionary<string, string>()
        {
            { "ContractId", ContractId.ToString() },
            //{ "ClientId", Metadata!["ClientId"] },
            //{ "TrackingId", Metadata!["TrackingId"] },
            { "Signers", Signers.ToJson() },
            //{ "ContractKind", Metadata!["Token"] }
        };
    }
}
