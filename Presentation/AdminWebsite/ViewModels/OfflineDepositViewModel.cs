using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AFT.RegoV2.AdminWebsite.Common;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Utils;
using AFT.RegoV2.Domain.Payment.Data;
using ServiceStack.Common;

namespace AFT.RegoV2.AdminWebsite.ViewModels
{
    public class OfflineDepositViewModel
    {
        public OfflineDepositViewModel(OfflineDeposit offlineDeposit)
        {
            this.Licensee = offlineDeposit.Brand.LicenseeName;
            this.Brand = offlineDeposit.Brand.Name;
            this.Username = offlineDeposit.Player.Username;
            this.FirstName = offlineDeposit.Player.FirstName;
            this.LastName = offlineDeposit.Player.LastName;
            this.PlayerName = offlineDeposit.Player.GetFullName();
            this.TransactionNumber = offlineDeposit.TransactionNumber;
            this.Status = offlineDeposit.Status.ToString();
            this.PlayerAccountName = offlineDeposit.PlayerAccountName;
            this.PlayerAccountNumber = offlineDeposit.PlayerAccountNumber;
            this.ReferenceNumber = offlineDeposit.ReferenceNumber;
            this.Amount = offlineDeposit.Amount;
            this.CurrencyCode = offlineDeposit.CurrencyCode;
            this.BankName = offlineDeposit.BankAccount.Bank.Name;
            this.BankAccountId = offlineDeposit.BankAccount.AccountId;
            this.BankAccountName = offlineDeposit.BankAccount.AccountName;
            this.BankAccountNumber = offlineDeposit.BankAccount.AccountNumber;
            this.BankProvince = offlineDeposit.BankAccount.Province;
            this.BankBranch = offlineDeposit.BankAccount.Branch;
            this.TransferType = offlineDeposit.TransferType.ToString("F");
            this.DepositType = offlineDeposit.DepositType.ToString("F");
            this.OfflineDepositType = LabelHelper.LabelOfflineDepositType(offlineDeposit.DepositMethod);
            this.PaymentMethod = LabelHelper.LabelPaymentMethod(offlineDeposit.PaymentMethod);
            this.Remark = offlineDeposit.Remark;
            this.PlayerRemark = offlineDeposit.PlayerRemark;
            this.Created = offlineDeposit.Created.GetNormalizedDateTime();
            this.CreatedBy = offlineDeposit.CreatedBy;
            this.Verified = offlineDeposit.Verified.HasValue ? offlineDeposit.Verified.Value.GetNormalizedDateTime() : "";
            this.VerifiedBy = offlineDeposit.VerifiedBy;
            this.IdFrontImage = offlineDeposit.IdFrontImage;
            this.IdBackImage = offlineDeposit.IdBackImage;
            this.ReceiptImage = "";
            this.UnverifyReason = offlineDeposit.UnverifyReason.HasValue
                ? GetUnverifyReasons().Single(o => o.Key == offlineDeposit.UnverifyReason.ToString()).Value
                : string.Empty;
            this.UnverifyReasons = GetUnverifyReasons().Select(o => new
            {
                Code = o.Key,
                Message = o.Value
            });
            NotificationMethod = offlineDeposit.NotificationMethod.ToString();
        }

        private Dictionary<string, string> GetUnverifyReasons()
        {
            var enumType = typeof(UnverifyReasons);

            return Enum.GetValues(enumType)
                .Cast<UnverifyReasons>()
                .ToDictionary(
                    value => value.ToString(),
                    value =>
                    {
                        var descr = value.ToDescription();
                        return descr;
                    });
        }

        public string Licensee { get; set; }
        public string Brand { get; set; }
        public string Username { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PlayerName { get; set; }
        public string TransactionNumber { get; set; }
        public string Status { get; set; }
        public string PlayerAccountName { get; set; }
        public string PlayerAccountNumber { get; set; }
        public string ReferenceNumber { get; set; }
        public decimal Amount { get; set; }
        public string CurrencyCode { get; set; }
        public string BankName { get; set; }
        public string BankAccountId { get; set; }
        public string BankAccountName { get; set; }
        public string BankAccountNumber { get; set; }
        public string BankProvince { get; set; }
        public string BankBranch { get; set; }
        public string TransferType { get; set; }
        public string DepositType { get; set; }
        public string OfflineDepositType { get; set; }
        public string PaymentMethod { get; set; }
        public string Remark { get; set; }
        public string PlayerRemark { get; set; }
        public string Created { get; set; }
        public string CreatedBy { get; set; }
        public string Verified { get; set; }
        public string VerifiedBy { get; set; }
        public string IdFrontImage { get; set; }
        public string IdBackImage { get; set; }
        public string ReceiptImage { get; set; }
        public string NotificationMethod { get; set; }
        public string BonusName { get; set; }
        public IEnumerable UnverifyReasons { get; set; }
        public string UnverifyReason { get; set; }
    }
}