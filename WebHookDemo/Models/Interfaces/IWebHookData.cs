using System.Text.Json.Nodes;

namespace WebHookDemo;

public interface IWebHookData<T>
    where T : new()
{
    WebHookDataKind Kind { get; }

    public static abstract T Create(JsonNode? node);
}
