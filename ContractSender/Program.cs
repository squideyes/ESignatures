using ContractSenderDemo;
using ESignatures;
using Microsoft.Extensions.Configuration;
using SharedModels;
using SquidEyes.Fundamentals;
using static SharedModels.Nickname;

var config = new ConfigurationBuilder()
    .AddUserSecrets<Program>()
    .AddCommandLine(args)
    .Build();

var cts = new CancellationTokenSource();

// Sourced from UserSecrets:
var authToken = Guid.Parse(config["AuthToken"]!);

// Sourced from command-line parameters
var templateId = Guid.Parse(config["TemplateId"]!);
var baseUri = new Uri(config["BaseUri"]!);
var company = config["Company"]!;
var replyTo = config["ReplyTo"]!.ToEmailOrNull();
var ccPdfsTo = config["CcPdfsTo"]!.ToEmailArrayOrNull();
var logoUri = config["LogoUri"]!.ToUriOrNull();
var partnerEmail = Email.From(config["PartnerEmail"]!);
var partnerMobile = Phone.From(config["PartnerMobile"]!);
var vendorEmail = Email.From(config["VendorEmail"]!);
var vendorMobile = Phone.From(config["VendorMobile"]!);

// Generated or known
var pubDate = DateOnly.FromDateTime(DateTime.Today);
var metadata = GetMetadata();

try
{
    var p = GetSignerInfo(Partner, partnerEmail, partnerMobile, 0);
    var v = GetSignerInfo(Vendor, vendorEmail, vendorMobile, 1);

    var contractInfo = new ContractInfo<Metadata>()
    {
        TemplateId = templateId,
        Metadata = metadata,
        Company = company,
        Title = $"Marketing Agreement (w/{p.Signer.FullName})",
        ReplyTo = replyTo,
        LogoUri = logoUri!,
        RequestSpec = GetRequestSpec(),
        ContractSpec = GetContractSpec(),
        WebHookUri = new Uri(baseUri, "/api/WebHook")
    };

    var request = new ContractSender<Metadata>(authToken, contractInfo)
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

EmailSpec GetRequestSpec()
{
    return new EmailSpec(
        "Your document is ready to sign",
        """
        Hi __FULL_NAME__, 
        
        To review and sign the contract please press the button below.        
        
        Kind Regards.
        """);
}

EmailSpec GetContractSpec()
{
    return new EmailSpec(
        "Your document is signed",
        """
        Hi __FULL_NAME__,

        Your document is signed.                
        
        Kind Regards
        """);
}

Metadata GetMetadata()
{
    var clientId = ClientId.Next();
    var docKind = DocKind.PartnerContract;
    var signDate = new DateOnly(2023, 5, 23);

    return new Metadata(clientId, docKind, signDate);
}

SignerInfo GetSignerInfo(Nickname nickname, Email email, Phone mobile, int ordinal)
{
    var signer = new Signer()
    {
        FullName = $"{nickname} Mc{nickname}",
        Nickname = Token.From(nickname.ToString()),
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

void HandleAccepted(ContractSender<Metadata>.Accepted accepted) =>
    Console.WriteLine($"The {accepted.ContractId} contract was accepted by eSignatures.io for further processing!");

void HandleRejected(ContractSender<Metadata>.Rejected rejected) =>
    Console.WriteLine($"{rejected.StatusCode}: ({rejected.ReasonPhrase})");

void HandleFailed(ContractSender<Metadata>.Failed failed) =>
    Console.WriteLine("FAILURE: " + failed.Error.Message);

void HandleCancelled(ContractSender<Metadata>.Cancelled cancelled) =>
    Console.WriteLine("The process was cancelled!");