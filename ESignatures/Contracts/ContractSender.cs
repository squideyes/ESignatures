using OneOf;
using SquidEyes.ESignatures.Internal;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SquidEyes.ESignatures;

public class ContractSender
{
    public record Accepted(Guid ContractId, Signer[] Signers);
    public record Rejected(HttpStatusCode StatusCode, string ReasonPhrase);
    public record Failed(Exception Error);
    public record Cancelled();

    private static readonly HttpClient client = new();

    private readonly Uri contractsUri;
    private readonly Guid templateId;

    private readonly Dictionary<string, TagValue> tagValues = new();
    private readonly Dictionary<string, Signer> signers = new();
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

    public ContractSender WithMetadata<T>(string tag, T value)
    {
        tagValues.Add(tag, TagValue.Create(tag, value));

        return this;
    }

    public ContractSender AsTest()
    {
        isTest = true;

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

    public ContractSender WithSigner(
        Signer signer, bool withPlacholders = true)
    {
        signers.Add(signer.GetSha256Hash(), signer);

        if (withPlacholders)
        {
            var placeholders = signer.AsPlaceholders();

            foreach (var key in placeholders.Keys)
                WithPlaceholder(key, placeholders[key]);
        }

        return this;
    }

    private static SignerData GetSignerData(Signer signer)
    {
        var idModes = (signer.IdByEmail, signer.IdBySms) switch
        {
            (true, true) => new string[] { "email", "sms" },
            (true, false) => new string[] { "email" },
            (false, true) => new string[] { "sms" },
            (false, false) => null!
        };

        return new SignerData()
        {
            Name = signer.Name,
            Email = signer.Email,
            Mobile = signer.Mobile.ToPlusAndDigits(),
            Company = signer.Company!,
            Ordinal = signer.Ordinal,
            GetDocBy = signer.GetDocBy.ToString().ToLower(),
            IdModes = idModes,
            SigReqBy = signer.SigReqBy.ToString().ToLower()
        };
    }

    public async Task<OneOf<Accepted, Rejected, Failed, Cancelled>> SendAsync(
        CancellationToken cancellationToken)
    {
        try
        {
            if (signers.Count == 0)
            {
                throw new InvalidOperationException(
                    "A contract must have one or more signers!");
            }

            var sb = new StringBuilder();

            foreach (var tagValue in tagValues)
            {
                if (sb.Length > 0)
                    sb.Append('|');

                sb.Append(tagValue.Value.ToString());
            }

            var data = new ContractData()
            {
                TemplateId = templateId.ToString(),
                Title = title!,
                IsTest = isTest ? "yes" : "no",
                WebHook = webHookUri?.AbsoluteUri!,
                ExpireHours = expiryInHours,
                Locale = locale.ToCode(),
                Metadata = sb.Length == 0 ? null! : sb.ToString()
            };

            if (signers.Count > 0)
            {
                data.Signers = new List<SignerData>();

                foreach (var signer in signers.Values)
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

            var (contractId, signerIds) = ContractResultParser.Parse(responseJson);

            foreach (var key in signerIds.Keys)
                signers[key].SignerId = signerIds[key];

            return new Accepted(contractId, signers.Values.ToArray());
        }
        catch (Exception error)
        {
            return new Failed(error);
        }
    }
}
