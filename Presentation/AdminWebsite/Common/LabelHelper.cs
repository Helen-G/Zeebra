using System;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Data.Payment;
using AFT.RegoV2.Domain.Payment.Data;

namespace AFT.RegoV2.AdminWebsite.Common
{
    public class LabelHelper
    {
        public static string LabelPaymentType(PaymentType paymentType)
        {
            switch (paymentType)
            {
                case PaymentType.Deposit:
                    return "Deposit";
                case PaymentType.Withdraw:
                    return "Withdraw";
                default:
                    throw new ArgumentOutOfRangeException("paymentType");
            }
        }

        public static string LabelTransferType(TransferFundType transferFundType)
        {
            switch (transferFundType)
            {
                case TransferFundType.FundIn:
                    return "Fund In";
                case TransferFundType.FundOut:
                    return "Fund Out";
                default:
                    throw new ArgumentOutOfRangeException("transferFundType");
            }
        }

        public static string LabelTransferStatus(TransferFundStatus transferFundStatus)
        {
            switch (transferFundStatus)
            {
                case TransferFundStatus.Approved:
                    return "Success";
                case TransferFundStatus.Rejected:
                    return "Failed";
                default:
                    throw new ArgumentOutOfRangeException("transferFundStatus");
            }
        }

        public static string LabelStatus(bool status)
        {
            return status ? "Active" : "Inactive";
        }

        public static string LabelPaymentMethod(PaymentMethod paymentMethod)
        {
            return paymentMethod == PaymentMethod.OfflineBank ? "Offline-Bank" : paymentMethod.ToString();
        }

        public static string LabelOfflineDepositType(DepositMethod depositMethod)
        {
            switch (depositMethod)
            {
                case DepositMethod.CounterDeposit:
                    return "Counter Deposit";
                case DepositMethod.InternetBanking:
                    return "Internet Banking";
                default:
                    return depositMethod.ToString("F");
            }
        }
    }
}