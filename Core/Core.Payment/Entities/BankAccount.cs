using System;
using System.Collections.ObjectModel;
using AFT.RegoV2.Domain.Payment.Data;

namespace AFT.RegoV2.Domain.Payment.Entities
{
    public class BankAccount
    {
        private readonly Data.BankAccount _data;

        public BankAccount(Data.BankAccount data)
        {
            _data = data;
        }

        public void Activate(string user, string remarks)
        {
            _data.Updated = DateTime.Now;
            _data.UpdatedBy = user;
            _data.Remarks = remarks;
            _data.Status = BankAccountStatus.Active;
        }

        public void Deactivate(string user, string remarks)
        {
            _data.PaymentLevels = new Collection<PaymentLevel>();
            _data.Updated = DateTime.Now;
            _data.UpdatedBy = user;
            _data.Remarks = remarks;
            _data.Status = BankAccountStatus.Pending;
        }
    }
}