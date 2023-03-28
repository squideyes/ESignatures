using Azure.Storage.Blobs;
using Azure.Storage.Queues;
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
    private readonly ILogger logger;
    private readonly QueueClient queue;
    private readonly BlobContainerClient container;
    private readonly ServiceBusSender serviceBus;

    public WebHookSink(ILoggerFactory loggerFactory, IConfiguration config)
    {
        logger = loggerFactory.CreateLogger<WebHookSink>();

        var connString = config["AzureWebJobsStorage"];

        queue = new QueueClient(connString,
            config["Storage:Queues:ContractSigned"]);

        container = new BlobContainerClient(connString,
            config["Storage:Containers:ESignatures"]);

        serviceBus = new ServiceBusSender(
            config["ServiceBus:ConnString"]!, 
            config["ServiceBus:Topic"]!, 
            int.Parse(config["ServiceBus:TtlHours"]!));
    }

    [Function("ContractSigned")]
    public void ContractSignedAsync([QueueTrigger("")] ContractSigned contractSigned)
    {
    }

    [Function("WebHook")]
    public async Task<HttpResponseData> WebHookAsync([HttpTrigger(Function, "post")]
        HttpRequestData request, CancellationToken cancellationToken)
    {
        HttpResponseData GetResponse(HttpStatusCode statusCode, string message)
        {
            var response = request.CreateResponse(statusCode);

            response.Headers.Add("Content-Type", "text/plain; charset=utf-8");

            response.WriteString(message);

            return response;
        }

        if (!request.Headers.TryGetValues("Authorization", out var authHeader))
            return GetResponse(Forbidden, "No Authorization Header!");

        // handle bad authHeader

        var json = await request.ReadAsStringAsync();

        logger.LogDebug($"Raw WebHook: {json}");

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
            _ => (WebHookKind?)null!
        };

        if (!kind.HasValue)
            return GetResponse(BadRequest, "Invalid \"Status\" Value");

        switch (kind)
        {
            case WebHookKind.ContractSent:
                var contractSent = node!.ParseContractSent();
                await serviceBus.SendAsync(contractSent, cancellationToken);
                break;
            case WebHookKind.ContractSigned:
                var contractSigned = node!.ParseContractSigned();
                await queue.SendMessageAsync(contractSigned.ToJson());
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

        return GetResponse(OK, "Received");
    }
}
