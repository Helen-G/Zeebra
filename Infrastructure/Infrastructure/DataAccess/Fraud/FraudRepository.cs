using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.Linq;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Fraud;
using AFT.RegoV2.Core.Fraud.Data;
using AFT.RegoV2.Infrastructure.DataAccess.Fraud.Mappings;
using LocalBrand = AFT.RegoV2.Core.Fraud.Data.Brand;

namespace AFT.RegoV2.Infrastructure.DataAccess.Fraud
{
    public class FraudRepository : DbContext, IFraudRepository, ISeedable
    {
        #region Constructors

        static FraudRepository()
        {
            Database.SetInitializer(new FraudRepositoryRepositoryInitializer());
        }

        public FraudRepository()
            : base("name=Default")
        {
        }

        #endregion

        #region Methods

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Configurations.Add(new BrandMap());
            modelBuilder.Configurations.Add(new RiskLevelMap());

            modelBuilder.Configurations.Add(new PaymentLevelMap());

            modelBuilder.Configurations.Add(new WinningRuleMap());
            modelBuilder.Configurations.Add(new PlayerRiskLevelMap());
            modelBuilder.Configurations.Add(new WagerConfigurationMap());

            modelBuilder.Configurations.Add(new AutoVerificationCheckConfigurationMap());
            modelBuilder.Conventions.Remove<OneToManyCascadeDeleteConvention>();
        }

        public void Seed()
        {
            #region Brands

            var brand138 = new LocalBrand
            {
                Id = new Guid("00000000-0000-0000-0000-000000000138"),
                Name = "138",
                Code = "138",
                LicenseeId = new Guid("4A557EA9-E6B7-4F1F-AEE5-49E170ADB7E0"),
                LicenseeName = "Flycow",
                TimeZoneId = TimeZoneInfo.GetSystemTimeZones().First(t => t.BaseUtcOffset.Hours == 2).Id
            };
            var brand831 = new LocalBrand
            {
                Id = new Guid("D2CFDF01-FB79-4365-9E47-FC8E4DC1349C"),
                Name = "831",
                Code = "831",
                LicenseeId = new Guid("4A557EA9-E6B7-4F1F-AEE5-49E170ADB7E0"),
                LicenseeName = "Flycow",
                TimeZoneId = TimeZoneInfo.GetSystemTimeZones().First(t => t.BaseUtcOffset.Hours == -8).Id
            };

            Brands.AddOrUpdate(brand => brand.Id, brand138);
            Brands.AddOrUpdate(brand => brand.Id, brand831);
            SaveChanges();

            #endregion

            #region RiskLevels

            Guid[] riskIds = new Guid[] 
            {
                new Guid("5B6EA085-9661-4FA9-8391-54704040FE91"),
                new Guid("5B6EA085-9661-4FA9-8391-54704040FE92"),
                new Guid("5B6EA085-9661-4FA9-8391-54704040FE93"),
                new Guid("5B6EA085-9661-4FA9-8391-54704040FE94"),
                new Guid("5B6EA085-9661-4FA9-8391-54704040FE95")
            };
            string[] riskNames = { "VIP", "New Players", "Test Account", "Multiple Accounts", "Stolen Accounts" };

            for (int i = 0; i < 5; i++)
            {
                RiskLevels.AddOrUpdate(x => x.Id, new RiskLevel()
                {
                    Id = riskIds[i],
                    Level = i + 1,
                    Name = riskNames[i],
                    Status = i % 2 == 0 ? Status.Inactive : Status.Active,
                    BrandId = i % 2 == 0 ? brand138.Id : brand831.Id,
                    CreatedBy = "Initializer",
                    DateCreated = DateTimeOffset.Now,
                    Description = (i + 1).ToString() + " Initialized by dao"
                });
            }

            SaveChanges();
            #endregion

            #region PaymentLevels
            Guid[] paymentIds = 
            {
                new Guid("E1E600D4-0729-4D5C-B93E-085A94B55B33"),
                new Guid("1ED97A2B-EBA2-4B68-A70C-18A7070908F9"),
                new Guid("54A8B43D-B200-43A0-BCB4-4E2623BD5353")
            };

            Guid[] brandIds = 
            {
                new Guid("00000000-0000-0000-0000-000000000138"),
                new Guid("00000000-0000-0000-0000-000000000138"),
                new Guid("D2CFDF01-FB79-4365-9E47-FC8E4DC1349C")
            };

            string[] paymentNames = { "CADLevel", "CNYLevel", "CADVan"};
            string[] currencyCodes = {"CAD", "CNY", "CAD"};

            for (int i = 0; i < 3; i++)
            {
                PaymentLevels.AddOrUpdate(x => x.Id, new PaymentLevel()
                {
                    Id = paymentIds[i],
                    BrandId = brandIds[i],
                    CurrencyCode = currencyCodes[i],
                    ActivatedBy = "Initializer",
                    Name = paymentNames[i],
                    Code = currencyCodes[i],
                    EnableOfflineDeposit = true,
                    CreatedBy = "Initializer",
                    DateCreated = DateTimeOffset.Now,
                    DateActivated = DateTimeOffset.Now,
                    Status = PaymentLevelStatus.Active
                });
            }

            SaveChanges();
            #endregion

        }

        #endregion

        #region IFraudRepository Members

        public virtual IDbSet<RiskLevel> RiskLevels { get; set; }
        public virtual IDbSet<WinningRule> WinningRules { get; set; }
        public virtual IDbSet<LocalBrand> Brands { get; set; }
        public virtual IDbSet<PlayerRiskLevel> PlayerRiskLevels { get; set; }
        public virtual IDbSet<AutoVerificationCheckConfiguration> AutoVerificationCheckConfigurations { get; set; }
        public IDbSet<WagerConfiguration> WagerConfigurations { get; set; }


        public virtual IDbSet<PaymentLevel> PaymentLevels { get; set; }



        public override int SaveChanges()
        {
            try
            {
                return base.SaveChanges();
            }
            catch (DbEntityValidationException e)
            {
                var dbValidationErrorMessages = e.EntityValidationErrors.ToArray();
                Trace.WriteLine(dbValidationErrorMessages.ToString());
                throw;
            }
        }


        #endregion
    }
}