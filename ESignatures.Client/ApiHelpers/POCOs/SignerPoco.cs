// ********************************************************
// The use of this source code is licensed under the terms
// of the MIT License (https://opensource.org/licenses/MIT)
// ********************************************************

using System.Text.Json.Serialization;

namespace ESignatures.Client;

internal class SignerPoco
{
    [JsonPropertyName("name")]
    public required string FullName { get; init; }

    [JsonPropertyName("email")]
    public required string Email { get; init; }

    [JsonPropertyName("mobile")]
    public required string Mobile { get; init; }

    [JsonPropertyName("company_name")]
    public required string Company { get; init; }

    [JsonPropertyName("signing_order")]
    public required int Ordinal { get; init; }

    [JsonPropertyName("signature_request_delivery_method")]
    public required string SigReqBy { get; init; }

    [JsonPropertyName("signed_document_delivery_method")]
    public required string GetDocBy { get; init; }

    [JsonPropertyName("required_identification_methods")]
    public required string[] IdModes { get; init; }
}