using Microsoft.Extensions.Configuration;
using SharedModels;
using SquidEyes.Basics;
using SquidEyes.ESignatures;
using static SharedModels.SignerKind;

var config = new ConfigurationBuilder()
    .AddUserSecrets<Program>()
    .AddCommandLine(args)
    .Build();

// Sourced from UserSecrets:
var authToken = Guid.Parse(config["AuthToken"]!);

// Sourced from command-line parameters
var templateId = Guid.Parse(config["TemplateId"]!);
var baseUri = new Uri(config["BaseUri"]!);
var partnerEmail = config["PartnerEmail"]!;
var partnerMobile = config["PartnerMobile"]!;
var vendorEmail = config["VendorEmail"]!;
var vendorMobile = config["VendorMobile"]!;

// Generated or known
var date = DateOnly.FromDateTime(DateTime.Today);
var clientId = ClientId.Next();
var trackingId = ShortId.Next();
var contractKind = ContractKind.JointMarketing;

var cts = new CancellationTokenSource();

try
{
    var partner = GetSigner(Partner, partnerEmail, partnerMobile, 0);
    var vendor = GetSigner(Vendor, vendorEmail, vendorMobile, 1);

    var request = new ContractSender(authToken, templateId)
        .WithTitle($"Marketing Agreement (w/{partner.Name})")
        .WithMetadata("ClientId", clientId)
        .WithMetadata("TrackingId", trackingId)
        .WithMetadata("ContractKind", contractKind)
        .WithDayMonthYear(date)
        .WithPlaceholder("client-id", clientId)
        .WithPlaceholder("tracking-id", trackingId)
        .WithPlaceholder("contract-kind", contractKind)
        .WithLocale(Locale.EN)
        .WithExpiryInHours(6)
        //.WithSigner(vendor)
        .WithSigner(partner)
        .WithWebHook(new Uri(baseUri, "/api/WebHook"))
        .AsTest();

    (await request.SendAsync(cts.Token)).Switch(
        HandleAccepted, HandleRejected, HandleFailed, HandleCancelled);
}
catch(Exception error)
{
    Console.WriteLine("ERROR: " + error.Message);
}

SquidEyes.ESignatures.Signer GetSigner(SignerKind kind, string email, string mobile, int ordinal)
{
    return new SquidEyes.ESignatures.Signer()
    {
        Kind = kind.ToString(),
        Ordinal = ordinal,
        Name = $"{kind} Mc{kind}",
        Email = email,
        Mobile = mobile,
        Company = $"{kind}, Inc.",
        KnownAs = $"{kind}",
        Country = "US",
        Address1 = $"123 {kind} Blvd.",
        Locality = $"{kind} Town",
        Region = "NY",
        PostalCode = "12345"
    };
}

void HandleAccepted(ContractSender.Accepted accepted)
{
    Console.WriteLine($"The {accepted.ContractId} contract was accepted by eSignatures.io for further processing!");
}

void HandleRejected(ContractSender.Rejected rejected) =>
    Console.WriteLine($"{rejected.StatusCode}: ({rejected.ReasonPhrase})");

void HandleFailed(ContractSender.Failed failed) =>
    Console.WriteLine("FAILURE: " + failed.Error.Message);

void HandleCancelled(ContractSender.Cancelled cancelled) =>
    Console.WriteLine("The process was cancelled!");