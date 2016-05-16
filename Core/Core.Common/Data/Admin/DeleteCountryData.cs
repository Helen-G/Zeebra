using AFT.RegoV2.Core.Common.Interfaces.Admin;

namespace AFT.RegoV2.Core.Common.Data.Admin
{
    public class DeleteCountryData : IDeleteCountryData
    {
        public string Code { get; set; }
    }
}