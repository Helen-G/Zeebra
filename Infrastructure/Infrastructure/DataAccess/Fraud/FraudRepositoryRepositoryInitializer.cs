﻿using System.Data.Entity;
using AFT.RegoV2.Infrastructure.DataAccess.Fraud.Migrations;

namespace AFT.RegoV2.Infrastructure.DataAccess.Fraud
{
    public class FraudRepositoryRepositoryInitializer :
        MigrateDatabaseToLatestVersion<FraudRepository, Configuration>
    {
    }
}