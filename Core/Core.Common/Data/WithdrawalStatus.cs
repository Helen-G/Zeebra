namespace AFT.RegoV2.Domain.Payment.Data
{
    public enum WithdrawalStatus
    {
        Pending,
        Verified,
        Unverified,
        OnHold,
        Accepted,
        Reverted,
        Approved,
        Rejected,
        Canceled
    }
}