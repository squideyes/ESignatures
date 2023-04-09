// ********************************************************
// The use of this source code is licensed under the terms
// of the MIT License (https://opensource.org/licenses/MIT)
// ********************************************************

namespace ESignatures.Json;

public class ContractSigner: BasicSigner
{
    public required SignerEvent[] Events { get; init; }
    public required Dictionary<string, string> Values { get; init; }
}