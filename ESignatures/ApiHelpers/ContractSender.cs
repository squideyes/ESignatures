using OneOf;
using SquidEyes.Basics;
using ESignatures.Internal;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ESignatures;

public class ContractSender
{
    private static readonly Signer.Validator signerValidator = new();
    private static readonly Handling.Validator signingPlanValidator = new();
    private static readonly Address.Validator addressValidator = new();

    public record Accepted(Guid ContractId, Signer[] Signers);
    public record Rejected(HttpStatusCode StatusCode, string ReasonPhrase);
    public record Failed(Exception Error);
    public record Cancelled();

    private static readonly HttpClient client = new();

    private readonly Uri contractsUri;
    private readonly Guid templateId;

    private readonly Dictionary<string, string> metadata = new();
    private readonly Dictionary<string, SignerInfo> signerInfos = new();
    private readonly Dictionary<string, string> placeholders = new();

    private Uri webHookUri = null!;
    private bool isTest = false;
    private Locale locale = Locale.EN;
    private int expiryInHours = 48;
    private string? title = null!;

    public ContractSender(Guid authToken, Guid templateId)
    {
        if (authToken == Guid.Empty)
            throw new ArgumentOutOfRangeException(nameof(authToken));

        if (templateId == Guid.Empty)
            throw new ArgumentOutOfRangeException(nameof(templateId));

        this.templateId = templateId;

        contractsUri = new Uri(
            $"https://esignatures.io/api/contracts?token={authToken}");
    }

    public ContractSender WithTitle(string title)
    {
        if (title is not null && string.IsNullOrWhiteSpace(title))
            throw new ArgumentOutOfRangeException(nameof(title));

        this.title = title!;

        return this;
    }

    public ContractSender WithWebHook(Uri uri)
    {
        if (!uri.IsAbsoluteUri)
            throw new ArgumentOutOfRangeException(nameof(uri));

        webHookUri = uri;

        return this;
    }

    public ContractSender WithExpiryInHours(int hours)
    {
        if (expiryInHours < 1 || expiryInHours > 168)
            throw new ArgumentOutOfRangeException(nameof(expiryInHours));

        expiryInHours = hours;

        return this;
    }

    public ContractSender WithLocale(Locale locale)
    {
        if (locale == default)
            throw new ArgumentNullException(nameof(locale));

        this.locale = locale;

        return this;
    }

    public ContractSender WithMetadata<T>(string token, T value)
    {
        if (!token.IsToken())
            throw new ArgumentOutOfRangeException(nameof(token));

        switch (value)
        {
            case bool _:
            case ClientId _:
            case DateOnly _:
            case DateTime _:
            case double _:
            case Enum _:
            case float _:
            case Guid _:
            case int _:
            case long _:
            case string _:
            case ShortId _:
            case TimeOnly _:
            case TimeSpan _:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(value));
        };

        metadata.Add(token, value.ToString()!);

        return this;
    }

    public ContractSender AsTest()
    {
        isTest = true;

        return this;
    }

    public ContractSender WithDayMonthYear(DateOnly date)
    {
        placeholders.Add("day", date.ToDayName());
        placeholders.Add("month", date.ToMonthName());
        placeholders.Add("year", date.Year.ToString());

        return this;
    }

    public ContractSender WithPlaceholder(string key, object value)
    {
        if (!key.IsApiKey())
            throw new ArgumentOutOfRangeException(nameof(key));

        ArgumentNullException.ThrowIfNull(value);

        placeholders.Add(key, value.ToString()!);

        return this;
    }

    public ContractSender WithSigner(Signer signer, Handling handling,
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
                { "email", signer.Email! },
                { "locality", address!.Locality! },
                { "mobile", signer.Mobile! },
                { "nickname", signer.Nickname! },
                { "name", signer.FullName! },
                { "one-line-address", address!.GetOneLineAddress() },
                { "postal-code", address!.PostalCode! },
                { "region", address!.Region! }
            };

            var prefix = signer.Nickname.ToLower() + "-";

            foreach (var key in dict.Keys)
                WithPlaceholder(prefix + "+" + key, dict[key]);
        }

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
            Email = signer.Email,
            Mobile = signer.Mobile.ToPlusAndDigits(),
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
            if (signerInfos.Count == 0)
            {
                throw new InvalidOperationException(
                    "A contract must have one or more signers!");
            }

            var metadata = this.metadata.Count == 0 ? null! : string.Join(
                '|', this.metadata.Select(kv => kv.Key + "=" + kv.Value));

            var data = new ContractData()
            {
                ExpireHours = expiryInHours,
                IsTest = isTest ? "yes" : "no",
                Locale = locale.ToCode(),
                Metadata = metadata,
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
