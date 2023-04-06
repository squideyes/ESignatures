using Azure.Storage.Blobs;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SharedModels;
using System.Net;
using System.Text.Json.Nodes;
using static Microsoft.Azure.Functions.Worker.AuthorizationLevel;
using static System.Net.HttpStatusCode;

namespace WebHookProcessor;

public class WebHookSink
{
    private const string WEBHOOK_RECEIVED = "webhook-received";

    public class ReceiveWebHookResult
    {
        public ReceiveWebHookResult(HttpRequestData request,
            HttpStatusCode statusCode, string message, string json = null!)
        {
            Response = request.GetResponse(statusCode, message);
            Json = json;
        }

        public HttpResponseData Response { get; }

        [QueueOutput(WEBHOOK_RECEIVED)]
        public string Json { get; }
    }

    private readonly ILogger logger;
    private readonly BlobContainerClient container;
    private readonly ServiceBusSender serviceBus;
    private readonly Guid apiKey;

    public WebHookSink(ILoggerFactory loggerFactory, IConfiguration config)
    {
        logger = loggerFactory.CreateLogger<WebHookSink>();

        var connString = config["AzureWebJobsStorage"];

        apiKey = Guid.Parse(config["ESignatures:ApiKey"]!);

        container = new BlobContainerClient(connString,
            config["Storage:Containers:ESignatures"]);

        serviceBus = new ServiceBusSender(
            config["ServiceBus:ConnString"]!,
            config["ServiceBus:Topic"]!,
            int.Parse(config["ServiceBus:TtlHours"]!));
    }

    //[Function("ProcessWebHook")]
    //public async Task ProcessWebHookAsync(
    //    [QueueTrigger(WEBHOOK_RECEIVED)] string json,
    //    CancellationToken cancellationToken)
    //{
    //    var node = JsonNode.Parse(json!);

    //    async Task SendAsync<T>(Func<JsonNode?, T> getWebHook)
    //        where T : IWebHook<T>, new()
    //    {
    //        await serviceBus.SendAsync(getWebHook(node!), cancellationToken);
    //    }

    //    //switch (node.GetString("status"))
    //    //{
    //    //    case "contract-sent-to-signer":
    //    //        await SendAsync(n => n!.ParseContractSent());
    //    //        break;
    //    //    case "contract-signed":
    //    //        await SendAsync(n => n!.ParseContractSigned());
    //    //        break;
    //    //    case "contract-withdrawn":
    //    //        await SendAsync(n => n!.ParseContractWithdrawn());
    //    //        break;
    //    //    case "signer-mobile-update-request":
    //    //        await SendAsync(n => n!.ParseMobileUpdate());
    //    //        break;
    //    //    case "signer-declined":
    //    //        await SendAsync(n => n!.ParseSignerDeclined());
    //    //        break;
    //    //    case "signer-signed":
    //    //        await SendAsync(n => n!.ParseSignerSigned());
    //    //        break;
    //    //    case "signer-viewed-the-contract":
    //    //        await SendAsync(n => n!.ParseSignerViewed());
    //    //        break;
    //    //    case "error":
    //    //        await SendAsync(n => n!.ParseWebHookError());
    //    //        break;
    //    //    default:
    //    //        throw new ArgumentOutOfRangeException("status");
    //    //}
    //}

    [Function("ReceiveWebHook")]
    public ReceiveWebHookResult ReceiveWebHookAsync([HttpTrigger(
        Function, "post", Route = "WebHook")] HttpRequestData request)
    {
        if (!request.Headers.TryGetValues("Authorization", out var headers))
        {
            return new ReceiveWebHookResult(
                request, Forbidden, "No Authorization Header");
        }

        var authHeader = headers.First();

        if (!authHeader.StartsWith("Basic "))
        {
            return new ReceiveWebHookResult(
                request, Forbidden, "No Basic-Auth Token!");
        }

        var apiKey = authHeader[6..].DecodeBase64()[0..^1];

        if (!Guid.TryParse(
            apiKey, out Guid guid) || this.apiKey != guid)
        {
            return new ReceiveWebHookResult(
                request, Forbidden, "Bad Basic-Auth Token!");
        }

        var json = request.ReadAsString();

        logger.LogDebug($"WebHook-Data: {json}");

        return new ReceiveWebHookResult(
            request, OK, "WebHook-Data Received!", json!);
    }
}
