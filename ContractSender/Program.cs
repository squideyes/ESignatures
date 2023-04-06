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
var replyTo = config["ReplyTo"];
var ccPdfsTo = config["CcPdfsTo"];
var baseUri = new Uri(config["BaseUri"]!);
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

    var request = new ContractSender<Metadata>(authToken)
        .WithTemplate(templateId)
        .WithTitle($"Marketing Agreement (w/{p.Signer.FullName})")
        .WithWebHook(new Uri(baseUri, "/api/WebHook"))
        .WithLocale(Locale.EN)
        .WithExpiryHours(6)
        .WithSigner(p.Signer, p.Handling, p.Address)
        .WithSigner(v.Signer, v.Handling, v.Address)
        .WithPubDate(pubDate)
        .WithMetadata(metadata)
        .WithPlaceholder("client-id", metadata.ClientId)
        .WithPlaceholder("doc-code", metadata.DocCode)
        .WithReplyTo("louis@squideyes.com")
        .WithCcPdfsTo(Email.From("louis@squideyes.com"))
        .WithEmailSpec(EmailKind.Request, GetRequestEmailSpec())
        .WithEmailSpec(EmailKind.Contract, GetContractEmailSpec())
        .AsTest();

    (await request.SendAsync(cts.Token)).Switch(
        HandleAccepted, HandleRejected, HandleFailed, HandleCancelled);
}
catch (Exception error)
{
    Console.WriteLine("ERROR: " + error.Message);
}

ContractSender<Metadata>.EmailSpec GetRequestEmailSpec()
{
    return new ContractSender<Metadata>.EmailSpec(
        "Your document is ready to sign",
        """
        Hi __FULL_NAME__, 
        
        To review and sign the contract please press the button below.        
        
        Kind Regards.
        """);
}

ContractSender<Metadata>.EmailSpec GetContractEmailSpec()
{
    return new ContractSender<Metadata>.EmailSpec(
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
    var docDate = new DateOnly(2023, 5, 23);

    return new Metadata(clientId, docKind, docDate);
}

SignerInfo GetSignerInfo(
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