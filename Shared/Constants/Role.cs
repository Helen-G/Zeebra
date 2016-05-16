using System;

namespace AFT.RegoV2.Shared.Constants
{
    public static class RoleIds
    {
        public static Guid SuperAdminId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        public static Guid DefaultId = new Guid("00000000-0000-0000-0000-000000000002");
        public static Guid MultipleBrandManagerId = Guid.Parse("00000000-0000-0000-0000-000000000009");
        public static Guid SingleBrandManagerId = Guid.Parse("00000000-0000-0000-0000-000000000010");
        public static Guid LicenseeId = Guid.Parse("00000000-0000-0000-0000-000000000008");
        public static Guid PlayerId = new Guid("00000000-0000-0000-0000-000000000011");
        public static Guid FraudOfficerId = new Guid("00000000-0000-0000-0000-000000000012");
        public static Guid KYCOfficerId = new Guid("00000000-0000-0000-0000-000000000014");
        public static Guid CSOfficerId = new Guid("00000000-0000-0000-0000-000000000007");
        public static Guid MarketingOfficerId = new Guid("00000000-0000-0000-0000-000000000006");
        public static Guid PaymentOfficerId = new Guid("00000000-0000-0000-0000-000000000013");
    }
}