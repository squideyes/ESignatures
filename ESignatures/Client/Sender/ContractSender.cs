// ********************************************************
// The use of this source code is licensed under the terms
// of the MIT License (https://opensource.org/licenses/MIT)
// ********************************************************

using OneOf;
using SquidEyes.Fundamentals;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SquidEyes.ESignatures.Client;

public class ContractSender<M>
    where M : class
{
    public record Accepted(Guid ContractId, Signer[] Signers);
    public record Rejected(HttpStatusCode StatusCode, string ReasonPhrase);
    public record Failed(Exception Error);
    public record Cancelled();

    private record SignerInfo(
        Signer Signer, Address Address, Handling Handling);

    private static readonly HttpClient client = new();
    private static readonly Signer.Validator signerValidator = new();
    private static readonly Handling.Validator handlingValidator = new();
    private static readonly Address.Validator addressValidator = new();

    private readonly Dictionary<string, SignerInfo> signers = new();
    private readonly HashSet<Email> ccPdfsTo = new();
    private readonly Dictionary<string, string> keyValues = new();

    private readonly Uri contractsUri;
    private readonly Contract<M> contractInfo;

    private bool isTest = false;

    public ContractSender(Guid authToken, Contract<M> contractInfo)
    {
        authToken.MayNot().BeDefault();
        contractInfo.MayNot().BeNull();

        new Contract<M>.Validator().Validate(contractInfo);

        this.contractInfo = contractInfo;

        contractsUri = new Uri(
            $"https://esignatures.io/api/contracts?token={authToken}");

        var signDate = contractInfo.SignDate;

        if (signDate == default)
            signDate = DateOnly.FromDateTime(DateTime.Today);

        WithPlaceholder("sign-date", signDate.ToString("MM/dd/yyyy"));
        WithPlaceholder("sign-day", signDate.ToDayName());
        WithPlaceholder("sign-month", signDate.ToMonthName());
        WithPlaceholder("sign-year", signDate.Year.ToString());
    }

    public ContractSender<M> AsTest()
    {
        isTest = true;

        return this;
    }

    public ContractSender<M> WithPlaceholder(string key, object value)
    {
        key.Must().Be(v => v.IsApiKey());

        ArgumentNullException.ThrowIfNull(value);

        keyValues[key] = value.ToString()!;

        return this;
    }

    public ContractSender<M> WithSigner(
        Signer signer, Address address, Handling handling)
    {
        signerValidator.Validate(signer);
        addressValidator.Validate(address);
        handlingValidator.Validate(handling);

        signers.Add(signer.GetSha256Hash(),
            new SignerInfo(signer, address, handling));

        var dict = new Dictionary<string, string>
        {
            { "address1", address!.Address1! },
            { "address2", address!.Address2! },
            { "company", signer.Company! },
            { "country", address!.Country! },
            { "email", signer.Email!.ToString() },
            { "locality", address!.Locality! },
            { "mobile", signer.Mobile!.ToString() },
            { "nickname", signer.Nickname!.ToString() },
            { "name", signer.FullName! },
            { "one-line-address", address!.GetOneLineAddress() },
            { "postal-code", address!.PostalCode! },
            { "region", address!.Region! }
        };

        var prefix = signer.Nickname.ToString().ToLower() + "-";

        foreach (var key in dict.Keys)
            WithPlaceholder(prefix + key, dict[key]);

        return this;
    }

    private static SignerPoco GetSignerPoco(SignerInfo signerInfo)
    {
        var signer = signerInfo.Signer;
        var handling = signerInfo.Handling;

        var idModes = (handling.IdByEmail, handling.IdBySms) switch
        {
            (true, true) => new string[] { "email", "sms" },
            (true, false) => new string[] { "email" },
            (false, true) => new string[] { "sms" },
            (false, false) => null!
        };

        return new SignerPoco()
        {
            FullName = signer.FullName,
            Email = signer.Email.ToString(),
            Mobile = signer.Mobile.Formatted(PhoneFormat.E164),
            Company = signer.Company!,
            Ordinal = handling.Ordinal,
            GetDocBy = handling.GetDocBy.ToString().ToLower(),
            IdModes = idModes,
            SigReqBy = handling.SigReqBy.ToString().ToLower()
        };
    }

    public async Task<OneOf<Accepted, Rejected, Failed, Cancelled>> SendAsync(
        CancellationToken cancellationToken)
    {
        try
        {
            if (signers.Count == 0)
            {
                return new Rejected(HttpStatusCode.BadRequest,
                    "A contract must have one or more signers!");
            }

            var data = GetContractData();

            var options = new JsonSerializerOptions()
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };

            var requestJson = JsonSerializer.Serialize(data, options);

            var requestContent = new StringContent(
                requestJson, new MediaTypeHeaderValue("text/json"));

            var response = await client.PostAsync(
                contractsUri, requestContent, cancellationToken);

            if (response.StatusCode != HttpStatusCode.OK)
                return new Rejected(response.StatusCode, response.ReasonPhrase!);

            if (cancellationToken.IsCancellationRequested)
                return new Cancelled();

            var responseJson = await response.Content
                .ReadAsStringAsync(cancellationToken);

            if (cancellationToken.IsCancellationRequested)
                return new Cancelled();

            var (contractId, signerIds) =
                ContractResultParser.Parse(responseJson);

            foreach (var key in signerIds.Keys)
                signers[key].Signer.SignerId = signerIds[key];

            return new Accepted(contractId,
                signers.Values.Select(si => si.Signer).ToArray());
        }
        catch (Exception error)
        {
            return new Failed(error);
        }
    }

    private ContractPoco GetContractData()
    {
        return new ContractPoco()
        {
            Branding = GetBranding(),
            Emails = GetEmails(),
            ExpireHours = contractInfo.ExpiryHours,
            IsTest = isTest ? "yes" : "no",
            Locale = contractInfo.Locale.ToCode(),
            Metadata = contractInfo.Metadata.ToString()!,
            Placeholders = GetPlaceholders(),
            TemplateId = contractInfo.TemplateId.ToString(),
            Title = contractInfo.Title,
            Signers = GetSigners(),
            WebHook = contractInfo.WebHookUri.AbsoluteUri!
        };
    }

    private List<SignerPoco> GetSigners()
    {
        var signers = new List<SignerPoco>();

        foreach (var signer in this.signers.Values)
            signers.Add(ContractSender<M>.GetSignerPoco(signer));

        return signers;
    }

    private BrandingPoco GetBranding()
    {
        if (contractInfo.Company is null && contractInfo.LogoUri is null)
            return null!;

        return new BrandingPoco()
        {
            Company = contractInfo.Company!,
            LogoUri = contractInfo?.LogoUri
        };
    }

    private List<PlaceholderPoco> GetPlaceholders()
    {
        var placeholders = new List<PlaceholderPoco>();

        foreach (var apiKey in keyValues.Keys)
        {
            placeholders.Add(new PlaceholderPoco()
            {
                ApiKey = apiKey,
                Value = keyValues[apiKey]
            });
        }

        return placeholders;
    }

    private EmailsPoco GetEmails()
    {
        EmailsPoco poco = null!;

        if (contractInfo.RequestSpec == null && contractInfo.ContractSpec == null
            && contractInfo.ReplyTo == null && ccPdfsTo.Count == 0)
        {
            return poco;
        }

        poco = new EmailsPoco();

        if (contractInfo.RequestSpec != null)
        {
            poco.RequestSubject = contractInfo.RequestSpec.Subject;
            poco.RequestBodyText = contractInfo.RequestSpec.BodyText;
        }

        if (contractInfo.ContractSpec != null)
        {
            poco.ContractSubject = contractInfo.ContractSpec.Subject;
            poco.ContractBodyText = contractInfo.ContractSpec.BodyText;
        }

        if (contractInfo.ReplyTo != null)
            poco.ReplyTo = contractInfo.ReplyTo.ToString();

        if (ccPdfsTo.Count > 0)
        {
            poco.CcPdfsTo =
                ccPdfsTo.Select(v => v.ToString()).ToArray();
        }

        return poco;
    }
}