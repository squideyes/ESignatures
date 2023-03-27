namespace SharedModels
{
    public enum SignerEventKind
    {
        ContractViewed = 1,
        DisableReminders,
        EmailContractSent,
        EmailDeliveryFailed,
        EmailFinalContractSent,
        EmailSpamComplaint,
        MobileUpdateRequest,
        ReminderEmailed,
        SignContract,
        SignatureDeclined,
        SmsContractSent,
        SmsDeliveryFailed,
        SmsFinalContractSent
    }
}
