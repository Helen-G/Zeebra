using System;
using System.Collections.Generic;

namespace AFT.RegoV2.AdminWebsite.ViewModels
{
    public class EditIpRegulationModelBase
    {
        public Guid Id { get; set; }
        
        public string IpAddress { get; set; }

        public string IpAddressBatch { get; set; }

        public string Description { get; set; }
    }

    public class EditAdminIpRegulationModel : EditIpRegulationModelBase
    {
    }

    public class BrandIpRegulationModelBase : EditIpRegulationModelBase
    {
        public Guid LicenseeId { get; set; }

        public string RedirectionUrl { get; set; }

        public string BlockingType { get; set; }
    }

    public class AddBrandIpRegulationModel : BrandIpRegulationModelBase
    {
        public IList<Guid> AssignedBrands { get; set; }
    }

    public class EditBrandIpRegulationModel : BrandIpRegulationModelBase
    {
        public Guid BrandId { get; set; }

    }
}