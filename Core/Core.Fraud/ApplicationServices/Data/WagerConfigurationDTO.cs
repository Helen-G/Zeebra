using System;

namespace AFT.RegoV2.Core.Fraud.ApplicationServices.Data
{
    public class WagerConfigurationDTO
    {
        #region Properties

        public Guid Id { get; set; }
        public Guid BrandId { get; set; }

        public Guid Brand
        {
            get
            {
                return BrandId;
            }
        }

        public string Currency { get; set; }
        public bool IsDepositWageringCheck { get; set; }
        public bool IsManualAdjustmentWageringCheck { get; set; }
        public bool IsRebateWageringCheck { get; set; }
        public string IsActive { get; set; }
        public string CreatedBy { get; set; }
        public string UpdatedBy { get; set; }
        public DateTimeOffset DateCreated { get; set; }
        public DateTimeOffset? DateUpdated { get; set; }
        public string ActivatedBy { get; set; }
        public string DeactivatedBy { get; set; }
        public DateTimeOffset? DateActivated { get; set; }
        public DateTimeOffset? DateDeactivated { get; set; }

        public string Status
        {
            get
            {
                var status = String.Empty;
                if (IsDepositWageringCheck)
                    status += " Deposit";

                if (IsManualAdjustmentWageringCheck)
                    status += " Manual";

                if (IsRebateWageringCheck)
                    status += " Rebate";

                if (!string.IsNullOrEmpty(status))
                    status += " Wagering Check";

                return status;
            }
        }

        #endregion
    }
}