using System;
using System.Linq;
using AFT.RegoV2.ApplicationServices.Security;
using AFT.RegoV2.BoundedContexts.Report;
using AFT.RegoV2.BoundedContexts.Report.Data;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Report.Data.Admin;
using AFT.RegoV2.Core.Report.Data.Brand;
using AFT.RegoV2.Core.Security.Common;

namespace AFT.RegoV2.ApplicationServices.Report
{
    public class ReportQueries : MarshalByRefObject, IApplicationService
    {
        private readonly IReportRepository _repository;

        public ReportQueries(IReportRepository repository)
        {
            _repository = repository;
        }

        #region Admin
        [Permission(Permissions.View, Module = Modules.AdminActivityLog)]
        public IQueryable<AdminActivityLog> GetAdminActivityLog()
        {
            return _repository.AdminActivityLog;
        }

        [Permission(Permissions.View, Module = Modules.AdminAuthenticationLog)]
        public IQueryable<AdminAuthenticationLog> GetAdminAuthenticationLog()
        {
            return _repository.AdminAuthenticationLog;
        }

        [Permission(Permissions.View, Module = Modules.MemberAuthenticationLog)]
        public IQueryable<MemberAuthenticationLog> GetMemberAuthenticationLog()
        {
            return _repository.MemberAuthenticationLog;
        }
        #endregion
        #region Player Reports
        [Permission(Permissions.View, Module = Modules.PlayerReport)]
        public IQueryable<PlayerRecord> GetPlayerRecords()
        {
            return _repository.PlayerRecords;
        }

        [Permission(Permissions.Export, Module = Modules.PlayerReport)]
        public IQueryable<PlayerRecord> GetPlayerRecordsForExport()
        {
            return _repository.PlayerRecords;
        }

        [Permission(Permissions.View, Module = Modules.PlayerBetHistoryReport)]
        public IQueryable<PlayerBetHistoryRecord> GetPlayerBetHistoryRecords()
        {
            return _repository.PlayerBetHistoryRecords;
        }

        [Permission(Permissions.Export, Module = Modules.PlayerBetHistoryReport)]
        public IQueryable<PlayerBetHistoryRecord> GetPlayerBetHistoryRecordsForExport()
        {
            return _repository.PlayerBetHistoryRecords;
        }

        //Add permissions
        public IQueryable<PlayerTransactionRecord> GetPlayerTransactionRecords()
        {
            return _repository.PlayerTransactionRecords;
        }
        public IQueryable<PlayerTransactionRecord> GetPlayerTransactionRecords(Guid playerId)
        {
            return _repository.PlayerTransactionRecords.Where(x => x.PlayerId == playerId);
        }
        public IQueryable<PlayerTransactionRecord> GetPlayerTransactionRecords(Guid playerId, string wallet)
        {
            return _repository.PlayerTransactionRecords.Where(x => x.PlayerId == playerId && x.Wallet == wallet);
        }

        #endregion
        #region Payment Reports
        [Permission(Permissions.View, Module = Modules.DepositReport)]
        public IQueryable<DepositRecord> GetDepositRecords()
        {
            return _repository.DepositRecords;
        }

        [Permission(Permissions.Export, Module = Modules.DepositReport)]
        public IQueryable<DepositRecord> GetDepositRecordsForExport()
        {
            return _repository.DepositRecords;
        }

        #endregion
        #region Brand Reports
        [Permission(Permissions.View, Module = Modules.BrandReport)]
        public IQueryable<BrandRecord> GetBrandRecords()
        {
            return _repository.BrandRecords;
        }

        [Permission(Permissions.Export, Module = Modules.BrandReport)]
        public IQueryable<BrandRecord> GetBrandRecordsForExport()
        {
            return _repository.BrandRecords;
        }

        [Permission(Permissions.View, Module = Modules.LicenseeReport)]
        public IQueryable<LicenseeRecord> GetLicenseeRecords()
        {
            return _repository.LicenseeRecords;
        }

        [Permission(Permissions.Export, Module = Modules.LicenseeReport)]
        public IQueryable<LicenseeRecord> GetLicenseeRecordsForExport()
        {
            return _repository.LicenseeRecords;
        }

        [Permission(Permissions.View, Module = Modules.LanguageReport)]
        public IQueryable<LanguageRecord> GetLanguageRecords()
        {
            return _repository.LanguageRecords;
        }

        [Permission(Permissions.Export, Module = Modules.LanguageReport)]
        public IQueryable<LanguageRecord> GetLanguageRecordsForExport()
        {
            return _repository.LanguageRecords;
        }

        [Permission(Permissions.View, Module = Modules.VipLevelReport)]
        public IQueryable<VipLevelRecord> GetVipLevelRecords()
        {
            return _repository.VipLevelRecords;
        }

        [Permission(Permissions.Export, Module = Modules.VipLevelReport)]
        public IQueryable<VipLevelRecord> GetVipLevelRecordsForExport()
        {
            return _repository.VipLevelRecords;
        }

        #endregion
    }
}
