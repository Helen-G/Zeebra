using System;
using System.Collections.Generic;
using System.Linq;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Data.Payment;
using AFT.RegoV2.Core.Common.Events.Payment;
using AFT.RegoV2.Domain.Payment.Commands;
using AFT.RegoV2.Domain.Payment.Data;
using AFT.RegoV2.Domain.Payment.Events;

namespace AFT.RegoV2.Domain.Payment.Entities
{
    /// <summary>
    /// OfflineDepositData has DepositType which says "Online" and "Offline".
    /// Therefore, name of the DTO as well as name of the Entity is inconsistent and should be changed to 'Deposit': Entities.Deposit and Data.Deposit
    /// </summary>
    public class OfflineDeposit
    {
        public OfflineDeposit(Guid id,
            string transactionNumber,
            OfflineDepositRequest request,
            Data.BankAccount bankAccount,
            Guid brandId,
            out Data.OfflineDeposit offlineDeposit)
        {
            if (request.Amount <= 0)
            {
                throw new ArgumentException("Amount must be greater than 0.");
            }
            Data = new Data.OfflineDeposit
            {
                Id = id,
                BrandId = brandId,
                TransactionNumber = transactionNumber,
                Amount = request.Amount,
                PlayerId = request.PlayerId,
                CreatedBy = request.RequestedBy,
                Created = DateTime.Now,
                BankAccountId = bankAccount.Id,
                CurrencyCode = bankAccount.CurrencyCode,
                Status = OfflineDepositStatus.New,
                PaymentMethod = PaymentMethod.OfflineBank,
                DepositType = DepositType.Offline,
                BankAccount = bankAccount,
                DepositWagering = request.Amount,
                PlayerRemark = request.PlayerRemark,
                NotificationMethod = request.NotificationMethod,
                BonusCode = request.BonusCode
            };
            offlineDeposit = Data;
        }

        public OfflineDeposit(Data.OfflineDeposit offlineDeposit)
        {
            Data = offlineDeposit;
            if (Data.Player == null)
                throw new NullReferenceException("Player is not loaded");
        }

        public Data.OfflineDeposit Data { get; private set; }

        public DepositSubmitted Submit()
        {
            var depositSubmitted = new DepositSubmitted
            {
                DepositId = Data.Id,
                PlayerId = Data.PlayerId,
                TransactionNumber = Data.TransactionNumber,
                Amount = Data.Amount,
                Submitted = Data.Created,
                SubmittedBy = Data.CreatedBy,
                CurrencyCode = Data.CurrencyCode,
                PaymentMethod = Data.PaymentMethod,
                DepositType = Data.DepositType,
                Remarks = Data.Remark,
                BonusCode = Data.BonusCode
            };

            if (Data.BankAccount != null)
            {
                var bankAccount = Data.BankAccount;
                depositSubmitted.BankAccountName = bankAccount.AccountName;
                depositSubmitted.BankAccountNumber = bankAccount.AccountNumber;
                depositSubmitted.BankAccountId = bankAccount.AccountId;
                depositSubmitted.BankProvince = bankAccount.Province;
                depositSubmitted.BankBranch = bankAccount.Branch;
                depositSubmitted.BankName = bankAccount.Bank != null ? bankAccount.Bank.Name : null;
            }

            return depositSubmitted;
        }

        public DepositConfirmed Confirm(
            string playerAccountName,
            string playerAccountNumber,
            string referenceNumber,
            decimal amount,
            TransferType transferType,
            DepositMethod depositMethod,
            string remark,
            string idFrontImage = null,
            string idBackImage = null,
            string receiptImage = null)
        {
            if (!IsAccountNameValid(playerAccountName))
            {
                if (string.IsNullOrEmpty(receiptImage) && (string.IsNullOrEmpty(idFrontImage) || string.IsNullOrEmpty(idBackImage)))
                    throw new ArgumentException("Front and back copy of ID or receipt should be uploaded.");
            }

            Data.PlayerAccountName = playerAccountName;
            Data.PlayerAccountNumber = playerAccountNumber;
            Data.ReferenceNumber = referenceNumber;
            Data.Amount = amount;
            Data.TransferType = transferType;
            Data.DepositMethod = depositMethod;
            Data.IdFrontImage = idFrontImage;
            Data.IdBackImage = idBackImage;
            Data.ReceiptImage = receiptImage;
            SetRemark(remark, false);
            ChangeState(OfflineDepositStatus.Processing);
            return new DepositConfirmed(Data);
        }

