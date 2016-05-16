namespace AFT.RegoV2.Core.Payment.Data.Commands
{
    public enum DeactivatePaymentLevelStatus
    {
        CanDeactivate,
        CanDeactivateIsDefault,
        CanDeactivateIsAssigned,
        CannotDeactivateNoReplacement,
        CannotDeactivateStatusInactive
    }
}
