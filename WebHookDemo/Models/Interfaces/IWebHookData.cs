using System.Text.Json.Nodes;

namespace WebHookDemo;

public interface IWebHookData<T>
    where T : new()
{
    Guid ContractId { get; }
    WebHookDataKind Kind { get; }

    public static abstract T Create(JsonNode? node);
}
