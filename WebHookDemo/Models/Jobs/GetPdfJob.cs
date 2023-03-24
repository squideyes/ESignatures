namespace WebHookDemo;

public class GetPdfJob
{
    public required Guid ContractId { get; init; }
    public required Metadata? Metadata { get; init; }
    public required Uri? PdfUri { get; init; }
    public required Signer[]? Signers { get; init; }
}
