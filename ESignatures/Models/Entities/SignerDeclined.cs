﻿namespace ESignatures;

public class SignerDeclined<M> : IWebHook<SignerDeclined<M>>, IBasicWebHook<M>
    where M : class
{
    public Guid ContractId { get; set; }
    public M? Metadata { get; set; }
    public Signer? Signer { get; set; }

    public WebHookKind WebHookKind => WebHookKind.SignerDeclined;
}