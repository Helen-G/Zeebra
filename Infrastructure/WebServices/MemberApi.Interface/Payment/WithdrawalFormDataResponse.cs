using System;
using AFT.RegoV2.Shared;

namespace AFT.RegoV2.MemberApi.Interface.Payment
{
    public class WithdrawalFormDataRequest
    {
        public Guid BrandId { get; set; }
    }

    public class WithdrawalFormDataResponse
    {
        public BankData BankAccount { get; set; }
    }

    public class OfflineWithdrawalRequest
    {
        public Guid PlayerBankAccountId { get; set; }

        public decimal Amount { get; set; }

        public string NotificationType { get; set; }

        public string BankAccountTime { get; set; }

        public string BankTime { get; set; }
    }

    public class OfflineWithdrawalResponse
    {
        public string Result { get; set; }
        public string[] Errors { get; set; }
    }
}
