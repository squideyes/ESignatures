using System.Text.Json.Serialization;

namespace ESignatures;

internal class EmailsData
{
    [JsonPropertyName("signature_request_subject")]
    public required string RequestSubject { get; init; }

    [JsonPropertyName("signature_request_text")]
    public required string RequestText { get; init; }

    [JsonPropertyName("final_contract_subject")]
    public required string FinalContractSubject { get; init; }

    [JsonPropertyName("final_contract_text")]
    public required string FinalContractText { get; init; }

    [JsonPropertyName("cc_email_addresses")]
    public required string[] CcEmailAddresses { get; init; }

    [JsonPropertyName("reply_to")]
    public required string ReplyTo { get; init; }
}
