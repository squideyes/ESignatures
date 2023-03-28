using Microsoft.Azure.Functions.Worker.Http;
using SharedModels;
using System.Text.Json.Nodes;
using System.Threading;

namespace WebHookProcessor;

internal static class MiscExtenders
{
    public static R Get<T, R>(this T value, Func<T, R> convert) => convert(value);

    public static string TryGetHeaderValue(this HttpRequestData request, string name)
    {
        return request.Headers.TryGetValues(name, out var values)
            ? values.FirstOrDefault()! : null!;
    }

    public static string GetString(this JsonNode? node, string propertyName) =>
        (string)node![propertyName]!;

    public static JsonNode? GetDataIfStatusIs(this JsonNode? node, string status)
    {
        var actual = node.GetString("status");

        if (actual != status)
        {
            throw new InvalidDataException(
                $"\"{actual}\" JSON received when \"{status}\" was expected!");
        }

        return node!["data"];
    }

    public static SignerEventKind ToSignerEventKind(this string value)
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
