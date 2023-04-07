// ********************************************************
// The use of this source code is licensed under the terms
// of the MIT License (https://opensource.org/licenses/MIT)
// ********************************************************

using Azure.Messaging.ServiceBus;
using SharedModels;
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
        var message = new ServiceBusMessage(data.ToJson())
        {
            ContentType = "application/json",
            CorrelationId = data.ContractId.ToString(),
            MessageId = Guid.NewGuid().ToString(),
            Subject = $"{data.ContractId} ({data.WebHookKind})",
            TimeToLive = TimeSpan.FromHours(ttlHours)
        };

        message.ApplicationProperties
            .Add("ContractId", data.ContractId);

        message.ApplicationProperties.Add(
            "WebHookKind", data.WebHookKind.ToString());

        return message;
    }
}