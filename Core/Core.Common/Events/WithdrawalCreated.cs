using System;
using AFT.RegoV2.Core.Common.Interfaces;

namespace AFT.RegoV2.Core.Payment.Events
{
    public class WithdrawalCreated : DomainEventBase
    {
        // default constructor is required for publishing event to MQ

        #region Properties

        public Guid PlayerId { get; set; }
        public string TransactionNumber { get; set; }
        public decimal Amount { get; set; }
        public DateTimeOffset Created { get; set; }
        public string CreatedBy { get; set; }

        #endregion

        #region Constructors

        public WithdrawalCreated()
        {
        }

        #endregion
    }
}