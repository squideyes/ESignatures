// ********************************************************
// The use of this source code is licensed under the terms
// of the MIT License (https://opensource.org/licenses/MIT)
// ********************************************************

namespace SquidEyes.ESignatures.WebHook;

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