        public bool IsAccountNameValid(string playerAccountName)
        {
            return
                String.Equals(playerAccountName, Data.Player.FirstName + " " + Data.Player.LastName,
                    StringComparison.InvariantCultureIgnoreCase) ||
                String.Equals(playerAccountName, Data.Player.LastName + " " + Data.Player.FirstName,
                    StringComparison.InvariantCultureIgnoreCase) ||
                String.Equals(playerAccountName, Data.Player.FirstName,
                    StringComparison.InvariantCultureIgnoreCase);
        }

        public DepositVerified Verify(string verifiedBy, string remark)
        {
            Data.Verified = DateTime.Now;
            Data.VerifiedBy = verifiedBy;
            SetRemark(remark);
            ChangeState(OfflineDepositStatus.Verified);
            return new DepositVerified(Data);
        }

        public DepositUnverified Unverify(string unverifiedBy, string remark, UnverifyReasons unverifyReason)
        {
            SetRemark(remark);
            ChangeState(OfflineDepositStatus.Unverified);
            Data.UnverifyReason = unverifyReason;
            return new DepositUnverified
            {
                DepositId = Data.Id,
                PlayerId = Data.Player.Id,
                Status = Data.Status,
                Cancelled = DateTime.Now,
                CancelledBy = unverifiedBy,
                Remarks = Data.Remark,
                UnverifyReason = Data.UnverifyReason.ToString()
            };
        }

        public DepositApproved Approve(decimal actualAmount, decimal fee, string playerRemark, string approveBy, string remark)
        {
            Data.ActualAmount = actualAmount;
            Data.Fee = fee;
            Data.PlayerRemark = playerRemark;
            Data.Approved = DateTime.Now;
            Data.ApprovedBy = approveBy;
            SetRemark(remark);
            ChangeState(OfflineDepositStatus.Approved);
            return new DepositApproved
            {
                DepositId = Data.Id,
                PlayerId = Data.PlayerId,
                ActualAmount = Data.ActualAmount,
                Fee = Data.Fee,
                Approved = (DateTimeOffset)Data.Approved,
                ApprovedBy = Data.ApprovedBy,
                Remarks = Data.Remark,
                DepositWagering = Data.DepositWagering,
            };
        }

        public DepositUnverified Reject(string rejectedBy, string remark)
        {
            SetRemark(remark);
            ChangeState(OfflineDepositStatus.Rejected);
            return new DepositUnverified
            {
                DepositId = Data.Id,
                PlayerId = Data.Player.Id,
                Status = Data.Status,
                Cancelled = DateTime.Now,
                CancelledBy = rejectedBy,
                Remarks = Data.Remark,
                UnverifyReason = Data.UnverifyReason.ToString()
            };
        }

        private void SetRemark(string remark = "", bool isRemarkRequired = true)
        {
            if (isRemarkRequired && string.IsNullOrWhiteSpace(remark))
            {
                throw new ArgumentException(@"Remark is required", "remark");
            }
            Data.Remark = remark;
        }

        private void ChangeState(OfflineDepositStatus newStatus)
        {
            var allowed = _statesMap[Data.Status].Contains(newStatus);
            if (allowed == false)
            {
                throw new InvalidOperationException(
                    string.Format("The deposit has \"{0}\" status, so it can't be {1}", Data.Status, newStatus));
            }

            Data.Status = newStatus;
        }

        private readonly Dictionary<OfflineDepositStatus, IEnumerable<OfflineDepositStatus>> _statesMap
            = new Dictionary<OfflineDepositStatus, IEnumerable<OfflineDepositStatus>>
        {
            { OfflineDepositStatus.New, new[] { OfflineDepositStatus.New, OfflineDepositStatus.Processing} },
            { OfflineDepositStatus.Processing, new[] { OfflineDepositStatus.Verified, OfflineDepositStatus.Unverified } },
            { OfflineDepositStatus.Verified, new[] { OfflineDepositStatus.Rejected, OfflineDepositStatus.Approved } },
            { OfflineDepositStatus.Unverified, new[] { OfflineDepositStatus.Processing} },
            { OfflineDepositStatus.Rejected, Enumerable.Empty<OfflineDepositStatus>() },
            { OfflineDepositStatus.Approved, Enumerable.Empty<OfflineDepositStatus>() },
        };
    }
}