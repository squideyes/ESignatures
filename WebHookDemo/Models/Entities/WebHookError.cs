using System.Text.Json.Nodes;

namespace WebHookDemo;

public class WebHookError : IWebHookData<WebHookError>
{
    public Guid ContractId { get; set; }
    public Metadata? Metadata { get; set; }
    public string? ErrorCode { get; set; }
    public string? Message { get; set; }

    public WebHookDataKind Kind => WebHookDataKind.WebHookError;

    public static WebHookError Create(JsonNode? node)
    {
        var data = node.GetData("error");

        return new WebHookError()
        {
            ErrorCode = data.GetString("error_code"),
            Message = data.GetString("error_message"),
            ContractId = Guid.Parse(data.GetString("contract_id")),
            Metadata = Metadata.Parse(data.GetString("metadata"))
        };
    }
}