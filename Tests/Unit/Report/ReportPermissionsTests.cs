using AFT.RegoV2.ApplicationServices.Report;
using AFT.RegoV2.ApplicationServices.Security;
using AFT.RegoV2.Core.Security.Common;
using AFT.RegoV2.Tests.Common.Base;
using Microsoft.Practices.Unity;
using NUnit.Framework;

namespace AFT.RegoV2.Tests.Unit.Report
{
    internal class ReportPermissionsTests : PermissionsTestsBase
    {
        private ReportQueries _reportQueries;

        public override void BeforeEach()
        {
            base.BeforeEach();

            _reportQueries = Container.Resolve<ReportQueries>();
        }

        [Test]
        public void Cannot_execute_ReportQueries_without_permissions()
        {
            // Arrange
            LogWithNewUser(Modules.PlayerManager, Permissions.Edit);

            // Act
            Assert.Throws<InsufficientPermissionsException>(() => _reportQueries.GetAdminActivityLog());
            Assert.Throws<InsufficientPermissionsException>(() => _reportQueries.GetAdminAuthenticationLog());
            Assert.Throws<InsufficientPermissionsException>(() => _reportQueries.GetMemberAuthenticationLog());
            Assert.Throws<InsufficientPermissionsException>(() => _reportQueries.GetPlayerRecords());
            Assert.Throws<InsufficientPermissionsException>(() => _reportQueries.GetPlayerRecordsForExport());
            Assert.Throws<InsufficientPermissionsException>(() => _reportQueries.GetPlayerBetHistoryRecords());
            Assert.Throws<InsufficientPermissionsException>(() => _reportQueries.GetPlayerBetHistoryRecordsForExport());
            Assert.Throws<InsufficientPermissionsException>(() => _reportQueries.GetDepositRecords());
            Assert.Throws<InsufficientPermissionsException>(() => _reportQueries.GetDepositRecordsForExport());
            Assert.Throws<InsufficientPermissionsException>(() => _reportQueries.GetBrandRecords());
            Assert.Throws<InsufficientPermissionsException>(() => _reportQueries.GetBrandRecordsForExport());
            Assert.Throws<InsufficientPermissionsException>(() => _reportQueries.GetLicenseeRecords());
            Assert.Throws<InsufficientPermissionsException>(() => _reportQueries.GetLicenseeRecordsForExport());
            Assert.Throws<InsufficientPermissionsException>(() => _reportQueries.GetLanguageRecords());
            Assert.Throws<InsufficientPermissionsException>(() => _reportQueries.GetLanguageRecordsForExport());
            Assert.Throws<InsufficientPermissionsException>(() => _reportQueries.GetVipLevelRecords());
            Assert.Throws<InsufficientPermissionsException>(() => _reportQueries.GetVipLevelRecordsForExport());
        }
    }
}