// ********************************************************
// The use of this source code is licensed under the terms
// of the MIT License (https://opensource.org/licenses/MIT)
// ********************************************************

namespace SquidEyes.ESignatures.WebHook;

public class ContractSigned<M> : IWebHook<ContractSigned<M>>
    where M : class
{
    public Guid ContractId { get; init; }
    public M? Metadata { get; init; }
    public Uri? PdfUri { get; init; }
    public ContractSigner[]? Signers { get; init; }

    public WebHookKind WebHookKind => WebHookKind.ContractSigned;

    public string GetBlobName() => $"Signed/{ContractId:N}.json";
}