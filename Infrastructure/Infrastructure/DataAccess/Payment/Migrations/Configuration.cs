using System;
using System.Data.Entity.Migrations;
using System.Globalization;
using System.Linq;
using System.Collections.Generic;
using AFT.RegoV2.Domain.Payment.Data;
using AFT.RegoV2.Infrastructure.DataAccess.Payment.Repository;

namespace AFT.RegoV2.Infrastructure.DataAccess.Payment.Migrations
{
    public sealed class Configuration : DbMigrationsConfiguration<PaymentRepository>
    {
        public const string Schema = "payment";

        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
            AutomaticMigrationDataLossAllowed = true;
            MigrationsDirectory = @"DataAccess\Payment\Migrations";
        }
    }

    public static class CurrencyTools
    {
        public static IDictionary<string, string> Currencies;

        static CurrencyTools()
        {
            Currencies = CultureInfo
                .GetCultures(CultureTypes.AllCultures)
                .Where(c => !c.IsNeutralCulture)
                .Select(culture =>
                {
                    try
                    {
                        const int invariantClutureId = 127;
                        if (culture.LCID == invariantClutureId) return null;
                        return new RegionInfo(culture.LCID);
                    }
                    catch
                    {
                        return null;
                    }
                })
                .Where(ri => ri != null)
                .GroupBy(ri => ri.ISOCurrencySymbol)
                .ToDictionary(x => x.Key, x => x.First().CurrencyEnglishName);
        }
    }
}