using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using AFT.RegoV2.Core.Bonus;
using AFT.RegoV2.Core.Bonus.ApplicationServices;
using AFT.RegoV2.Core.Bonus.Data;
using AFT.RegoV2.Core.Bonus.Data.ViewModels;
using AFT.RegoV2.Core.Bonus.DomainServices;
using AFT.RegoV2.Core.Common.Utils;
using AFT.RegoV2.Infrastructure.DataAccess.Bonus;
using AFT.RegoV2.Shared;
using AFT.RegoV2.Tests.Common;
using AFT.RegoV2.Tests.Common.Base;
using AFT.RegoV2.Tests.Common.Helpers;
using Microsoft.Practices.Unity;
using NUnit.Framework;

namespace AFT.RegoV2.Tests.Integration
{
    internal class BonusServiceTests : WebServiceTestsBase
    {
        private BonusQueries _bonusQueries;
        private BonusTestHelper _bonusTestHelper;
        private BonusBuilder _bonusBuilder;
        private BonusManagementCommands _bonusManagementCommands;

        public override void BeforeEach()
        {
            base.BeforeEach();

            Container.Resolve<SecurityTestHelper>().SignInSuperAdmin();

            _bonusQueries = Container.Resolve<BonusQueries>();
            _bonusManagementCommands = Container.Resolve<BonusManagementCommands>();
            _bonusBuilder = Container.Resolve<BonusBuilder>();
            _bonusTestHelper = Container.Resolve<BonusTestHelper>();
        }

        [Test]
        public void Updated_bonus_is_correctly_saved_to_DB()
        {
            var template = _bonusTestHelper.CreateFirstDepositTemplate(mode: IssuanceMode.AutomaticWithCode);
            var baseBonus = _bonusTestHelper.CreateBasicBonus(mode: IssuanceMode.AutomaticWithCode);

            var updateUiData = new BonusVM
            {
                Name = TestDataGenerator.GetRandomString(),
                Code = TestDataGenerator.GetRandomString(),
                IsActive = true,
                TemplateId = template.Id,
                TemplateVersion = template.Version,
                Id = baseBonus.Id,
                Version = baseBonus.Version,
                DurationType = DurationType.None.ToString(),
                ActiveFrom = DateTimeOffset.Now.ToBrandOffset(template.Info.Brand.TimezoneId).ToString("d"),
                ActiveTo = DateTimeOffset.Now.AddDays(1).ToBrandOffset(template.Info.Brand.TimezoneId).ToString("d")
            };
            var bonus = _bonusBuilder.BuildBonus(updateUiData).Entity;
            _bonusManagementCommands.UpdateBonus(bonus);

            var updatedBonus = _bonusQueries.GetCurrentVersionBonuses().Single(a => a.Id == bonus.Id);

            Assert.AreEqual(1, updatedBonus.Version);
            Assert.AreEqual(updateUiData.Code, updatedBonus.Code);
        }

        [Test]
        public void Bonus_deactivation_is_saved_to_DB()
        {
            var bonus = _bonusTestHelper.CreateBasicBonus();
            _bonusManagementCommands.ChangeBonusStatus(new ToggleBonusStatusVM { Id = bonus.Id });

            var updatedBonus = _bonusQueries.GetCurrentVersionBonuses().Single(a => a.Id == bonus.Id);

            Assert.False(updatedBonus.IsActive);
        }

        [Test]
        public void Statistic_persist_between_bonus_versions()
        {
            var bonus = _bonusTestHelper.CreateBasicBonus();
            _bonusManagementCommands = Container.Resolve<BonusManagementCommands>();
            _bonusManagementCommands.ChangeBonusStatus(new ToggleBonusStatusVM { Id = bonus.Id });
            _bonusManagementCommands = Container.Resolve<BonusManagementCommands>();
            _bonusManagementCommands.ChangeBonusStatus(new ToggleBonusStatusVM { Id = bonus.Id });
            _bonusManagementCommands = Container.Resolve<BonusManagementCommands>();
            _bonusManagementCommands.ChangeBonusStatus(new ToggleBonusStatusVM { Id = bonus.Id });

            var bonusRepository = Container.Resolve<IBonusRepository>();
            var statisticIds = bonusRepository.Bonuses.Where(a => a.Id == bonus.Id).Select(s => s.StatisticId).ToList();

            //assert that all ids are the same
            Assert.True(statisticIds.All(id => id == statisticIds.First()));
        }

