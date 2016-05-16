using AFT.RegoV2.Core.Common.Interfaces.Admin;

namespace AFT.RegoV2.Core.Common.Data.Admin
{
    public class DeactivateCultureData : IDeactivateCultureData
    {
        public string Code { get; set; }
        public string Remarks { get; set; }
    }
}