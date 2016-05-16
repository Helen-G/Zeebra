using System;
using System.Collections.Generic;
using AFT.RegoV2.Domain.Payment.Data;

namespace AFT.RegoV2.Core.Fraud.Data
{
    public class AVCConfigurationDTO
    {
        #region Properties

        public Guid Id { get; set; }
        public Guid Licensee { get; set; }
        public Guid Brand { get; set; }
        public string Currency { get; set; }
        public Guid VipLevel { get; set; }
        public bool HasFraudRiskLevel { get; set; }
        
        public bool AllowWithdrawalExemption { get; set; }
        public bool HasNoRecentBonus { get; set; }

        //HasWinnings criteria
        public bool HasWinnings { get; set; }
        public bool HasCompleteDocuments { get; set; }

        //WinLoss criteria
        public bool HasWinLoss { get; set; }
        public decimal WinLossAmount { get; set; }
        public ComparisonEnum WinLossOperator { get; set; }

        //Total deposit amount criteria
        public bool HasTotalDepositAmount { get; set; }
        public decimal TotalDepositAmount { get; set; }
        public ComparisonEnum TotalDepositAmountOperator { get; set; }

        //Total deposit count criteria
        public bool HasDepositCount { get; set; }
        public int TotalDepositCountAmount { get; set; }
        public ComparisonEnum TotalDepositCountOperator { get; set; }

        //Total withdrawal count criteria
        public bool HasWithdrawalCount { get; set; }
        public decimal TotalWithdrawalCountAmount { get; set; }
        public ComparisonEnum TotalWithdrawalCountOperator { get; set; }

        //Total withdrawal count criteria
        public bool HasAccountAge { get; set; }
        public int AccountAge { get; set; }
        public ComparisonEnum AccountAgeOperator { get; set; }

        //Payment level criteria
        public bool HasPaymentLevel { get; set; }
        public IEnumerable<Guid> PaymentLevels { get; set; }


        /// <summary>
        /// Risk levels to except
        /// </summary>
        public IEnumerable<Guid> RiskLevels { get; set; }
        public IEnumerable<WinningRuleDTO> WinningRules { get; set; }

        #endregion

        #region Constructors

        public AVCConfigurationDTO()
        {
            RiskLevels = new List<Guid>();
            WinningRules = new List<WinningRuleDTO>();
            PaymentLevels = new List<Guid>();
        }

        #endregion
    }

    public class WinningRuleDTO
    {
        public Guid Id { get; set; }

        public Guid ProductId { get; set; }
        public ComparisonEnum Comparison { get; set; }
        public decimal Amount { get; set; }

        public PeriodEnum Period { get; set; }
        public DateTimeOffset? StartDate { get; set; }
        public DateTimeOffset? EndDate { get; set; }
    }
}