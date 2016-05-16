namespace AFT.RegoV2.Core.Payment.Validators
{
    public enum DeactivatePaymentLevelErrors
    {
        Requred,
        NotFound,
        NotActive,
        NewPaymentLevelRequired,
        NewPaymentLevelNotFound,        
        NewPaymentLevelNotActive
    }
}