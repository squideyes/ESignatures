// ********************************************************
// The use of this source code is licensed under the terms
// of the MIT License (https://opensource.org/licenses/MIT)
// ********************************************************

namespace SquidEyes.ESignatures.Json;

public class WebHookError<M> : IWebHook<WebHookError<M>>
    where M : class
{
    public Guid ContractId { get; set; }
    public M? Metadata { get; set; }
    public string? Message { get; set; }
    public string? ErrorCode { get; set; }

    public WebHookKind WebHookKind => WebHookKind.WebHookError;
}