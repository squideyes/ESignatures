using Azure;
using Azure.Messaging.EventGrid;
using Azure.Storage.Blobs;
using Azure.Storage.Queues;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json.Nodes;
using static System.Net.HttpStatusCode;

namespace WebHookDemo;

public class WebHooks
{
    private readonly ILogger logger;
    private readonly QueueClient queue;
    private readonly BlobContainerClient container;
    private readonly EventGridPublisherClient eventGrid;
    private readonly ContractEventsToSend contractEventsToSend;

    public WebHooks(ILoggerFactory loggerFactory, IConfiguration config)
    {
        logger = loggerFactory.CreateLogger<WebHooks>();

        var connString = config["AzureWebJobsStorage"];

        queue = new QueueClient(connString,
            config["Storage:Queues:ContractSigned"]);

        container = new BlobContainerClient(connString,
            config["Storage:Containers:ESignatures"]);

        contractEventsToSend = Enum.Parse<ContractEventsToSend>(
            config["EventGrid:ContractSigned:ContractEventsToSend"]!);

        eventGrid = new EventGridPublisherClient(
            new Uri(config["EventGrid:ContractSigned:Uri"]!),
            new AzureKeyCredential(config["EventGrid:ContractSigned:Key"]!));
    }

    [Function("WebHook")]
    public async Task<HttpResponseData> WebHookAsync(
        [HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData request,
        CancellationToken cancellationToken)
    {
        if (!request.Headers.TryGetValues("Authorization", out var authHeader))
            return GetResponse(request, Forbidden, "No Authorization Header!");

        // handle bad authHeader

        var json = await request.ReadAsStringAsync();

        var node = JsonNode.Parse(json!);

        var response = node.GetString("status") switch
        {
            "contract-sent-to-signer" =>
                await HandleContractSentAsync(request, node, cancellationToken),
            "contract-signed" =>
                await HandleContractSignedAsync(request, node, cancellationToken),
            "contract-withdrawn" =>
                await HandleContractWithdrawnAsync(request, node, cancellationToken),
            "signer-viewed-the-contract" =>
                await HandleSignerViewedAsync(request, node, cancellationToken),
            "signer-signed" =>
                await HandleSignerSignedAsync(request, node, cancellationToken),
            "signer-declined" =>
                await HandleSignerDeclinedAsync(request, node, cancellationToken),
            "signer-mobile-update-request" =>
                await HandleMobileUpdateAsync(request, node, cancellationToken),
            "error" =>
                await HandleWebHookErrorAsync(request, node, cancellationToken),
            _ =>
                GetResponse(request, BadRequest, "Invalid \"Status\" Value")
        };

        return response;
    }

    private static HttpResponseData GetResponse(
        HttpRequestData request, HttpStatusCode statusCode, string message)
    {
        var response = request.CreateResponse(statusCode);

        response.Headers.Add("Content-Type", "text/plain; charset=utf-8");

        response.WriteString(message);

        return response;
    }

    private async Task<HttpResponseData> HandleContractSentAsync(
        HttpRequestData request, JsonNode? node, CancellationToken cancellationToken)
    {
        await Task.CompletedTask;

        var data = ContractSent.Create(node!);

        var json = data.ToJson();

        if (!cancellationToken.IsCancellationRequested)
            await SendEventAsync(data, json, cancellationToken);

        // TODO: remove 
        logger.LogWarning(json);

        return GetResponse(request, OK, "Received");
    }

    private async Task SendEventAsync<T>(T data, string json, CancellationToken cancellationToken)
        where T : IWebHookData<T>, new()
    {
        bool CanSend(ContractEventsToSend contractEventsToSend) =>
            this.contractEventsToSend.HasFlag(contractEventsToSend);

        var canSend = data switch
        {
            ContractSent _ => CanSend(ContractEventsToSend.ContractSent),
            ContractSigned _ => CanSend(ContractEventsToSend.ContractSigned),
            ContractWithdrawn _ => CanSend(ContractEventsToSend.ContractWithdrawn),
            SignerViewed _ => CanSend(ContractEventsToSend.SignerViewed),
            SignerSigned _ => CanSend(ContractEventsToSend.SignerSigned),
            SignerDeclined _ => CanSend(ContractEventsToSend.SignerDeclined),
            MobileUpdate _ => CanSend(ContractEventsToSend.MobileUpdate),
            WebHookError _ => CanSend(ContractEventsToSend.WebHookError),
            _ => throw new ArgumentOutOfRangeException(nameof(data))
        };

        if (!canSend)
            return;

        var events = new List<EventGridEvent>
        {
            new EventGridEvent(data.ToSubject(), typeof(T).FullName, "1.0", json),
        };

        await eventGrid.SendEventsAsync(events, cancellationToken);
    }

    private async Task<HttpResponseData> HandleContractSignedAsync(
        HttpRequestData request, JsonNode? node, CancellationToken cancellationToken)
    {
        var data = ContractSigned.Create(node);

        var json = data.ToJson();

        var message = new BinaryData(json);

        await queue.SendMessageAsync(
            message, cancellationToken: cancellationToken);

        if (!cancellationToken.IsCancellationRequested)
            await SendEventAsync(data, json, cancellationToken);

        // TODO: remove 
        logger.LogWarning(json);

        return GetResponse(request, OK, "Received");
    }

    private async Task<HttpResponseData> HandleContractWithdrawnAsync(
        HttpRequestData request, JsonNode? node, CancellationToken cancellationToken)
    {
        await Task.CompletedTask;

        var data = ContractWithdrawn.Create(node);

        var json = data.ToJson();

        if (!cancellationToken.IsCancellationRequested)
            await SendEventAsync(data, json, cancellationToken);

        // TODO: remove 
        logger.LogWarning(json);

        return GetResponse(request, OK, "Received");
    }

    private async Task<HttpResponseData> HandleSignerViewedAsync(
        HttpRequestData request, JsonNode? node, CancellationToken cancellationToken)
    {
        await Task.CompletedTask;

        var data = SignerViewed.Create(node);

        var json = data.ToJson();

        if (!cancellationToken.IsCancellationRequested)
            await SendEventAsync(data, json, cancellationToken);

        // TODO: remove 
        logger.LogWarning(json);

        return GetResponse(request, OK, "Received");
    }

    private async Task<HttpResponseData> HandleSignerSignedAsync(
        HttpRequestData request, JsonNode? node, CancellationToken cancellationToken)
    {
        await Task.CompletedTask;

        var data = SignerSigned.Create(node);

        var json = data.ToJson();

        if (!cancellationToken.IsCancellationRequested)
            await SendEventAsync(data, json, cancellationToken);

        // TODO: remove 
        logger.LogWarning(json);

        return GetResponse(request, OK, "Received");
    }

    private async Task<HttpResponseData> HandleSignerDeclinedAsync(
        HttpRequestData request, JsonNode? node, CancellationToken cancellationToken)
    {
        await Task.CompletedTask;

        var data = SignerDeclined.Create(node);

        var json = data.ToJson();

        if (!cancellationToken.IsCancellationRequested)
            await SendEventAsync(data, json, cancellationToken);

        // TODO: remove 
        logger.LogWarning(json);

        return GetResponse(request, OK, "Received");
    }

    private async Task<HttpResponseData> HandleMobileUpdateAsync(
        HttpRequestData request, JsonNode? node, CancellationToken cancellationToken)
    {
        await Task.CompletedTask;

        var data = MobileUpdate.Create(node);

        var json = data.ToJson();

        if (!cancellationToken.IsCancellationRequested)
            await SendEventAsync(data, json, cancellationToken);

        // TODO: remove 
        logger.LogWarning(json);

        return GetResponse(request, OK, "Received");
    }

    private async Task<HttpResponseData> HandleWebHookErrorAsync(
        HttpRequestData request, JsonNode? node, CancellationToken cancellationToken)
    {
        await Task.CompletedTask;

        var data = WebHookError.Create(node);

        var json = data.ToJson();

        if (!cancellationToken.IsCancellationRequested)
            await SendEventAsync(data, json, cancellationToken);

        // TODO: remove 
        logger.LogWarning(json);

        return GetResponse(request, OK, "Received");
    }
}
