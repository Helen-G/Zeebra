using System;

namespace AFT.RegoV2.Core.Brand.Validators
{
    public class RenewLicenseeContractData
    {
        public Guid LicenseeId { get; set; }
        public string ContractStart { get; set; }
        public string ContractEnd { get; set; }
    }
}