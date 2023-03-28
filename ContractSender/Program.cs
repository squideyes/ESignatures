using ESignatures;
using Microsoft.Extensions.Configuration;
using SharedModels;
using SquidEyes.Basics;
using static SharedModels.Nickname;

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
    var p = GetSignerInfo(Partner, partnerEmail, partnerMobile, 0);
    var v = GetSignerInfo(Vendor, vendorEmail, vendorMobile, 1);

    var request = new ContractSender(authToken, templateId)
        .WithTitle($"Marketing Agreement (w/{p.Signer.FullName})")
        .WithWebHook(new Uri(baseUri, "/api/WebHook"))
        .WithExpiryInHours(6)
        .WithLocale(Locale.EN)
        .WithSigner(p.Signer, p.Handling, p.Address)
        .WithSigner(v.Signer, v.Handling, v.Address)
        .WithDayMonthYear(date)
        .WithMetadata("ClientId", clientId)
        .WithPlaceholder("client-id", clientId)
        .WithMetadata("TrackingId", trackingId)
        .WithPlaceholder("tracking-id", trackingId)
        .WithMetadata("ContractKind", contractKind)
        .WithPlaceholder("contract-kind", contractKind)
        .AsTest();

    (await request.SendAsync(cts.Token)).Switch(
        HandleAccepted, HandleRejected, HandleFailed, HandleCancelled);
}
catch(Exception error)
{
    Console.WriteLine("ERROR: " + error.Message);
}

SignerInfo GetSignerInfo(
    Nickname nickname, string email, string mobile, int ordinal)
{
    var signer = new Signer()
    {
        FullName = $"{nickname} Mc{nickname}",
        Nickname = nickname.ToString(),
        Email = email,
        Mobile = mobile,
        Company = $"{nickname}, Inc.",
    };

    var signingPlan = new Handling()
    {
        Ordinal = ordinal,
        IdBySms = true,
        IdByEmail = true,
        SigReqBy = Mode.Email,
        GetDocBy = Mode.Email
    };

    var address = new Address()
    {
        Country = "US",
        Address1 = $"123 {nickname} Blvd.",
        Locality = $"{nickname} Town",
        Region = "NY",
        PostalCode = "12345"
    };

    return new SignerInfo(signer, signingPlan, address);
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