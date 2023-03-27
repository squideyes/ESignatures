namespace SharedModels;

public class ContractSigned : IWebHook<ContractSigned>
{
    public Guid ContractId { get; init; }
    public Metadata? Metadata { get; init; }
    public Uri? PdfUri { get; init; }
    public Signer[]? Signers { get; init; }

    public WebHookKind Kind => WebHookKind.ContractSigned;

    public string GetBlobName() => $"Signed/{ContractId:N}.json";

    public Dictionary<string, string> GetMetadata()
    {
        return new Dictionary<string, string>()
        {
            { "ContractId", ContractId.ToString() },
            { "ClientId", Metadata!.ClientId.ToString() },
            { "TrackingId", Metadata!.TrackingId.ToString() },
            { "Signers", Signers.ToJson() },
            { "ContractKind", Metadata!.ContractKind.ToString() }
        };
    }
}
