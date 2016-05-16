using System;

namespace AFT.RegoV2.Core.Brand.Data
{
    public class Contract
    {
        public Guid Id { get; set; }
        public Guid LicenseeId { get; set; }
        public Licensee Licensee { get; set; }
        public DateTimeOffset StartDate { get; set; }
        public DateTimeOffset? EndDate { get; set; }
        public bool IsCurrentContract { get; set; }
    }

    public enum ContractStatus
    {
        Inactive,
        Active,
        Expired
    }
}
