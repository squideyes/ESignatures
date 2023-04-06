namespace ESignatures;

public class WebHookError<M> : IWebHook<WebHookError<M>>
    where M : class
{
    public Guid ContractId { get; set; }
    public M? Metadata { get; set; }
    public string? Message { get; set; }
    public string? ErrorCode { get; set; }

    public WebHookKind WebHookKind => WebHookKind.WebHookError;
}