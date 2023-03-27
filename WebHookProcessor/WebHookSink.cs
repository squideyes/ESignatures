using Azure.Storage.Blobs;
using Azure.Storage.Queues;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SharedModels;
using System.Net;
using System.Text.Json.Nodes;
using static System.Net.HttpStatusCode;

namespace WebHookProcessor;

public class WebHookSink
{
    private readonly ILogger logger;
    private readonly QueueClient queue;
    private readonly BlobContainerClient container;

    public WebHookSink(ILoggerFactory loggerFactory, IConfiguration config)
    {
        logger = loggerFactory.CreateLogger<WebHookSink>();

        var connString = config["AzureWebJobsStorage"];

        queue = new QueueClient(connString,
            config["Storage:Queues:ContractSigned"]);

        container = new BlobContainerClient(connString,
            config["Storage:Containers:ESignatures"]);
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

        var data = WebHookParser.ParseSignerSigned(node!);

        var json = data.ToJson();

        // TODO: remove 
        logger.LogWarning(json);

        return GetResponse(request, OK, "Received");
    }

    private async Task<HttpResponseData> HandleContractSignedAsync(
        HttpRequestData request, JsonNode? node, CancellationToken cancellationToken)
    {
        var data = WebHookParser.ParseContractSigned(node);

        var json = data.ToJson();

        var message = new BinaryData(json);

        await queue.SendMessageAsync(
            message, cancellationToken: cancellationToken);

        // TODO: remove 
        logger.LogWarning(json);

        return GetResponse(request, OK, "Received");
    }

    private async Task<HttpResponseData> HandleContractWithdrawnAsync(
        HttpRequestData request, JsonNode? node, CancellationToken cancellationToken)
    {
        await Task.CompletedTask;

        var data = WebHookParser.ParseContractWithdrawn(node);

        var json = data.ToJson();

        // TODO: remove 
        logger.LogWarning(json);

        return GetResponse(request, OK, "Received");
    }

    private async Task<HttpResponseData> HandleSignerViewedAsync(
        HttpRequestData request, JsonNode? node, CancellationToken cancellationToken)
    {
        await Task.CompletedTask;

        var data = WebHookParser.ParseSignerViewed(node);

        var json = data.ToJson();

        // TODO: remove 
        logger.LogWarning(json);

        return GetResponse(request, OK, "Received");
    }

    private async Task<HttpResponseData> HandleSignerSignedAsync(
        HttpRequestData request, JsonNode? node, CancellationToken cancellationToken)
    {
        await Task.CompletedTask;

        var data = WebHookParser.ParseSignerSigned(node);

        var json = data.ToJson();

        // TODO: remove 
        logger.LogWarning(json);

        return GetResponse(request, OK, "Received");
    }

    private async Task<HttpResponseData> HandleSignerDeclinedAsync(
        HttpRequestData request, JsonNode? node, CancellationToken cancellationToken)
    {
        await Task.CompletedTask;

        var data = WebHookParser.ParseSignerDeclined(node);

        var json = data.ToJson();

        // TODO: remove 
        logger.LogWarning(json);

        return GetResponse(request, OK, "Received");
    }

    private async Task<HttpResponseData> HandleMobileUpdateAsync(
        HttpRequestData request, JsonNode? node, CancellationToken cancellationToken)
    {
        await Task.CompletedTask;

        var data = WebHookParser.ParseMobileUpdate(node);

        var json = data.ToJson();

        // TODO: remove 
        logger.LogWarning(json);

        return GetResponse(request, OK, "Received");
    }

    private async Task<HttpResponseData> HandleWebHookErrorAsync(
        HttpRequestData request, JsonNode? node, CancellationToken cancellationToken)
    {
        await Task.CompletedTask;

        var data = WebHookParser.ParseWebHookError(node);

        var json = data.ToJson();

        // TODO: remove 
        logger.LogWarning(json);

        return GetResponse(request, OK, "Received");
    }
}