        [Test]
        public void Bonus_activation_do_not_create_new_statistic_record()
        {
            const decimal depositAmount = 100;
            var bonus = _bonusTestHelper.CreateBasicBonus(mode: IssuanceMode.AutomaticWithCode);

            var player = Container.Resolve<PlayerTestHelper>().CreatePlayer();
            WaitForPlayerRegistered(player.Id, TimeSpan.FromSeconds(2));

            Container.Resolve<PaymentTestHelper>().MakeDeposit(player.Id, depositAmount, bonus.Code);
            var bonusRepository = Container.Resolve<IBonusRepository>();
            var statisticIds = bonusRepository.Bonuses.Where(a => a.Id == bonus.Id).Select(s => s.StatisticId).ToList();

            //assert that all ids are the same
            Assert.True(statisticIds.All(id => id == statisticIds.First()));
        }

        [Test]
        public void Add_template_saves_all_data_to_DB()
        {
            var template = _bonusTestHelper.CreateFirstDepositTemplate();
            var templateFromDb = _bonusQueries.GetCurrentVersionTemplates().Single(t => t.Id == template.Id);

            Assert.NotNull(templateFromDb.Info);
            Assert.NotNull(templateFromDb.Availability);
            Assert.NotNull(templateFromDb.Rules);
            Assert.NotNull(templateFromDb.Wagering);
            Assert.NotNull(templateFromDb.Notification);
            Assert.AreEqual(TemplateStatus.Complete, templateFromDb.Status);
        }

        [Test]
        public void Updated_template_is_correctly_saved_to_DB()
        {
            var baseTemplate = _bonusTestHelper.CreateFirstDepositTemplate();

            var templateUiData = new TemplateVM
            {
                Id = baseTemplate.Id,
                Version = baseTemplate.Version,
                Wagering = new TemplateWageringVM
                {
                    Threshold = 1000,
                    GameContributions = 
                    {
                        new GameContributionVM
                        {
                            GameId = new Guid("B641B4E9-CA08-4443-8FD3-8D1A43727C3E"),
                            Contribution = 4
                        }
                    }
                }
            };
            var template = _bonusBuilder.BuildTemplate(templateUiData).Entity;
            _bonusManagementCommands.AddUpdateTemplate(template);
            var updatedTemplate = _bonusQueries.GetCurrentVersionTemplates().Single(a => a.Id == baseTemplate.Id);

            Assert.AreEqual(1, updatedTemplate.Version);
            Assert.AreEqual(1000, updatedTemplate.Wagering.Threshold);
            Assert.AreEqual(0.04, updatedTemplate.Wagering.GameContributions.First().Contribution);
        }

        [Test]
        public void Can_delete_template()
        {
            var template = _bonusTestHelper.CreateFirstDepositTemplate();
            _bonusManagementCommands.DeleteTemplate(template.Id);

            template = _bonusQueries.GetCurrentVersionTemplates().SingleOrDefault(t => t.Id == template.Id);
            Assert.Null(template);
        }

