// ********************************************************
// The use of this source code is licensed under the terms
// of the MIT License (https://opensource.org/licenses/MIT)
// ********************************************************

namespace ESignatures.Json;

public class MobileUpdate<M> : IWebHook<MobileUpdate<M>>
    where M : class
{
    public Guid ContractId { get; set; }
    public M? Metadata { get; set; }
    public Signer? Signer { get; set; }
    public string? NewMobile { get; set; }

    public WebHookKind WebHookKind => WebHookKind.MobileUpdate;
}