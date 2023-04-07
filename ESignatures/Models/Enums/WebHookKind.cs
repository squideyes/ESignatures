// ********************************************************
// The use of this source code is licensed under the terms
// of the MIT License (https://opensource.org/licenses/MIT)
// ********************************************************

namespace ESignatures;

public enum WebHookKind
{
    ContractSent = 1,
    ContractSigned,
    ContractWithdrawn,
    MobileUpdate,
    SignerDeclined,
    SignerSigned,
    SignerViewed,
    WebHookError
}