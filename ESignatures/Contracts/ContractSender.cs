using OneOf;
using SquidEyes.ESignatures.Internal;
using System.Net;
using System.Net.Http.Headers;
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

    private readonly Metadata metadata = new();
    private readonly Dictionary<string, Signer> signers = new();
    private readonly Dictionary<string, string> placeholders = new();

    private Uri webHookUri = null!;
    private bool isTest = false;
    private Locale locale = Locale.EN;

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

    public ContractSender WithWebHook(Uri uri)
    {
        if (!uri.IsAbsoluteUri)
            throw new ArgumentOutOfRangeException(nameof(uri));

        webHookUri = uri;

        return this;
    }

    public ContractSender WithLocale(Locale locale)
    {
        if (locale == default)
            throw new ArgumentNullException(nameof(locale));

        this.locale = locale;

        return this;
    }

    public ContractSender WithMetadata(string key, object value)
    {
        metadata.Add(key, value);

        return this;
    }

    public ContractSender AsTest()
    {
        isTest = true;

        return this;
    }

    public ContractSender AddPlaceholder(string apiKey, object value)
    {
        if (!apiKey.IsApiKey())
            throw new ArgumentOutOfRangeException(nameof(apiKey));

        ArgumentNullException.ThrowIfNull(value);

        placeholders.Add(apiKey, value.ToString()!);

        return this;
    }

    public ContractSender AddSigners(params Signer[] signers)
    {
        foreach (var signer in signers)
            this.signers.Add(signer.GetSha256Hash(), signer);

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

            var data = new ContractData()
            {
                TemplateId = templateId.ToString(),
                IsTest = isTest ? "yes" : "no",
                WebHook = webHookUri?.AbsoluteUri!,
                Locale = locale.ToCode(),
                Metadata = metadata.Count == 0 ? null! : metadata.ToString()
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
