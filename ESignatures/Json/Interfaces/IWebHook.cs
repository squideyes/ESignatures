// ********************************************************
// The use of this source code is licensed under the terms
// of the MIT License (https://opensource.org/licenses/MIT)
// ********************************************************

namespace SquidEyes.ESignatures.Json;

public interface IWebHook<T>
    where T : new()
{
    Guid ContractId { get; }
    WebHookKind WebHookKind { get; }
}