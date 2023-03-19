namespace WebHookDemo;

public enum WebHookDataKind
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
