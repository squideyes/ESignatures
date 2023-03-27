namespace SharedModels;

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
