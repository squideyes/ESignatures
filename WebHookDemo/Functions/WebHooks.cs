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
            return GetResponse(request, Forbidden, "No Authorization Header!");

        var json = await request.ReadAsStringAsync();

        var node = JsonNode.Parse(json!);

        var data = node.GetString("status") switch
        {
            "contract-sent-to-signer" => HandleContractSent(request, node),
            "contract-signed" => HandleContractSigned(request, node),
            "contract-withdrawn" => HandleContractWithdrawn(request, node),
            "signer-viewed-the-contract" => HandleSignerViewed(request, node),
            "signer-signed" => HandleSignerSigned(request, node),
            "signer-declined" => HandleSignerDeclined(request, node),
            "signer-mobile-update-request" => HandleMobileUpdate(request, node),
            "error" => HandleWebHookError(request, node),
            _ => GetResponse(request, BadRequest, "Invalid \"Status\" Value")
        };

        return data;
    }

    private static HttpResponseData GetResponse(
        HttpRequestData request, HttpStatusCode statusCode, string message)
    {
        var response = request.CreateResponse(statusCode);

        response.Headers.Add("Content-Type", "text/plain; charset=utf-8");

        response.WriteString(message);

        return response;
    }

    private HttpResponseData HandleContractSent(
        HttpRequestData request, JsonNode? node)
    {
        var data = ContractSent.Create(node!);

        // TODO: raise EventSink event
        // TODO: change and expand to logger.Debug
        logger.LogWarning(data.ToJson(options));

        return GetResponse(request, OK, "Received");
    }

    private HttpResponseData HandleContractSigned(
        HttpRequestData request, JsonNode? node)
    {
        var data = ContractSigned.Create(node);

        // TODO: raise EventSink event
        // TODO: change and expand to logger.Debug
        logger.LogWarning(data.ToJson(options));

        return GetResponse(request, OK, "Received");
    }

    private HttpResponseData HandleContractWithdrawn(
        HttpRequestData request, JsonNode? node)
    {
        var data = ContractWithdrawn.Create(node);

        // TODO: raise EventSink event
        // TODO: change and expand to logger.Debug
        logger.LogWarning(data.ToJson(options));

        return GetResponse(request, OK, "Received");
    }

    private HttpResponseData HandleSignerViewed(
        HttpRequestData request, JsonNode? node)
    {
        var data = SignerViewed.Create(node);

        // TODO: raise EventSink event
        // TODO: change and expand to logger.Debug
        logger.LogWarning(data.ToJson(options));

        return GetResponse(request, OK, "Received");
    }

    private HttpResponseData HandleSignerSigned(
        HttpRequestData request, JsonNode? node)
    {
        var data = SignerSigned.Create(node);

        // TODO: raise EventSink event
        // TODO: change and expand to logger.Debug
        logger.LogWarning(data.ToJson(options));

        return GetResponse(request, OK, "Received");
    }

    private HttpResponseData HandleSignerDeclined(
        HttpRequestData request, JsonNode? node)
    {
        var data = SignerDeclined.Create(node);

        // TODO: raise EventSink event
        // TODO: change and expand to logger.Debug
        logger.LogWarning(data.ToJson(options));

        return GetResponse(request, OK, "Received");
    }

    private HttpResponseData HandleMobileUpdate(
        HttpRequestData request, JsonNode? node)
    {
        var data = MobileUpdate.Create(node);

        // TODO: raise EventSink event
        // TODO: change and expand to logger.Debug
        logger.LogWarning(data.ToJson(options));

        return GetResponse(request, OK, "Received");
    }

    private HttpResponseData HandleWebHookError(
        HttpRequestData request, JsonNode? node)
    {
        var data = WebHookError.Create(node);

        // TODO: raise EventSink event
        // TODO: change and expand to logger.Debug
        logger.LogWarning(data.ToJson(options));

        return GetResponse(request, OK, "Received");
    }
}
