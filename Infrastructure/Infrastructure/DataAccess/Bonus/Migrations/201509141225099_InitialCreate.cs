namespace AFT.RegoV2.Infrastructure.DataAccess.Bonus.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "bonus.Bonuses",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Version = c.Int(nullable: false),
                        Name = c.String(),
                        Code = c.String(),
                        ActiveFrom = c.DateTimeOffset(nullable: false, precision: 7),
                        ActiveTo = c.DateTimeOffset(nullable: false, precision: 7),
                        Description = c.String(),
                        DurationType = c.Int(nullable: false),
                        DurationStart = c.DateTimeOffset(nullable: false, precision: 7),
                        DurationEnd = c.DateTimeOffset(nullable: false, precision: 7),
                        DaysToClaim = c.Int(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                        Statistic_Id = c.Guid(nullable: false),
                        CreatedBy = c.String(),
                        CreatedOn = c.DateTimeOffset(nullable: false, precision: 7),
                        UpdatedBy = c.String(),
                        UpdatedOn = c.DateTimeOffset(precision: 7),
                        Template_Id = c.Guid(),
                        Template_Version = c.Int(),
                    })
                .PrimaryKey(t => new { t.Id, t.Version })
                .ForeignKey("bonus.BonusStatistics", t => t.Statistic_Id, cascadeDelete: true)
                .ForeignKey("bonus.Templates", t => new { t.Template_Id, t.Template_Version })
                .Index(t => t.Statistic_Id)
                .Index(t => new { t.Template_Id, t.Template_Version });
            
            CreateTable(
                "bonus.BonusStatistics",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        TotalRedeemedAmount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        TotalRedemptionCount = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "bonus.Templates",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Version = c.Int(nullable: false),
                        CreatedBy = c.String(),
                        CreatedOn = c.DateTimeOffset(nullable: false, precision: 7),
                        UpdatedBy = c.String(),
                        UpdatedOn = c.DateTimeOffset(precision: 7),
                        Status = c.Int(nullable: false),
                        Availability_Id = c.Guid(),
                        Info_Id = c.Guid(),
                        Notification_Id = c.Guid(),
                        Rules_Id = c.Guid(),
                        Wagering_Id = c.Guid(),
                    })
                .PrimaryKey(t => new { t.Id, t.Version })
                .ForeignKey("bonus.TemplateAvailabilities", t => t.Availability_Id)
                .ForeignKey("bonus.TemplateInfoes", t => t.Info_Id)
                .ForeignKey("bonus.TemplateNotifications", t => t.Notification_Id)
                .ForeignKey("bonus.TemplateRules", t => t.Rules_Id)
                .ForeignKey("bonus.TemplateWagerings", t => t.Wagering_Id)
                .Index(t => t.Availability_Id)
                .Index(t => t.Info_Id)
                .Index(t => t.Notification_Id)
                .Index(t => t.Rules_Id)
                .Index(t => t.Wagering_Id);
            
            CreateTable(
                "bonus.TemplateAvailabilities",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        ParentBonusId = c.Guid(),
                        PlayerRegistrationDateFrom = c.DateTimeOffset(precision: 7),
                        PlayerRegistrationDateTo = c.DateTimeOffset(precision: 7),
                        WithinRegistrationDays = c.Int(nullable: false),
                        ExcludeOperation = c.Int(nullable: false),
                        PlayerRedemptionsLimit = c.Int(nullable: false),
                        PlayerRedemptionsLimitType = c.Int(nullable: false),
                        RedemptionsLimit = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "bonus.BonusExcludes",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        ExcludedBonusId = c.Guid(nullable: false),
                        TemplateAvailability_Id = c.Guid(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("bonus.TemplateAvailabilities", t => t.TemplateAvailability_Id)
                .Index(t => t.TemplateAvailability_Id);
            
            CreateTable(
                "bonus.RiskLevelExcludes",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        ExcludedRiskLevelId = c.Guid(nullable: false),
                        TemplateAvailability_Id = c.Guid(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("bonus.TemplateAvailabilities", t => t.TemplateAvailability_Id)
                .Index(t => t.TemplateAvailability_Id);
            
            CreateTable(
                "bonus.BonusVips",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Code = c.String(),
                        TemplateAvailability_Id = c.Guid(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("bonus.TemplateAvailabilities", t => t.TemplateAvailability_Id)
                .Index(t => t.TemplateAvailability_Id);
            
            CreateTable(
                "bonus.TemplateInfoes",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Name = c.String(),
                        BonusTrigger = c.Int(nullable: false),
                        DepositKind = c.Int(),
                        Description = c.String(),
                        WalletTemplateId = c.Guid(nullable: false),
                        Mode = c.Int(nullable: false),
                        IsWithdrawable = c.Boolean(nullable: false),
                        Brand_Id = c.Guid(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("bonus.Brands", t => t.Brand_Id)
                .Index(t => t.Brand_Id);
            
            CreateTable(
                "bonus.Brands",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Name = c.String(),
                        LicenseeId = c.Guid(nullable: false),
                        LicenseeName = c.String(),
                        TimezoneId = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "bonus.Currencies",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        BrandId = c.Guid(nullable: false),
                        Code = c.String(),
                    })
                .PrimaryKey(t => new { t.Id, t.BrandId })
                .ForeignKey("bonus.Brands", t => t.BrandId, cascadeDelete: true)
                .Index(t => t.BrandId);
            
            CreateTable(
                "bonus.RiskLevels",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                        Brand_Id = c.Guid(),
                        Player_Id = c.Guid(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("bonus.Brands", t => t.Brand_Id)
                .ForeignKey("bonus.Players", t => t.Player_Id)
                .Index(t => t.Brand_Id)
                .Index(t => t.Player_Id);
            
            CreateTable(
                "bonus.VipLevels",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Code = c.String(),
                        Brand_Id = c.Guid(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("bonus.Brands", t => t.Brand_Id)
                .Index(t => t.Brand_Id);
            
            CreateTable(
                "bonus.WalletTemplates",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Brand_Id = c.Guid(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("bonus.Brands", t => t.Brand_Id)
                .Index(t => t.Brand_Id);
            
            CreateTable(
                "bonus.Products",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        WalletTemplateId = c.Guid(nullable: false),
                        ProductId = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => new { t.Id, t.WalletTemplateId })
                .ForeignKey("bonus.WalletTemplates", t => t.WalletTemplateId, cascadeDelete: true)
                .Index(t => t.WalletTemplateId);
            
            CreateTable(
                "bonus.TemplateNotifications",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        EmailTemplateId = c.Guid(),
                        SmsTemplateId = c.Guid(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "bonus.TemplateRules",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        RewardType = c.Int(nullable: false),
                        IsAutoGenerateHighDeposit = c.Boolean(nullable: false),
                        ReferFriendMinDepositAmount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        ReferFriendWageringCondition = c.Decimal(nullable: false, precision: 18, scale: 2),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "bonus.BonusFundInWallets",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        WalletId = c.Guid(nullable: false),
                        TemplateRules_Id = c.Guid(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("bonus.TemplateRules", t => t.TemplateRules_Id)
                .Index(t => t.TemplateRules_Id);
            
            CreateTable(
                "bonus.RewardTiers",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        CurrencyCode = c.String(),
                        RewardAmountLimit = c.Decimal(nullable: false, precision: 18, scale: 2),
                        TemplateRules_Id = c.Guid(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("bonus.TemplateRules", t => t.TemplateRules_Id)
                .Index(t => t.TemplateRules_Id);
            
            CreateTable(
                "bonus.TemplateTiers",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        DateCreated = c.DateTime(nullable: false),
                        From = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Reward = c.Decimal(nullable: false, precision: 16, scale: 4),
                        To = c.Decimal(precision: 18, scale: 2),
                        MaxAmount = c.Decimal(precision: 18, scale: 2),
                        NotificationPercentThreshold = c.Decimal(precision: 3, scale: 2),
                        Discriminator = c.String(nullable: false, maxLength: 128),
                        RewardTier_Id = c.Guid(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("bonus.RewardTiers", t => t.RewardTier_Id)
                .Index(t => t.RewardTier_Id);
            
            CreateTable(
                "bonus.TemplateWagerings",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        HasWagering = c.Boolean(nullable: false),
                        Method = c.Int(nullable: false),
                        Multiplier = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Threshold = c.Decimal(nullable: false, precision: 18, scale: 2),
                        IsAfterWager = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "bonus.GameContributions",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        GameId = c.Guid(nullable: false),
                        Contribution = c.Decimal(nullable: false, precision: 18, scale: 2),
                        TemplateWagering_Id = c.Guid(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("bonus.TemplateWagerings", t => t.TemplateWagering_Id)
                .Index(t => t.TemplateWagering_Id);
            
            CreateTable(
                "bonus.Games",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        ProductId = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "bonus.Players",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Name = c.String(),
                        DepositTransactionsCount = c.Int(nullable: false),
                        VipLevel = c.String(),
                        CurrencyCode = c.String(),
                        DateRegistered = c.DateTimeOffset(nullable: false, precision: 7),
                        PhoneNumber = c.String(),
                        Email = c.String(),
                        ReferralId = c.Guid(nullable: false),
                        ReferredBy = c.Guid(),
                        AccumulatedWageringAmount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        IsMobileVerified = c.Boolean(nullable: false),
                        IsEmailVerified = c.Boolean(nullable: false),
                        Brand_Id = c.Guid(),
                        ReferredWith_Id = c.Guid(),
                        ReferredWith_Version = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("bonus.Brands", t => t.Brand_Id)
                .ForeignKey("bonus.Bonuses", t => new { t.ReferredWith_Id, t.ReferredWith_Version })
                .Index(t => t.Brand_Id)
                .Index(t => new { t.ReferredWith_Id, t.ReferredWith_Version });
            
            CreateTable(
                "bonus.Wallets",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        TemplateId = c.Guid(nullable: false),
                        NotWithdrawableBalance = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Player_Id = c.Guid(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("bonus.Players", t => t.Player_Id)
                .Index(t => t.Player_Id);
            
            CreateTable(
                "bonus.BonusRedemptions",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        ActivationState = c.Int(nullable: false),
                        RolloverState = c.Int(nullable: false),
                        Amount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        LockedAmount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Rollover = c.Decimal(nullable: false, precision: 18, scale: 2),
                        CreatedOn = c.DateTimeOffset(nullable: false, precision: 7),
                        UpdatedOn = c.DateTimeOffset(precision: 7),
                        Parameters_TransferExternalId = c.Guid(),
                        Parameters_TransferWalletTemplateId = c.Guid(),
                        Parameters_TransferAmount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Parameters_IsIssuedByCs = c.Boolean(nullable: false),
                        Bonus_Id = c.Guid(),
                        Bonus_Version = c.Int(),
                        Player_Id = c.Guid(),
                        Wallet_Id = c.Guid(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("bonus.Bonuses", t => new { t.Bonus_Id, t.Bonus_Version })
                .ForeignKey("bonus.Players", t => t.Player_Id)
                .ForeignKey("bonus.Wallets", t => t.Wallet_Id)
                .Index(t => new { t.Bonus_Id, t.Bonus_Version })
                .Index(t => t.Player_Id)
                .Index(t => t.Wallet_Id);
            
            CreateTable(
                "bonus.RolloverContributions",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Contribution = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Type = c.Int(nullable: false),
                        Transaction_Id = c.Guid(),
                        BonusRedemption_Id = c.Guid(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("bonus.BonusTransactions", t => t.Transaction_Id)
                .ForeignKey("bonus.BonusRedemptions", t => t.BonusRedemption_Id)
                .Index(t => t.Transaction_Id)
                .Index(t => t.BonusRedemption_Id);
            
            CreateTable(
                "bonus.BonusTransactions",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        RoundId = c.Guid(),
                        TotalAmount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        MainBalanceAmount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        BonusBalanceAmount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Type = c.Int(nullable: false),
                        CreatedOn = c.DateTimeOffset(nullable: false, precision: 7),
                        Wallet_Id = c.Guid(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("bonus.Wallets", t => t.Wallet_Id)
                .Index(t => t.Wallet_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("bonus.Wallets", "Player_Id", "bonus.Players");
            DropForeignKey("bonus.BonusTransactions", "Wallet_Id", "bonus.Wallets");
            DropForeignKey("bonus.BonusRedemptions", "Wallet_Id", "bonus.Wallets");
            DropForeignKey("bonus.BonusRedemptions", "Player_Id", "bonus.Players");
            DropForeignKey("bonus.RolloverContributions", "BonusRedemption_Id", "bonus.BonusRedemptions");
            DropForeignKey("bonus.RolloverContributions", "Transaction_Id", "bonus.BonusTransactions");
            DropForeignKey("bonus.BonusRedemptions", new[] { "Bonus_Id", "Bonus_Version" }, "bonus.Bonuses");
            DropForeignKey("bonus.RiskLevels", "Player_Id", "bonus.Players");
            DropForeignKey("bonus.Players", new[] { "ReferredWith_Id", "ReferredWith_Version" }, "bonus.Bonuses");
            DropForeignKey("bonus.Players", "Brand_Id", "bonus.Brands");
            DropForeignKey("bonus.Bonuses", new[] { "Template_Id", "Template_Version" }, "bonus.Templates");
            DropForeignKey("bonus.Templates", "Wagering_Id", "bonus.TemplateWagerings");
            DropForeignKey("bonus.GameContributions", "TemplateWagering_Id", "bonus.TemplateWagerings");
            DropForeignKey("bonus.Templates", "Rules_Id", "bonus.TemplateRules");
            DropForeignKey("bonus.RewardTiers", "TemplateRules_Id", "bonus.TemplateRules");
            DropForeignKey("bonus.TemplateTiers", "RewardTier_Id", "bonus.RewardTiers");
            DropForeignKey("bonus.BonusFundInWallets", "TemplateRules_Id", "bonus.TemplateRules");
            DropForeignKey("bonus.Templates", "Notification_Id", "bonus.TemplateNotifications");
            DropForeignKey("bonus.Templates", "Info_Id", "bonus.TemplateInfoes");
            DropForeignKey("bonus.TemplateInfoes", "Brand_Id", "bonus.Brands");
            DropForeignKey("bonus.WalletTemplates", "Brand_Id", "bonus.Brands");
            DropForeignKey("bonus.Products", "WalletTemplateId", "bonus.WalletTemplates");
            DropForeignKey("bonus.VipLevels", "Brand_Id", "bonus.Brands");
            DropForeignKey("bonus.RiskLevels", "Brand_Id", "bonus.Brands");
            DropForeignKey("bonus.Currencies", "BrandId", "bonus.Brands");
            DropForeignKey("bonus.Templates", "Availability_Id", "bonus.TemplateAvailabilities");
            DropForeignKey("bonus.BonusVips", "TemplateAvailability_Id", "bonus.TemplateAvailabilities");
            DropForeignKey("bonus.RiskLevelExcludes", "TemplateAvailability_Id", "bonus.TemplateAvailabilities");
            DropForeignKey("bonus.BonusExcludes", "TemplateAvailability_Id", "bonus.TemplateAvailabilities");
            DropForeignKey("bonus.Bonuses", "Statistic_Id", "bonus.BonusStatistics");
            DropIndex("bonus.BonusTransactions", new[] { "Wallet_Id" });
            DropIndex("bonus.RolloverContributions", new[] { "BonusRedemption_Id" });
            DropIndex("bonus.RolloverContributions", new[] { "Transaction_Id" });
            DropIndex("bonus.BonusRedemptions", new[] { "Wallet_Id" });
            DropIndex("bonus.BonusRedemptions", new[] { "Player_Id" });
            DropIndex("bonus.BonusRedemptions", new[] { "Bonus_Id", "Bonus_Version" });
            DropIndex("bonus.Wallets", new[] { "Player_Id" });
            DropIndex("bonus.Players", new[] { "ReferredWith_Id", "ReferredWith_Version" });
            DropIndex("bonus.Players", new[] { "Brand_Id" });
            DropIndex("bonus.GameContributions", new[] { "TemplateWagering_Id" });
            DropIndex("bonus.TemplateTiers", new[] { "RewardTier_Id" });
            DropIndex("bonus.RewardTiers", new[] { "TemplateRules_Id" });
            DropIndex("bonus.BonusFundInWallets", new[] { "TemplateRules_Id" });
            DropIndex("bonus.Products", new[] { "WalletTemplateId" });
            DropIndex("bonus.WalletTemplates", new[] { "Brand_Id" });
            DropIndex("bonus.VipLevels", new[] { "Brand_Id" });
            DropIndex("bonus.RiskLevels", new[] { "Player_Id" });
            DropIndex("bonus.RiskLevels", new[] { "Brand_Id" });
            DropIndex("bonus.Currencies", new[] { "BrandId" });
            DropIndex("bonus.TemplateInfoes", new[] { "Brand_Id" });
            DropIndex("bonus.BonusVips", new[] { "TemplateAvailability_Id" });
            DropIndex("bonus.RiskLevelExcludes", new[] { "TemplateAvailability_Id" });
            DropIndex("bonus.BonusExcludes", new[] { "TemplateAvailability_Id" });
            DropIndex("bonus.Templates", new[] { "Wagering_Id" });
            DropIndex("bonus.Templates", new[] { "Rules_Id" });
            DropIndex("bonus.Templates", new[] { "Notification_Id" });
            DropIndex("bonus.Templates", new[] { "Info_Id" });
            DropIndex("bonus.Templates", new[] { "Availability_Id" });
            DropIndex("bonus.Bonuses", new[] { "Template_Id", "Template_Version" });
            DropIndex("bonus.Bonuses", new[] { "Statistic_Id" });
            DropTable("bonus.BonusTransactions");
            DropTable("bonus.RolloverContributions");
            DropTable("bonus.BonusRedemptions");
            DropTable("bonus.Wallets");
            DropTable("bonus.Players");
            DropTable("bonus.Games");
            DropTable("bonus.GameContributions");
            DropTable("bonus.TemplateWagerings");
            DropTable("bonus.TemplateTiers");
            DropTable("bonus.RewardTiers");
            DropTable("bonus.BonusFundInWallets");
            DropTable("bonus.TemplateRules");
            DropTable("bonus.TemplateNotifications");
            DropTable("bonus.Products");
            DropTable("bonus.WalletTemplates");
            DropTable("bonus.VipLevels");
            DropTable("bonus.RiskLevels");
            DropTable("bonus.Currencies");
            DropTable("bonus.Brands");
            DropTable("bonus.TemplateInfoes");
            DropTable("bonus.BonusVips");
            DropTable("bonus.RiskLevelExcludes");
            DropTable("bonus.BonusExcludes");
            DropTable("bonus.TemplateAvailabilities");
            DropTable("bonus.Templates");
            DropTable("bonus.BonusStatistics");
            DropTable("bonus.Bonuses");
        }
    }
}
