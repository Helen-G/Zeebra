using AFT.RegoV2.Domain.Payment.Commands;

namespace AFT.RegoV2.MemberApi.Interface.Common
{
    public class OfflineDepositConfirmRequest
    {
        public OfflineDepositConfirm DepositConfirm { get; set; }
        public byte[] IdFrontImage { get; set; }
        public byte[] IdBackImage { get; set; }
        public byte[] ReceiptImage { get; set; }
    }
}
