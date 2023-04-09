// ********************************************************
// The use of this source code is licensed under the terms
// of the MIT License (https://opensource.org/licenses/MIT)
// ********************************************************

using System.Text.Json.Serialization;

namespace SquidEyes.ESignatures.Client;

internal class EmailsPoco
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