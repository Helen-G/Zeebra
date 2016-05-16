using System;

namespace AFT.RegoV2.Core.Fraud.Data
{
    public class WagerConfigurationId
    {
        private readonly Guid _id;

        public WagerConfigurationId(Guid id)
        {
            _id = id;
        }

        public static implicit operator Guid(WagerConfigurationId id)
        {
            return id._id;
        }

        public static implicit operator WagerConfigurationId(Guid id)
        {
            return new WagerConfigurationId(id);
        }
    }


    public class WagerConfiguration
    {
        #region Properties

        public Guid Id { get; set; }
        public Guid BrandId { get; set; }
        public string CurrencyCode { get; set; }
        public bool IsServeAllCurrencies { get; set; }
        public bool IsDepositWageringCheck { get; set; }
        public bool IsManualAdjustmentWageringCheck { get; set; }
        public bool IsRebateWageringCheck { get; set; }
        public bool IsActive { get; set; }
        public Guid CreatedBy { get; set; }
        public Guid UpdatedBy { get; set; }
        public DateTimeOffset DateCreated { get; set; }
        public DateTimeOffset? DateUpdated { get; set; }
        public Guid? ActivatedBy { get; set; }
        public Guid? DeactivatedBy { get; set; }
        public DateTimeOffset? DateActivated { get; set; }
        public DateTimeOffset? DateDeactivated { get; set; }

        #endregion
    }
}