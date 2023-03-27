using SharedModels;
using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace WebHookProcessor;

internal static class WebHookParser
{
    public static ContractSent ParseContractSent(this JsonNode? node) =>
        ParseContractSigner<ContractSent>(node, "contract-sent-to-signer");

    public static SignerViewed ParseSignerViewed(this JsonNode? node) =>
        ParseContractSigner<SignerViewed>(node, "signer-viewed-the-contract");

    public static SignerSigned ParseSignerSigned(this JsonNode? node) =>
        ParseContractSigner<SignerSigned>(node, "signer-signed");

    public static SignerDeclined ParseSignerDeclined(this JsonNode? node) =>
        ParseContractSigner<SignerDeclined>(node, "signer-declined");

    public static ContractWithdrawn ParseContractWithdrawn(JsonNode? node)
    {
        var data = node.GetDataIfStatusIs("contract-withdrawn");

        var contract = data!["contract"]!;

        var signers = new List<Signer>();

        foreach (var s in contract!["signers"]!.AsArray())
        {
            signers.Add(new Signer()
            {
                SignerId = Guid.Parse(s.GetString("id")),
                Name = s.GetString("name"),
                Email = s.GetString("email"),
                Mobile = s.GetString("mobile"),
            });
        }

        return new ContractWithdrawn()
        {
            ContractId = Guid.Parse(data.GetString("contract_id")),
            Metadata = Metadata.Parse(contract.GetString("metadata")),
            Signers = signers.ToArray()
        };
    }

    public static MobileUpdate ParseMobileUpdate(JsonNode? node)
    {
        var data = node.GetDataIfStatusIs("signer-mobile-update-request");

        var contract = data!["contract"]!;

        var signer = data!["signer"]!;

        return new MobileUpdate()
        {
            ContractId = Guid.Parse(contract.GetString("id")),
            Metadata = Metadata.Parse(contract.GetString("metadata")),
            Signer = new Signer()
            {
                SignerId = Guid.Parse(signer.GetString("id")),
                Name = signer.GetString("name"),
                Email = signer.GetString("email"),
                Mobile = signer.GetString("mobile")
            },
            NewMobile = signer.GetString("mobile_new")
        };
    }
    
    public static WebHookError ParseWebHookError(JsonNode? node)
    {
        var data = node.GetDataIfStatusIs("error");

        return new WebHookError()
        {
            ErrorCode = data.GetString("error_code"),
            Message = data.GetString("error_message"),
            ContractId = Guid.Parse(data.GetString("contract_id")),
            Metadata = Metadata.Parse(data.GetString("metadata"))
        };
    }

    public static ContractSigned ParseContractSigned(JsonNode? node)
    {
        var data = node.GetDataIfStatusIs("contract-signed");

        var contract = data!["contract"]!;

        var signers = new List<Signer>();

        foreach (var s in contract!["signers"]!.AsArray())
        {
            var events = new List<SignerEvent>();

            foreach (var e in s!["events"]!.AsArray())
            {
                events.Add(new SignerEvent()
                {
                    Kind = e.GetString("event").ToSignerEventKind(),
                    TimeStamp = DateTime.Parse(e.GetString("timestamp"))
                });
            }

            Dictionary<string, string> fieldValues = null!;

            var sfv = s!["signer_field_values"];

            if (sfv != null)
                JsonSerializer.Deserialize<Dictionary<string, string>>(sfv);

            signers.Add(new Signer()
            {
                SignerId = Guid.Parse(s.GetString("id")),
                Name = s.GetString("name"),
                Email = s.GetString("email"),
                Mobile = s.GetString("mobile"),
                Events = events,
                FieldValues = fieldValues
            });
        }

        return new ContractSigned()
        {
            ContractId = Guid.Parse(contract.GetString("id")),
            Metadata = Metadata.Parse(contract.GetString("metadata")),
            PdfUri = new Uri(WebUtility.UrlDecode(
                contract.GetString("contract_pdf_url"))),
            Signers = signers.ToArray()
        };
    }

    private static T ParseContractSigner<T>(JsonNode? node, string status)
        where T : IContractSigner, new()
    {
        var data = node.GetDataIfStatusIs(status);

        var contract = data!["contract"]!;

        var signer = data!["signer"]!;

        return new T()
        {
            ContractId = Guid.Parse(contract.GetString("id")),
            Signer = new Signer()
            {
                SignerId = Guid.Parse(signer.GetString("id")),
                Name = signer.GetString("name"),
                Email = signer.GetString("email"),
                Mobile = signer.GetString("mobile")
            },
            Metadata = Metadata.Parse(contract.GetString("metadata"))
        };
    }
}
