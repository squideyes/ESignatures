using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace ESignatures;

public static class JsonNodeExtenders
{
    public static ContractSent ParseContractSent(this JsonNode? node) =>
        ParseContractSigner<ContractSent>(node, "contract-sent-to-signer");

    public static SignerViewed ParseSignerViewed(this JsonNode? node) =>
        ParseContractSigner<SignerViewed>(node, "signer-viewed-the-contract");

    public static SignerSigned ParseSignerSigned(this JsonNode? node) =>
        ParseContractSigner<SignerSigned>(node, "signer-signed");

    public static SignerDeclined ParseSignerDeclined(this JsonNode? node) =>
        ParseContractSigner<SignerDeclined>(node, "signer-declined");

    public static ContractWithdrawn ParseContractWithdrawn(this JsonNode? node)
    {
        var data = node.GetDataIfStatusIs("contract-withdrawn");

        var contract = data!["contract"]!;

        var signers = new List<Signer>();

        foreach (var s in contract!["signers"]!.AsArray())
        {
            signers.Add(new Signer()
            {
                SignerId = Guid.Parse(s.GetString("id")),
                FullName = s.GetString("name"),
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

    public static MobileUpdate ParseMobileUpdate(this JsonNode? node)
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
    
    public static WebHookError ParseWebHookError(this JsonNode? node)
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

    public static ContractSigned ParseContractSigned(this JsonNode? node)
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

    public static WebHookKind GetWebHoodKind(JsonNode? node)
    {
        return node.GetString("status") switch
        {
            "contract-sent-to-signer" => WebHookKind.ContractSent,
            "contract-signed" => WebHookKind.ContractSigned,
            "contract-withdrawn" => WebHookKind.ContractWithdrawn,
            "signer-mobile-update-request" => WebHookKind.MobileUpdate,
            "signer-declined" => WebHookKind.SignerDeclined,
            "signer-signed" => WebHookKind.SignerSigned,
            "signer-viewed-the-contract" => WebHookKind.SignerViewed,
            "error" => WebHookKind.WebHookError,
            _ => throw new ArgumentOutOfRangeException(nameof(node)),
        };
    }

    private static T ParseContractSigner<T>(JsonNode? node, string status)
        where T : IBasicWebHook, new()
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

    private static JsonNode? GetDataIfStatusIs(this JsonNode? node, string status)
    {
        var actual = node.GetString("status");

        if (actual != status)
        {
            throw new InvalidDataException(
                $"\"{actual}\" JSON received when \"{status}\" was expected!");
        }

        return node!["data"];
    }

    private static string GetString(this JsonNode? node, string propertyName) =>
        (string)node![propertyName]!;

    private static SignerEventKind ToSignerEventKind(this string value)
    {
        return value switch
        {
            "contract_viewed" => SignerEventKind.ContractViewed,
            "disable_reminders" => SignerEventKind.DisableReminders,
            "email_contract_sent" => SignerEventKind.EmailContractSent,
            "email_delivery_failed" => SignerEventKind.EmailDeliveryFailed,
            "email_final_contract_sent" => SignerEventKind.EmailFinalContractSent,
            "email_spam_complaint" => SignerEventKind.EmailSpamComplaint,
            "mobile_update_request" => SignerEventKind.MobileUpdateRequest,
            "reminder_emailed" => SignerEventKind.ReminderEmailed,
            "sign_contract" => SignerEventKind.SignContract,
            "signature_declined" => SignerEventKind.SignatureDeclined,
            "sms_contract_sent" => SignerEventKind.SmsContractSent,
            "sms_delivery_failed" => SignerEventKind.SmsDeliveryFailed,
            "sms_final_contract_sent" => SignerEventKind.SmsFinalContractSent,
            _ => throw new ArgumentOutOfRangeException(nameof(value))
        };
    }
}