        [Test]
        public void Vips_count_does_not_change_between_template_edits()
        {
            var templateUiData = new TemplateVM
            {
                Info = new TemplateInfoVM
                {
                    Name = TestDataGenerator.GetRandomString(),
                    TemplateType = BonusType.FirstDeposit.ToString(),
                    BrandId = new Guid("00000000-0000-0000-0000-000000000138"),
                    WalletTemplateId = new Guid("9D366EF4-7AEF-4DFE-80E1-045909DC8EFD")
                }
            };
            var template = _bonusBuilder.BuildTemplate(templateUiData).Entity;
            _bonusManagementCommands.AddUpdateTemplate(template);

            templateUiData = new TemplateVM
            {
                Id = template.Id,
                Version = template.Version,
                Availability = new TemplateAvailabilityVM { VipLevels = new List<string> { "S" } }
            };
            template = _bonusBuilder.BuildTemplate(templateUiData).Entity;
            _bonusManagementCommands.AddUpdateTemplate(template);

            var repository = Container.Resolve<IBonusRepository>();
            var templateFromDb = repository.Templates.Single(t => t.Id == template.Id && t.Version == template.Version);
            Assert.AreEqual(1, templateFromDb.Availability.VipLevels.Count);

            templateUiData = new TemplateVM
            {
                Id = template.Id,
                Version = template.Version,
                Rules = new TemplateRulesVM
                {
                    RewardTiers = new List<RewardTierVM>
                    {
                        new RewardTierVM
                        {
                            CurrencyCode = "CAD",
                            BonusTiers = new List<TemplateTierVM> { new TemplateTierVM { Reward = 25, DateCreated = DateTime.Now} }
                        }
                    },
                    RewardType = BonusRewardType.Amount
                }
            };
            template = _bonusBuilder.BuildTemplate(templateUiData).Entity;
            _bonusManagementCommands.AddUpdateTemplate(template);

            repository = Container.Resolve<IBonusRepository>();
            templateFromDb = repository.Templates.Single(t => t.Id == template.Id && t.Version == template.Version);
            Assert.AreEqual(1, templateFromDb.Availability.VipLevels.Count);
        }

        [Test]
        public void Template_update_updates_bonus()
        {
            var initialBonus = _bonusTestHelper.CreateBasicBonus(false);

            var bonusVersion = initialBonus.Version;
            var templateVersion = initialBonus.Template.Version;

            var templateUiData = new TemplateVM
            {
                Id = initialBonus.Template.Id,
                Version = initialBonus.Template.Version,
                Info = new TemplateInfoVM
                {
                    BrandId = initialBonus.Template.Info.Brand.Id,
                    Name = TestDataGenerator.GetRandomString(),
                    TemplateType = BonusType.FirstDeposit.ToString(),
                    WalletTemplateId = new Guid("9D366EF4-7AEF-4DFE-80E1-045909DC8EFD")
                },
                Rules = new TemplateRulesVM
                {
                    RewardTiers = new List<RewardTierVM>
                    {
                        new RewardTierVM
                        {
                            CurrencyCode = "CAD",
                            BonusTiers = new List<TemplateTierVM> { new TemplateTierVM { Reward = 25, DateCreated = DateTime.Now } }
                        }
                    },
                    RewardType = BonusRewardType.Percentage
                }
            };
            var template = _bonusBuilder.BuildTemplate(templateUiData).Entity;
            _bonusManagementCommands.AddUpdateTemplate(template);

            var updatedBonus = _bonusQueries.GetCurrentVersionBonuses().Single(bonus => bonus.Id == initialBonus.Id);

            Assert.AreEqual(bonusVersion + 1, updatedBonus.Version);
            Assert.AreEqual(templateVersion + 1, updatedBonus.Template.Version);
        }

        private void WaitForPlayerRegistered(Guid playerId, TimeSpan timeout)
        {
            var bonusRepository = Container.Resolve<BonusRepository>();
            var stopwatch = Stopwatch.StartNew();
            while (bonusRepository.Players.All(p => p.Id != playerId) && stopwatch.Elapsed < timeout)
            {
                Thread.Sleep(100);
            }
            if (bonusRepository.Players.All(p => p.Id != playerId))
            {
                throw new RegoException("Player registration timeout");
            }
        }
    }
}