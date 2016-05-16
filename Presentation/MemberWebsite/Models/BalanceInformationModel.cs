using AFT.RegoV2.MemberApi.Interface.Payment;
using AFT.RegoV2.MemberApi.Interface.Player;

namespace AFT.RegoV2.MemberWebsite.Models
{
    public class BalanceInformationModel
    {
        public BalancesResponse Balances { get; set; }
        public OfflineDepositFormDataResponse OfflineDeposit { get; set; }
        public FundTransferFormDataResponse FundTransfer { get; set; }
        public WithdrawalFormDataResponse Withdrawal { get; set; }
        public PendingDepositsResponse PendingDeposits { get; set; }
    }
}