using System.Text.Json.Serialization;

namespace ESignatures;

internal class EmailsData
{
    [JsonPropertyName("signature_request_subject")]
    public string? RequestSubject { get; set; }

    [JsonPropertyName("signature_request_text")]
    public string? RequestBodyText { get; set; }

    [JsonPropertyName("final_contract_subject")]
    public string? ContractSubject { get; set; }

    [JsonPropertyName("final_contract_text")]
    public string? ContractBodyText { get; set; }

    [JsonPropertyName("reply_to")]
    public string? ReplyTo { get; set; }

    [JsonPropertyName("cc_email_addresses")]
    public string[]? CcPdfsTo { get; set; }
}
