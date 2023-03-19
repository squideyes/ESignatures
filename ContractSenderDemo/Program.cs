using ContractSenderDemo;
using Microsoft.Extensions.Configuration;
using SquidEyes.ESignatures;

var config = new ConfigurationBuilder()
    .AddUserSecrets<Program>()
    .AddCommandLine(args)
    .Build();

var authToken = Guid.Parse(config["AuthToken"]!);
var templateId = Guid.Parse(config["TemplateId"]!);
var baseUri = new Uri(config["BaseUri"]!);
var date = DateTime.Today;
var partnerEmail = config["PartnerEmail"]!;
var partnerMobile = config["PartnerMobile"]!;
var vendorEmail = config["VendorEmail"]!;
var vendorMobile = config["VendorMobile"]!;

var cts = new CancellationTokenSource();

try
{
    var vendor = GetSigner(SignerKind.Vendor,
        vendorEmail, vendorMobile, 1);

    var partner = GetSigner(SignerKind.Partner,
        partnerEmail, partnerMobile, 0);

    var request = new ContractSender(authToken, templateId)
        .AddPlaceholder("day", date.ToDayName())
        .AddPlaceholder("month", date.ToMonthName())
        .AddPlaceholder("year", date.Year)
        .AddPlaceholder("partner-company", partner.Company!)
        .AddPlaceholder("partner-knownas", partner.KnownAs!)
        .AddPlaceholder("partner-region", partner.Region!)
        .AddPlaceholder("partner-address", partner.GetAddress())
        .AddPlaceholder("vendor-company", vendor.Company!)
        .AddPlaceholder("vendor-knownas", vendor.KnownAs!)
        .AddPlaceholder("vendor-region", vendor.Region!)
        .AddPlaceholder("vendor-address", vendor.GetAddress())
        .WithMetadata("ClientId", "ABC12345")
        .WithMetadata("ContractKind", "Partnership")
        .WithLocale(Locale.EN)
        .AddSigners(partner, vendor)
        .WithWebHook(new Uri(baseUri, "/api/WebHook"))
        .AsTest();

    (await request.SendAsync(cts.Token)).Switch(
        HandleAccepted, HandleRejected, HandleFailed, HandleCancelled);
}
catch(Exception error)
{
    Console.WriteLine("ERROR: " + error.Message);
}

Signer GetSigner(SignerKind kind, 
    string email, string mobile, int ordinal)
{
    return new Signer()
    {
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