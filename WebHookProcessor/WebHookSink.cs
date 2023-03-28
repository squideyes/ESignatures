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
    public class Result
    {
        public Result(HttpRequestData request, 
            HttpStatusCode statusCode, string message, string json = null!)
        {
            Response = request.GetResponse(statusCode, message);
            Json = json;
        }

        public HttpResponseData Response { get; }

        [QueueOutput(Known.QueueNames.WebHookReceived)]
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

    [Function("ProcessWebHook")]
    public async Task ProcessWebHookAsync(
        [QueueTrigger(Known.QueueNames.WebHookReceived)] string json,
        CancellationToken cancellationToken)
    {
        var node = JsonNode.Parse(json!);

        var kind = node.GetString("status") switch
        {
            "contract-sent-to-signer" => WebHookKind.ContractSent,
            "signer-viewed-the-contract" => WebHookKind.SignerViewed,
            "signer-signed" => WebHookKind.SignerSigned,
            "signer-declined" => WebHookKind.SignerDeclined,
            "contract-signed" => WebHookKind.ContractSigned,
            "contract-withdrawn" => WebHookKind.ContractWithdrawn,
            "signer-mobile-update-request" => WebHookKind.MobileUpdate,
            "error" => WebHookKind.WebHookError,
            _ => throw new ArgumentOutOfRangeException("status")
        };

        switch (kind)
        {
            case WebHookKind.ContractSent:
                var contractSent = node!.ParseContractSent();
                await serviceBus.SendAsync(contractSent, cancellationToken);
                break;
            case WebHookKind.ContractSigned:
                var contractSigned = node!.ParseContractSigned();
                //await queue.SendMessageAsync(contractSigned.ToJson());
                await serviceBus.SendAsync(contractSigned, cancellationToken);
                break;
            case WebHookKind.ContractWithdrawn:
                var contractWithdrawn = node!.ParseContractWithdrawn();
                await serviceBus.SendAsync(contractWithdrawn, cancellationToken);
                break;
            case WebHookKind.MobileUpdate:
                var mobileUpdate = node!.ParseMobileUpdate();
                await serviceBus.SendAsync(mobileUpdate, cancellationToken);
                break;
            case WebHookKind.SignerDeclined:
                var signerDeclined = node!.ParseSignerDeclined();
                await serviceBus.SendAsync(signerDeclined, cancellationToken);
                break;
            case WebHookKind.SignerSigned:
                var signerSigned = node!.ParseSignerSigned();
                await serviceBus.SendAsync(signerSigned, cancellationToken);
                break;
            case WebHookKind.SignerViewed:
                var signerViewed = node!.ParseSignerViewed();
                await serviceBus.SendAsync(signerViewed, cancellationToken);
                break;
            case WebHookKind.WebHookError:
                var webHookError = node!.ParseWebHookError();
                await serviceBus.SendAsync(webHookError, cancellationToken);
                break;
        }
    }

    [Function("ReceiveWebHook")]
    public Result ReceiveWebHookAsync([HttpTrigger(
        Function, "post", Route = "WebHook")] HttpRequestData request)
    {
        if (!request.Headers.TryGetValues("Authorization", out var headers))
            return new Result(request, Forbidden, "No Authorization Header");

        var authHeader = headers.First();

        if (!authHeader.StartsWith("Basic "))
            return new Result(request, Forbidden, "No Basic-Auth Token!");

        var apiKey = authHeader[6..].DecodeBase64()[0..^1];

        if (!Guid.TryParse(apiKey, out Guid guid) || this.apiKey != guid)
            return new Result(request, Forbidden, "Bad Basic-Auth Token!");

        var json = request.ReadAsString();

        logger.LogDebug($"WebHook-Data: {json}");

        return new Result(request, OK, "WebHook-Data Received!", json!);
    }



















    //[Function("ContractSigned")]
    //public void ContractSignedAsync(
    //    [QueueTrigger("")] ContractSigned contractSigned)
    //{
    //}

    //[Function("WebHook")]
    //public async Task<HttpResponseData> WebHookAsync([HttpTrigger(Function, "post")]
    //    HttpRequestData request, CancellationToken cancellationToken)
    //{
    //    HttpResponseData GetResponse(HttpStatusCode statusCode, string message)
    //    {
    //        var response = request.CreateResponse(statusCode);

    //        response.Headers.Add("Content-Type", "text/plain; charset=utf-8");

    //        response.WriteString(message);

    //        return response;
    //    }

    //    if (!request.Headers.TryGetValues("Authorization", out var authHeader))
    //        return GetResponse(Forbidden, "No Authorization Header!");

    //    // handle bad authHeader

    //    var json = await request.ReadAsStringAsync();

    //    logger.LogDebug($"Raw WebHook: {json}");

    //    var node = JsonNode.Parse(json!);

    //    var kind = node.GetString("status") switch
    //    {
    //        "contract-sent-to-signer" => WebHookKind.ContractSent,
    //        "signer-viewed-the-contract" => WebHookKind.SignerViewed,
    //        "signer-signed" => WebHookKind.SignerSigned,
    //        "signer-declined" => WebHookKind.SignerDeclined,
    //        "contract-signed" => WebHookKind.ContractSigned,
    //        "contract-withdrawn" => WebHookKind.ContractWithdrawn,
    //        "signer-mobile-update-request" => WebHookKind.MobileUpdate,
    //        "error" => WebHookKind.WebHookError,
    //        _ => (WebHookKind?)null!
    //    };

    //    if (!kind.HasValue)
    //        return GetResponse(BadRequest, "Invalid \"Status\" Value");

    //    switch (kind)
    //    {
    //        case WebHookKind.ContractSent:
    //            var contractSent = node!.ParseContractSent();
    //            await serviceBus.SendAsync(contractSent, cancellationToken);
    //            break;
    //        case WebHookKind.ContractSigned:
    //            var contractSigned = node!.ParseContractSigned();
    //            await queue.SendMessageAsync(contractSigned.ToJson());
    //            await serviceBus.SendAsync(contractSigned, cancellationToken);
    //            break;
    //        case WebHookKind.ContractWithdrawn:
    //            var contractWithdrawn = node!.ParseContractWithdrawn();
    //            await serviceBus.SendAsync(contractWithdrawn, cancellationToken);
    //            break;
    //        case WebHookKind.MobileUpdate:
    //            var mobileUpdate = node!.ParseMobileUpdate();
    //            await serviceBus.SendAsync(mobileUpdate, cancellationToken);
    //            break; 
    //        case WebHookKind.SignerDeclined:
    //            var signerDeclined = node!.ParseSignerDeclined();
    //            await serviceBus.SendAsync(signerDeclined, cancellationToken);
    //            break;
    //        case WebHookKind.SignerSigned:
    //            var signerSigned = node!.ParseSignerSigned();
    //            await serviceBus.SendAsync(signerSigned, cancellationToken);
    //            break;
    //        case WebHookKind.SignerViewed:
    //            var signerViewed = node!.ParseSignerViewed();
    //            await serviceBus.SendAsync(signerViewed, cancellationToken);
    //            break;
    //        case WebHookKind.WebHookError:
    //            var webHookError = node!.ParseWebHookError();
    //            await serviceBus.SendAsync(webHookError, cancellationToken);
    //            break;
    //    }

    //    return GetResponse(OK, "Received");
    //}
}
