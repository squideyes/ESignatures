// ********************************************************
// The use of this source code is licensed under the terms
// of the MIT License (https://opensource.org/licenses/MIT)
// ********************************************************

using OneOf;
using SquidEyes.Fundamentals;
using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace SquidEyes.ESignatures.Json;

public static class JsonHelper
{
    public static OneOf<
        ContractSent<M>,
        ContractSigned<M>,
        ContractWithdrawn<M>,
        MobileUpdate<M>,
        SignerDeclined<M>,
        SignerSigned<M>,
        SignerViewed<M>,
        WebHookError<M>>
        Parse<M>(string json, Func<string, M> getMetadata)
        where M : class
    {
        var node = JsonNode.Parse(json!)!;

        return node.GetString("status") switch
        {
            "contract-sent-to-signer" => node!.ParseContractSent(getMetadata),
            "contract-signed" => node!.ParseContractSigned(getMetadata),
            "contract-withdrawn" => node!.ParseContractWithdrawn(getMetadata),
            "signer-mobile-update-request" => node!.ParseMobileUpdate(getMetadata),
            "signer-declined" => node!.ParseSignerDeclined(getMetadata),
            "signer-signed" => node!.ParseSignerSigned(getMetadata),
            "signer-viewed-the-contract" => node!.ParseSignerViewed(getMetadata),
            "error" => node!.ParseWebHookError(getMetadata),
            _ => throw new ArgumentOutOfRangeException("status"),
        };
    }

    public static ContractSent<M> ParseContractSent<M>(
        this JsonNode? node, Func<string, M> getMetadata)
        where M : class
    {
        return ParseBasicWebhook<M, ContractSent<M>>(
            node, "contract-sent-to-signer", getMetadata);
    }

    public static SignerViewed<M> ParseSignerViewed<M>(
        this JsonNode? node, Func<string, M> getMetadata)
        where M : class
    {
        return ParseBasicWebhook<M, SignerViewed<M>>(
            node, "signer-viewed-the-contract", getMetadata);
    }

    public static SignerSigned<M> ParseSignerSigned<M>(
        this JsonNode? node, Func<string, M> getMetadata)
        where M : class
    {
        return ParseBasicWebhook<M, SignerSigned<M>>(
            node, "signer-signed", getMetadata);
    }

    public static SignerDeclined<M> ParseSignerDeclined<M>(
        this JsonNode? node, Func<string, M> getMetadata)
        where M : class
    {
        return ParseBasicWebhook<M, SignerDeclined<M>>(
            node, "signer-declined", getMetadata);
    }

    private static T ParseBasicWebhook<M, T>(
        JsonNode? node, string status, Func<string, M> getMetadata)
        where M : class
        where T : IBasicWebHook<M>, new()
    {
        var data = node.GetDataIfStatusIs(status);

        var contract = data!["contract"]!;

        var signer = data!["signer"]!;

        return new T()
        {
            ContractId = Guid.Parse(contract.GetString("id")),
            Signer = new BasicSigner()
            {
                SignerId = Guid.Parse(signer.GetString("id")),
                FullName = signer.GetString("name"),
                Email = Email.From(signer.GetString("email")),
                Mobile = Phone.From(signer.GetString("mobile"))
            },
            Metadata = getMetadata(contract.GetString("metadata"))
        };
    }

    public static ContractWithdrawn<M> ParseContractWithdrawn<M>(
        this JsonNode? node, Func<string, M> getMetadata)
        where M : class
    {
        var data = node.GetDataIfStatusIs("contract-withdrawn");

        var contract = data!["contract"]!;

        var signers = new List<BasicSigner>();

        foreach (var s in contract!["signers"]!.AsArray())
        {
            signers.Add(new BasicSigner()
            {
                SignerId = Guid.Parse(s.GetString("id")),
                FullName = s.GetString("name"),
                Email = Email.From(s.GetString("email")),
                Mobile = Phone.From(s.GetString("mobile"))
            });
        }

        return new ContractWithdrawn<M>()
        {
            ContractId = Guid.Parse(data.GetString("contract_id")),
            Metadata = getMetadata(contract.GetString("metadata")),
            Signers = signers.ToArray()
        };
    }

    public static MobileUpdate<M> ParseMobileUpdate<M>(
        this JsonNode? node, Func<string, M> getMetadata)
        where M : class
    {
        var data = node.GetDataIfStatusIs("signer-mobile-update-request");

        var contract = data!["contract"]!;

        var signer = data!["signer"]!;

        return new MobileUpdate<M>()
        {
            ContractId = Guid.Parse(contract.GetString("id")),
            Metadata = getMetadata(contract.GetString("metadata")),
            Signer = new BasicSigner()
            {
                SignerId = Guid.Parse(signer.GetString("id")),
                FullName = signer.GetString("name"),
                Email = Email.From(signer.GetString("email")),
                Mobile = Phone.From(signer.GetString("mobile"))
            },
            NewMobile = signer.GetString("mobile_new")
        };
    }

    public static WebHookError<M> ParseWebHookError<M>(
        this JsonNode? node, Func<string, M> getMetadata)
        where M : class
    {
        var data = node.GetDataIfStatusIs("error");

        return new WebHookError<M>()
        {
            ErrorCode = data.GetString("error_code"),
            Message = data.GetString("error_message"),
            ContractId = Guid.Parse(data.GetString("contract_id")),
            Metadata = getMetadata(data.GetString("metadata"))
        };
    }

    public static ContractSigned<M> ParseContractSigned<M>(
        this JsonNode? node, Func<string, M> getMetadata)
        where M: class
    {
        var data = node.GetDataIfStatusIs("contract-signed");

        var contract = data!["contract"]!;

        var signers = new List<BasicSigner>();

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

            Dictionary<string, string> values = null!;

            var sfv = s!["signer_field_values"];

            if (sfv != null)
                values = JsonSerializer.Deserialize<Dictionary<string, string>>(sfv)!;

            signers.Add(new ContractSigner()
            {
                SignerId = Guid.Parse(s.GetString("id")),
                FullName = s.GetString("name"),
                Email = Email.From(s.GetString("email")),
                Mobile = Phone.From(s.GetString("mobile")),
                Events = events.ToArray(),
                Values = values
            });
        }

        return new ContractSigned<M>()
        {
            ContractId = Guid.Parse(contract.GetString("id")),
            Metadata = getMetadata(contract.GetString("metadata")),
            PdfUri = new Uri(WebUtility.UrlDecode(
                contract.GetString("contract_pdf_url"))),
            Signers = signers.ToArray()
        };
    }

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