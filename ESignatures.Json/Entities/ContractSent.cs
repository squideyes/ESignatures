// ********************************************************
// The use of this source code is licensed under the terms
// of the MIT License (https://opensource.org/licenses/MIT)
// ********************************************************

namespace ESignatures.Json;

public class ContractSent<M> : IWebHook<ContractSent<M>>, IBasicWebHook<M>
    where M : class
{
    public Guid ContractId { get; set; }
    public M? Metadata { get; set; }
    public BasicSigner? Signer { get; set; }

    public WebHookKind WebHookKind => WebHookKind.ContractSent;
}