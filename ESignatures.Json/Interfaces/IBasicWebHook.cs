// ********************************************************
// The use of this source code is licensed under the terms
// of the MIT License (https://opensource.org/licenses/MIT)
// ********************************************************

namespace ESignatures.Json;

public interface IBasicWebHook<M>
    where M : class
{
    Guid ContractId { get; set; }
    M? Metadata { get; set; }
    Signer? Signer { get; set; }
}