// ********************************************************
// The use of this source code is licensed under the terms
// of the MIT License (https://opensource.org/licenses/MIT)
// ********************************************************

using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SharedModels;
using SquidEyes.ESignatures.WebHook;
using System.Net;
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
            Json = json;
            Response = request.GetResponse(statusCode, message);
        }

        [QueueOutput(WEBHOOK_RECEIVED)]
        public string Json { get; }

        public HttpResponseData Response { get; }
    }

    private readonly ILogger logger;
    private readonly ServiceBusSender serviceBus;
    private readonly Guid apiKey;

    public WebHookSink(ILoggerFactory loggerFactory, IConfiguration config)
    {
        logger = loggerFactory.CreateLogger<WebHookSink>();

        apiKey = Guid.Parse(config["ESignatures:ApiKey"]!);

        serviceBus = new ServiceBusSender(
            config["ServiceBus:ConnString"]!,
            config["ServiceBus:Topic"]!,
            int.Parse(config["ServiceBus:TtlHours"]!));
    }

    async Task SendAsync<T>(T data, CancellationToken cancellationToken)
        where T : IWebHook<T>, new()
    {
        await serviceBus.SendAsync(data, cancellationToken);
    }

    [Function("ProcessWebHook")]
    public async Task ProcessWebHookAsync(
        [QueueTrigger(WEBHOOK_RECEIVED)] string json,
        CancellationToken cancellationToken)
    {
        await JsonHelper.Parse<Metadata>(json, v => default!).Match(
            contractSent => SendAsync(contractSent, cancellationToken),
            contractSigned => SendAsync(contractSigned, cancellationToken),
            contractWithdrawn => SendAsync(contractWithdrawn, cancellationToken),
            mobileUpdate => SendAsync(mobileUpdate, cancellationToken),
            signerDeclined => SendAsync(signerDeclined, cancellationToken),
            signerSigned => SendAsync(signerSigned, cancellationToken),
            signerViewed => SendAsync(signerViewed, cancellationToken),
            webHookError => SendAsync(webHookError, cancellationToken));
    }

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