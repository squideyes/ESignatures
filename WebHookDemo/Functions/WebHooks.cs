using Azure.Storage.Blobs;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using static System.Net.HttpStatusCode;

namespace WebHookDemo;

public class WebHooks
{
    private readonly ILogger logger;
    private readonly BlobContainerClient container;
    private readonly JsonSerializerOptions options;

    public WebHooks(ILoggerFactory loggerFactory, IConfiguration config)
    {
        logger = loggerFactory.CreateLogger<WebHooks>();

        container = new BlobContainerClient(
            config["AzureWebJobsStorage"], "signatures");

        options = new JsonSerializerOptions()
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        options.Converters.Add(new JsonStringEnumConverter());
    }

    [Function("WebHook")]
    public async Task<HttpResponseData> WebHookAsync(
        [HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData request)
    {
        if (!request.Headers.TryGetValues("Authorization", out var authHeader))
            return await GetResponseAsync(request, Forbidden, "No Authorization Header!");

        // handle bad authHeader

        var json = await request.ReadAsStringAsync();

        var node = JsonNode.Parse(json!);

        var response = node.GetString("status") switch
        {
            "contract-sent-to-signer" => HandleContractSentAsync(request, node),
            "contract-signed" => HandleContractSignedAsync(request, node),
            "contract-withdrawn" => HandleContractWithdrawnAsync(request, node),
            "signer-viewed-the-contract" => HandleSignerViewedAsync(request, node),
            "signer-signed" => HandleSignerSignedAsync(request, node),
            "signer-declined" => HandleSignerDeclinedAsync(request, node),
            "signer-mobile-update-request" => HandleMobileUpdateAsync(request, node),
            "error" => HandleWebHookErrorAsync(request, node),
            _ => GetResponseAsync(request, BadRequest, "Invalid \"Status\" Value")
        };

        return await response;
    }

    private static async Task<HttpResponseData> GetResponseAsync(
        HttpRequestData request, HttpStatusCode statusCode, string message)
    {
        await Task.CompletedTask;

        var response = request.CreateResponse(statusCode);

        response.Headers.Add("Content-Type", "text/plain; charset=utf-8");

        response.WriteString(message);

        return response;
    }

    private async Task<HttpResponseData> HandleContractSentAsync(
        HttpRequestData request, JsonNode? node)
    {
        var data = ContractSent.Create(node!);

        // TODO: raise EventSink event
        // TODO: change and expand to logger.Debug
        logger.LogWarning(data.ToJson(options));

        return await GetResponseAsync(request, OK, "Received");
    }

    private async Task<HttpResponseData> HandleContractSignedAsync(
        HttpRequestData request, JsonNode? node)
    {
        var data = ContractSigned.Create(node);

        var json = data.ToJson(options);

        //var blob = container.GetBlobClient(data.GetBlobName(""));



        //await blob.UploadAsync(json);

        // TODO: raise EventSink event
        // TODO: change and expand to logger.Debug
        logger.LogWarning(json);

        return await GetResponseAsync(request, OK, "Received");
    }

    private async Task<HttpResponseData> HandleContractWithdrawnAsync(
        HttpRequestData request, JsonNode? node)
    {
        var data = ContractWithdrawn.Create(node);

        // TODO: raise EventSink event
        // TODO: change and expand to logger.Debug
        logger.LogWarning(data.ToJson(options));

        return await GetResponseAsync(request, OK, "Received");
    }

    private async Task<HttpResponseData> HandleSignerViewedAsync(
        HttpRequestData request, JsonNode? node)
    {
        var data = SignerViewed.Create(node);

        // TODO: raise EventSink event
        // TODO: change and expand to logger.Debug
        logger.LogWarning(data.ToJson(options));

        return await GetResponseAsync(request, OK, "Received");
    }

    private async Task<HttpResponseData> HandleSignerSignedAsync(
        HttpRequestData request, JsonNode? node)
    {
        var data = SignerSigned.Create(node);

        // TODO: raise EventSink event
        // TODO: change and expand to logger.Debug
        logger.LogWarning(data.ToJson(options));

        return await GetResponseAsync(request, OK, "Received");
    }

    private async Task<HttpResponseData> HandleSignerDeclinedAsync(
        HttpRequestData request, JsonNode? node)
    {
        var data = SignerDeclined.Create(node);

        // TODO: raise EventSink event
        // TODO: change and expand to logger.Debug
        logger.LogWarning(data.ToJson(options));

        return await GetResponseAsync(request, OK, "Received");
    }

    private async Task<HttpResponseData> HandleMobileUpdateAsync(
        HttpRequestData request, JsonNode? node)
    {
        var data = MobileUpdate.Create(node);

        // TODO: raise EventSink event
        // TODO: change and expand to logger.Debug
        logger.LogWarning(data.ToJson(options));

        return await GetResponseAsync(request, OK, "Received");
    }

    private async Task<HttpResponseData> HandleWebHookErrorAsync(
        HttpRequestData request, JsonNode? node)
    {
        var data = WebHookError.Create(node);

        // TODO: raise EventSink event
        // TODO: change and expand to logger.Debug
        logger.LogWarning(data.ToJson(options));

        return await GetResponseAsync(request, OK, "Received");
    }
}
