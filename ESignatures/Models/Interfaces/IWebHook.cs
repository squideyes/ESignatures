// ********************************************************
// The use of this source code is licensed under the terms
// of the MIT License (https://opensource.org/licenses/MIT)
// ********************************************************

namespace ESignatures;

public interface IWebHook<T>
    where T : new()
{
    Guid ContractId { get; }
    WebHookKind WebHookKind { get; }
}