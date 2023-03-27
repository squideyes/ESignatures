namespace SharedModels;

public class WebHookError : IWebHook<WebHookError>
{
    public Guid ContractId { get; set; }
    public Metadata? Metadata { get; set; }
    public string? ErrorCode { get; set; }
    public string? Message { get; set; }

    public WebHookKind Kind => WebHookKind.WebHookError;
}