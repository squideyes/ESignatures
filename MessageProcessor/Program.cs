// ********************************************************
// The use of this source code is licensed under the terms
// of the MIT License (https://opensource.org/licenses/MIT)
// ********************************************************

using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using System.Diagnostics;
using System.Text.Json.Nodes;

const string TOPIC = "esignatures";
const string SUBSCRIPTION = "WebHookReceived";

var config = new ConfigurationBuilder()
    .AddUserSecrets<Program>()
    .Build();

var connString = config["ServiceBus:ConnString"];

var client = new ServiceBusClient(connString);

var processor = client.CreateProcessor(
    TOPIC, SUBSCRIPTION, new ServiceBusProcessorOptions());

try
{
    processor.ProcessMessageAsync += MessageHandler;

    processor.ProcessErrorAsync += ErrorHandler;

    await processor.StartProcessingAsync();

    Console.WriteLine("Press any key to terminate...");

    Console.ReadKey(true);

    Console.WriteLine();

    Console.WriteLine("Stopping the receiver...");

    await processor.StopProcessingAsync();

    Console.WriteLine("Stopped receiving messages");
}
finally
{
    await processor.DisposeAsync();
    await client.DisposeAsync();
}

async Task MessageHandler(ProcessMessageEventArgs args)
{
    var json = args.Message.Body.ToString();

    Console.WriteLine($"Received: {json}");

    var node = JsonNode.Parse(json);

    var kind = node!["WebHookKind"];

    if (kind!.GetValue<string>() == "ContractSigned")
        await FetchAndOpenPdfAsync(node);

    await args.CompleteMessageAsync(args.Message);
}

async Task FetchAndOpenPdfAsync(JsonNode? node)
{
    var uri = new Uri(node!["PdfUri"]!.GetValue<string>());

    var client = new HttpClient();

    var fileName = Path.ChangeExtension(
        Path.GetTempFileName(), ".pdf");

    var source = await client.GetStreamAsync(uri);

    using (var target = File.OpenWrite(fileName))
        source.CopyTo(target);

    try
    {
        var process = new Process();

        process.StartInfo.FileName = fileName;
        process.StartInfo.UseShellExecute = true;

        process.Start();
    }
    catch (Exception error)
    {
        Console.WriteLine(
            $"ERROR: {error.Message} (PDF: {fileName})");
    }
}

Task ErrorHandler(ProcessErrorEventArgs args)
{
    Console.WriteLine(args.Exception);

    return Task.CompletedTask;
}