using ESignatures.Internal;
using OneOf;
using SquidEyes.Fundamentals;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ESignatures;

public class ContractSender<M>
    where M : class
{
    public record Accepted(Guid ContractId, Signer[] Signers);
    public record Rejected(HttpStatusCode StatusCode, string ReasonPhrase);
    public record Failed(Exception Error);
    public record Cancelled();
    public record EmailSpec(string Subject, string Body);

    private static readonly HttpClient client = new();
    private static readonly Signer.Validator signerValidator = new();
    private static readonly Handling.Validator signingPlanValidator = new();
    private static readonly Address.Validator addressValidator = new();

    private readonly Dictionary<string, SignerInfo> signerInfos = new();
    private readonly Dictionary<string, string> placeholders = new();
    private readonly HashSet<Email> ccEmails = new();
    private readonly Dictionary<EmailKind, EmailSpec> emailSpecs = new();

    private readonly Uri contractsUri;

    private Guid templateId = default;
    private Email replyTo = default;
    private M metadata = null!;
    private Uri webHookUri = null!;
    private bool isTest = false;
    private Locale locale = Locale.EN;
    private int expiryHours = 48;
    private string? title = null!;

    public ContractSender(Guid authToken)
    {
        authToken.MayNot().BeDefault();

        contractsUri = new Uri(
            $"https://esignatures.io/api/contracts?token={authToken}");
    }

    public ContractSender<M> WithTemplate(Guid templateId)
    {
        templateId.MayNot().BeDefault();

        this.templateId = templateId;

        return this;
    }

    public ContractSender<M> WithTitle(string title)
    {
        if (title is not null && string.IsNullOrWhiteSpace(title))
            throw new ArgumentOutOfRangeException(nameof(title));

        this.title = title!;

        return this;
    }

    public ContractSender<M> WithWebHook(Uri uri)
    {
        if (!uri.IsAbsoluteUri)
            throw new ArgumentOutOfRangeException(nameof(uri));

        webHookUri = uri;

        return this;
    }

    public ContractSender<M> WithExpiryHours(int hours)
    {
        expiryHours.Must().BeBetween(1, 168);

        expiryHours = hours;

        return this;
    }

    public ContractSender<M> WithLocale(Locale locale)
    {
        if (locale == default)
            throw new ArgumentNullException(nameof(locale));

        this.locale = locale;

        return this;
    }

    public ContractSender<M> WithMetadata(M value)
    {
        metadata = value.MayNot().BeNull();

        return this;
    }

    public ContractSender<M> AsTest()
    {
        isTest = true;

        return this;
    }

    public ContractSender<M> WithPubDate(DateOnly pubDate = default)
    {
        if (pubDate == default)
            pubDate = DateOnly.FromDateTime(DateTime.Today);

        placeholders["pub-date"] = pubDate.ToString("MM/dd/yyyy");
        placeholders["pub-day"] = pubDate.ToDayName();
        placeholders["pub-month"] = pubDate.ToMonthName();
        placeholders["pub-year"] = pubDate.Year.ToString();

        return this;
    }

    public ContractSender<M> WithPlaceholder(string key, object value)
    {
        if (!key.IsApiKey())
            throw new ArgumentOutOfRangeException(nameof(key));

        ArgumentNullException.ThrowIfNull(value);

        placeholders[key] = value.ToString()!;

        return this;
    }

    public ContractSender<M> WithSigner(Signer signer, Handling handling,
        Address? address = null!, bool addPlaceholders = true)
    {
        signerValidator.Validate(signer);

        signingPlanValidator.Validate(handling);

        if (address != null)
            addressValidator.Validate(address);

        signerInfos.Add(signer.GetSha256Hash(),
            new SignerInfo(signer, handling, address!));

        if (addPlaceholders)
        {
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
        }

        return this;
    }

    public ContractSender<M> WithCcEmail(string email) =>
        WithCcPdfsTo(Email.From(email));

    public ContractSender<M> WithCcPdfsTo(Email email)
    {
        email.MayNot().BeDefault();

        ccEmails.Add(email);

        return this;
    }

    public ContractSender<M> WithReplyTo(string email) =>
        WithReplyTo(Email.From(email));

    public ContractSender<M> WithReplyTo(Email email)
    {
        email.MayNot().BeDefault();

        replyTo = email;

        return this;
    }

    public ContractSender<M> WithEmailSpec(
        EmailKind emailKind, EmailSpec emailSpec)
    {
        emailKind.Must().BeEnumValue();
        emailSpec.MayNot().BeNull();

        emailSpecs[emailKind] = emailSpec;

        return this;
    }

    private static SignerData GetSignerData(SignerInfo info)
    {
        var signer = info.Signer;
        var handling = info.Handling;

        var idModes = (handling.IdByEmail, handling.IdBySms) switch
        {
            (true, true) => new string[] { "email", "sms" },
            (true, false) => new string[] { "email" },
            (false, true) => new string[] { "sms" },
            (false, false) => null!
        };

        return new SignerData()
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
        static void FailIfNotValid(bool isValid, string message)
        {
            if (!isValid)
                throw new InvalidOperationException(message);
        }

        try
        {
            FailIfNotValid(!templateId.IsDefault(),
                "A \"TemplateId\" must be supplied!");

            FailIfNotValid(!title.IsDefault(),
                "A \"Title\" must be supplied!");

            FailIfNotValid(webHookUri != null,
                "A \"WebHook URI\" must be suppplied!");

            FailIfNotValid(placeholders.ContainsKey("pub-date"),
                "A \"PubDate\" must be supplied!");

            FailIfNotValid(signerInfos.Count > 0,
                "A contract must have one or more signers!");

            var data = new ContractData()
            {
                ExpireHours = expiryHours,
                IsTest = isTest ? "yes" : "no",
                Locale = locale.ToCode(),
                Metadata = metadata.ToString()!,
                TemplateId = templateId.ToString(),
                Title = title!,
                WebHook = webHookUri?.AbsoluteUri!
            };

            if (signerInfos.Count > 0)
            {
                data.Signers = new List<SignerData>();

                foreach (var signer in signerInfos.Values)
                    data.Signers.Add(GetSignerData(signer));
            }

            if (placeholders.Count > 0)
            {
                data.Placeholders = new List<Placeholder>();

                foreach (var apiKey in placeholders.Keys)
                {
                    data.Placeholders.Add(new Placeholder()
                    {
                        ApiKey = apiKey,
                        Value = placeholders[apiKey]
                    });
                }
            }

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

            var responseJson = await response.Content.ReadAsStringAsync();

            if (cancellationToken.IsCancellationRequested)
                return new Cancelled();

            var (contractId, signerIds) =
                ContractResultParser.Parse(responseJson);

            foreach (var key in signerIds.Keys)
                signerInfos[key].Signer.SignerId = signerIds[key];

            return new Accepted(contractId,
                signerInfos.Values.Select(si => si.Signer).ToArray());
        }
        catch (Exception error)
        {
            return new Failed(error);
        }
    }
}
