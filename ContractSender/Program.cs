// ********************************************************
// The use of this source code is licensed under the terms
// of the MIT License (https://opensource.org/licenses/MIT)
// ********************************************************

using ContractSenderDemo;
using ESignatures.Client;
using Microsoft.Extensions.Configuration;
using SharedModels;
using SquidEyes.Fundamentals;
using static SharedModels.Nickname;

var config = new ConfigurationBuilder()
    .AddUserSecrets<Program>()
    .AddJsonFile("appsettings.json", false, false)
    .AddCommandLine(args)
    .Build();

var cts = new CancellationTokenSource();

// Sourced from UserSecrets:
var authToken = Guid.Parse(config["AuthToken"]!);

// Sourced from command-line parameters
var partnerEmail = Email.From(config["PartnerEmail"]!);
var partnerMobile = Phone.From(config["PartnerMobile"]!);

// Source from appsettings.json
var templateId = Guid.Parse(config["TemplateId"]!);
var baseUri = new Uri(config["BaseUri"]!);
var replyTo = config["ReplyTo"]!.ToEmailOrNull();
var ccPdfsTo = config["CcPdfsTo"]!.ToEmailArrayOrNull();
var logoUri = config["LogoUri"]!.ToUriOrNull();
var vendorEmail = Email.From(config["VendorEmail"]!);
var vendorMobile = Phone.From(config["VendorMobile"]!);

// Generated or known
var metadata = GetMetadata();
var signDate = DateOnly.FromDateTime(DateTime.Today);

try
{
    var p = GetSignerPlan(Partner, partnerEmail, partnerMobile, 0);
    var v = GetSignerPlan(Vendor, vendorEmail, vendorMobile, 1);

    var title = $"Marketing Agreement (w/{p.Signer.Company})";

    var contract = new Contract<Metadata>()
    {
        TemplateId = templateId,
        Metadata = metadata,
        Company = v.Signer.Company!,
        Title = title,
        ReplyTo = replyTo,
        LogoUri = logoUri!,
        RequestSpec = GetRequestSpec(title),
        ContractSpec = GetContractSpec(title),
        WebHookUri = new Uri(baseUri, "/api/WebHook")
    };

    var request = new ContractSender<Metadata>(authToken, contract)
        .WithSignerInfo(p.Signer, p.Handling, p.Address)
        .WithSignerInfo(v.Signer, v.Handling, v.Address)
        .WithPlaceholder("client-id", metadata.ClientId)
        .WithPlaceholder("doc-code", metadata.DocCode)
        .AsTest();

    (await request.SendAsync(cts.Token)).Switch(
        HandleAccepted, HandleRejected, HandleFailed, HandleCancelled);
}
catch (Exception error)
{
    Console.WriteLine("ERROR: " + error.Message);
}

EmailSpec GetRequestSpec(string title)
{
    return new EmailSpec(
        $"Your \"{title}\" is READY-TO-SIGN",
        $"To review and sign the \"{title}\", please press the \"View and Sign\" button, below.");
}

EmailSpec GetContractSpec(string title)
{
    return new EmailSpec(
        $"Your \"{title}\" is SIGNED",
        $"Your \"{title}\" is signed");
}

Metadata GetMetadata()
{
    var clientId = ClientId.Next();
    var docKind = DocKind.PartnerContract;
    var signDate = new DateOnly(2023, 5, 23);

    return new Metadata(clientId, docKind, signDate);
}

SignerPlan GetSignerPlan(
    Nickname nickname, Email email, Phone mobile, int ordinal)
{
    var signer = new Signer()
    {
        FullName = $"{nickname} Mc{nickname}",
        Nickname = Token.From(nickname.ToString()),
        Email = email,
        Mobile = mobile,
        Company = $"{nickname}, Inc.",
    };

    var handling = new Handling()
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

    return new SignerPlan(signer, handling, address);
}

void HandleAccepted(ContractSender<Metadata>.Accepted accepted) =>
    Console.WriteLine($"The {accepted.ContractId} contract was accepted by eSignatures.io for further processing!");

void HandleRejected(ContractSender<Metadata>.Rejected rejected) =>
    Console.WriteLine($"{rejected.StatusCode}: ({rejected.ReasonPhrase})");

void HandleFailed(ContractSender<Metadata>.Failed failed) =>
    Console.WriteLine("FAILURE: " + failed.Error.Message);

void HandleCancelled(ContractSender<Metadata>.Cancelled cancelled) =>
    Console.WriteLine("The process was cancelled!");