using System.Data.Entity;
using AFT.RegoV2.BoundedContexts.Report.Data;
using AFT.RegoV2.Core.Report.Data.Admin;
using AFT.RegoV2.Core.Report.Data.Brand;

namespace AFT.RegoV2.BoundedContexts.Report
{
    public interface IReportRepository
    {
        // Admin
        IDbSet<AdminActivityLog> AdminActivityLog { get; }
        IDbSet<AdminAuthenticationLog> AdminAuthenticationLog { get; }
        IDbSet<MemberAuthenticationLog> MemberAuthenticationLog { get; }
        
        // Player Reports
        IDbSet<PlayerRecord> PlayerRecords { get; }
        IDbSet<PlayerBetHistoryRecord> PlayerBetHistoryRecords { get; }

        // Payment Reports
        IDbSet<DepositRecord> DepositRecords { get; }

        // Brand Reports
        IDbSet<BrandRecord> BrandRecords { get; }
        IDbSet<LicenseeRecord> LicenseeRecords { get; }
        IDbSet<LanguageRecord> LanguageRecords { get; }
        IDbSet<VipLevelRecord> VipLevelRecords { get; }

        // Transaction Reports
        IDbSet<PlayerTransactionRecord> PlayerTransactionRecords { get; }

        int SaveChanges();
    }
}