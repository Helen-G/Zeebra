using System.Data.Entity;
using System.Linq;
using AFT.RegoV2.BoundedContexts.Report;
using AFT.RegoV2.BoundedContexts.Report.Data;
using AFT.RegoV2.Core.Report.Data.Admin;
using AFT.RegoV2.Core.Report.Data.Brand;

namespace AFT.RegoV2.Tests.Common.TestDoubles
{
    public class FakeReportRepository : IReportRepository
    {
        #region Admin
        private readonly FakeDbSet<AdminActivityLog> _adminActivityLog = new FakeDbSet<AdminActivityLog>();
        private readonly FakeDbSet<AdminAuthenticationLog> _adminAuthenticationLog = new FakeDbSet<AdminAuthenticationLog>();
        private readonly FakeDbSet<MemberAuthenticationLog> _memberAuthenticationLog = new FakeDbSet<MemberAuthenticationLog>();

        public IDbSet<AdminActivityLog> AdminActivityLog
        {
            get { return _adminActivityLog; }
        }
        public IDbSet<AdminAuthenticationLog> AdminAuthenticationLog
        {
            get { return _adminAuthenticationLog; }
        }
        public IDbSet<MemberAuthenticationLog> MemberAuthenticationLog
        {
            get { return _memberAuthenticationLog; }
        }
        #endregion
        #region Player Reports
        private readonly FakeDbSet<PlayerBetHistoryRecord> _playerBetHistoryRecords = new FakeDbSet<PlayerBetHistoryRecord>();
        private readonly FakeDbSet<PlayerRecord> _playerRecords = new FakeDbSet<PlayerRecord>();
        private readonly FakeDbSet<PlayerTransactionRecord> _playerTransactionRecords = new FakeDbSet<PlayerTransactionRecord>();

        public IDbSet<PlayerRecord> PlayerRecords
        {
            get { return _playerRecords; }
        }

        public IDbSet<PlayerBetHistoryRecord> PlayerBetHistoryRecords
        {
            get { return _playerBetHistoryRecords; }
        }

        public IDbSet<PlayerTransactionRecord> PlayerTransactionRecords
        {
            get { return _playerTransactionRecords; }
        }

        #endregion
        #region Payment Reports
        private readonly FakeDbSet<DepositRecord> _depositRecords = new FakeDbSet<DepositRecord>();

        public IDbSet<DepositRecord> DepositRecords
        {
            get { return _depositRecords; }
        }

        #endregion
        #region Brand Reports
        private readonly FakeDbSet<BrandRecord> _brandRecords = new FakeDbSet<BrandRecord>();
        private readonly FakeDbSet<LicenseeRecord> _licenseeRecords = new FakeDbSet<LicenseeRecord>();
        private readonly FakeDbSet<LanguageRecord> _languageRecords = new FakeDbSet<LanguageRecord>();
        private readonly FakeDbSet<VipLevelRecord> _vipLevelRecords = new FakeDbSet<VipLevelRecord>();

        public IDbSet<BrandRecord> BrandRecords
        {
            get { return _brandRecords; }
        }
        public IDbSet<LicenseeRecord> LicenseeRecords
        {
            get { return _licenseeRecords; }
        }
        public IDbSet<LanguageRecord> LanguageRecords
        {
            get { return _languageRecords; }
        }
        public IDbSet<VipLevelRecord> VipLevelRecords
        {
            get { return _vipLevelRecords; }
        }

        #endregion

        public int SaveChanges()
        {
            return 0;
        }
    }
}
