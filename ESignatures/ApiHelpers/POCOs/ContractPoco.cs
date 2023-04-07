using System.Text.Json.Serialization;

namespace ESignatures;

internal class ContractPoco
{
    [JsonPropertyName("template_id")]
    public required string TemplateId { get; init; }

    [JsonPropertyName("title")]
    public required string Title { get; init; }

    [JsonPropertyName("expires_in_hours")]
    public required int ExpireHours { get; init; }

    [JsonPropertyName("locale")]
    public required string Locale { get; init; }

    [JsonPropertyName("metadata")]
    public required string Metadata { get; init; }

    [JsonPropertyName("emails")]
    public EmailsPoco? Emails { get; init; }

    [JsonPropertyName("custom_branding")]
    public BrandingPoco? Branding { get; init; }

    [JsonPropertyName("signers")]
    public required List<SignerPoco>? Signers { get; set; }

    [JsonPropertyName("placeholder_fields")]
    public required List<PlaceholderPoco>? Placeholders { get; set; }

    [JsonPropertyName("test")]
    public required string IsTest { get; init; }

    [JsonPropertyName("custom_webhook_url")]
    public required string WebHook { get; init; }
}
