using Azure.Messaging.ServiceBus;
using SharedModels;
using System.Text.Json;
using System.Text.Json.Serialization;
using AMS = Azure.Messaging.ServiceBus;

namespace WebHookProcessor;

internal sealed class ServiceBusSender : IAsyncDisposable
{
    private readonly ServiceBusClient client;
    private readonly AMS.ServiceBusSender sender;
    private readonly int ttlHours;

    public ServiceBusSender(
        string connString, string topicName, int ttlHours)
    {
        client = new ServiceBusClient(connString);
        sender = client.CreateSender(topicName);
        this.ttlHours = ttlHours;
    }

    public async ValueTask DisposeAsync()
    {
        await client.DisposeAsync();
        await sender.DisposeAsync();
    }

    public async Task SendAsync<T>(T webHook, CancellationToken cancellationToken)
        where T : IWebHook<T>, new()
    {
        await sender.SendMessageAsync(GetMessage(webHook), cancellationToken);
    }

    private ServiceBusMessage GetMessage<T>(T data)
        where T : IWebHook<T>, new()
    {
        var options = new JsonSerializerOptions();

        options.Converters.Add(new JsonStringEnumConverter());

        var json = JsonSerializer.Serialize(data, options);

        var message = new ServiceBusMessage(json)
        {
            ContentType = "application/json",
            CorrelationId = data.ContractId.ToString(),
            MessageId = Guid.NewGuid().ToString(),
            Subject = "Test", // Improve This
            TimeToLive = TimeSpan.FromHours(ttlHours)
        };

        message.ApplicationProperties
            .Add("ContractId", data.ContractId);

        message.ApplicationProperties.Add(
            "WebHookKind", data.WebHookKind.ToString());

        return message;
    }
}
