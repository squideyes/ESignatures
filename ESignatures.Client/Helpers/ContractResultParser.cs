// ********************************************************
// The use of this source code is licensed under the terms
// of the MIT License (https://opensource.org/licenses/MIT)
// ********************************************************

using SquidEyes.Fundamentals;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ESignatures.Client;

internal class ContractResultParser
{
    public record ContractResult(
        Guid ContractId, Dictionary<string, Guid> SignerIds);

    private class Root
    {
        [JsonPropertyName("status")]
        public required string Status { get; set; }

        [JsonPropertyName("data")]
        public required Data Data { get; init; }
    }

    private class Data
    {
        [JsonPropertyName("contract")]
        public required Contract Contract { get; init; }
    }

    private class Contract
    {
        [JsonPropertyName("id")]
        public required string ContractId { get; init; }

        [JsonPropertyName("signers")]
        public required Signer[] Signers { get; init; }
    }

    private class Signer
    {
        [JsonPropertyName("id")]
        public required string? SignerId { get; init; }

        [JsonPropertyName("name")]
        public required string Name { get; init; }

        [JsonPropertyName("email")]
        public required string Email { get; init; }

        [JsonPropertyName("mobile")]
        public required string Mobile { get; init; }
    }

    internal static ContractResult Parse(string json)
    {
        var root = JsonSerializer.Deserialize<Root>(json);

        var contractId = Guid.Parse(root!.Data.Contract.ContractId);

        var dict = new Dictionary<string, Guid>();

        foreach (var s in root!.Data.Contract.Signers)
        {
            var key = CryptoHelper.GetHash(s.Name, s.Email, s.Mobile);

            dict.Add(key, new Guid(s.SignerId!));
        }

        return new ContractResult(contractId, dict);
    }
}