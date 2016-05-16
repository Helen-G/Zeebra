using System;
using System.Collections.Generic;
using AFT.RegoV2.Shared;

namespace AFT.RegoV2.MemberApi.Interface.Payment
{
    public class OfflineDepositFormDataRequest 
    {
        public Guid BrandId { get; set; }
    }

    public class OfflineDepositFormDataResponse
    {
        public Dictionary<Guid, string> BankAccounts { get; set; }
    }
    

    public class OfflineDepositRequest 
    {
        public Guid BankAccountId { get; set; }
        public decimal Amount { get; set; }
        public string PlayerRemarks { get; set; }
        public string NotificationMethod { get; set; }
        public string BankTime { get; set; }
        public string BankAccountTime { get; set; }
        public string BonusCode { get; set; }
    }

    public class OfflineDepositResponse
    {
    }

    public class PendingDepositsRequest
    {
    }

    public class PendingDepositsResponse
    {
        public IEnumerable<OfflineDeposit> PendingDeposits { get; set; }
    }

    public class OfflineDeposit
    {
        public Guid Id { get; set; }
        public string ReferenceCode { get; set; }
        public string DateCreated { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; }
        public string DepositType { get; set; }

        public string TransferType { get; set; }

        public byte[] IdBack { get; set; }
        public byte[] IdFront { get; set; }
        public byte[] Receipt { get; set; }
    }

    public class GetOfflineDepositRequest
    {
        public Guid Id { get; set; }
    }
}
