namespace WebHookDemo;

[Flags]
internal enum ContractEventsToSend
{
    None = 0,
    All = ~(-1 << 8),
    ContractSent = 1 << 0,
    ContractSigned = 1 << 1,
    ContractWithdrawn = 1 << 2,
    SignerViewed = 1 << 3,
    SignerSigned = 1 << 4,
    SignerDeclined = 1 << 5,
    MobileUpdate = 1 << 6,
    WebHookError = 1 << 7
}
