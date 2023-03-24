using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace WebHookDemo;

public class ContractSigned : IWebHookData<ContractSigned>
{
    public Guid ContractId { get; private init; }
    public Metadata? Metadata { get; private init; }
    public Uri? PdfUri { get; private init; }
    public Signer[]? Signers { get; private init; }

    public WebHookDataKind Kind => WebHookDataKind.ContractSigned;

    public static ContractSigned Create(JsonNode? node)
    {
        var data = node.GetData("contract-signed");

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

    public string GetBlobName() => $"Signed/{ContractId:N}.json";

    public Dictionary<string, string> GetMetadata()
    {
        return new Dictionary<string, string>()
        {
            { "ContractId", ContractId.ToString() },
            { "ClientId", Metadata!.ClientId.ToString() },
            { "TrackingId", Metadata!.TrackingId.ToString() },
            { "Signers", Signers.ToJson() },
            { "ContractKind", Metadata!.ContractKind.ToString() }
        };
    }
}
