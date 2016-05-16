using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using AFT.RegoV2.Core.Security.Data;
using AFT.RegoV2.Domain.BoundedContexts.Security.Data;
using AFT.RegoV2.Domain.Security.Interfaces;
using AFT.RegoV2.Infrastructure.Constants;
using AFT.RegoV2.Infrastructure.DataAccess.Brand;
using AFT.RegoV2.Infrastructure.DependencyResolution;
using Microsoft.Practices.Unity;

namespace AFT.RegoV2.Core.Services.Security
{
    public sealed class Configuration : DbMigrationsConfiguration<SecurityRepository>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
            AutomaticMigrationDataLossAllowed = true;
            MigrationsDirectory = @"Services\Security\Repository\Migrations";
        }
    }
}
