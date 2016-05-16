using System;
using System.Collections.Generic;
using System.Linq;
using AFT.RegoV2.Core.Bonus;
using AFT.RegoV2.Core.Bonus.Data;
using AFT.RegoV2.Core.Bonus.Data.ViewModels;
using AFT.RegoV2.Core.Bonus.DomainServices;
using AFT.RegoV2.Core.Bonus.Resources;
using AFT.RegoV2.Tests.Common;
using AFT.RegoV2.Tests.Common.Base;
using Microsoft.Practices.Unity;
using NUnit.Framework;

namespace AFT.RegoV2.Tests.Unit.Bonus.Validation
{
    class BuildTemplateTests : BonusTestsBase
    {
        private BonusBuilder _bonusBuilder;
        private TemplateVM _vm;
        private Core.Bonus.Data.Brand _brand;

        public override void BeforeEach()
        {
            base.BeforeEach();

            _bonusBuilder = Container.Resolve<BonusBuilder>();
            const string currency = "AAA";
            var walletId = Guid.NewGuid();
            _brand = new Core.Bonus.Data.Brand
            {
                Id = Guid.NewGuid(),
                Currencies = new List<Currency> { new Currency { Code = currency } },
                TimezoneId = TestDataGenerator.GetRandomTimeZone().Id,
                LicenseeName = TestDataGenerator.GetRandomString(),
                WalletTemplates = new List<WalletTemplate> { new WalletTemplate { Id = walletId } },
                Vips = new List<VipLevel> { new VipLevel { Code = "S" } }
            };
            BonusRepository.Brands.Add(_brand);

            var templateId = Guid.NewGuid();
            var template = new Template
            {
                Id = templateId,
                Info = new TemplateInfo
                {
                    Name = TestDataGenerator.GetRandomString(),
                    Brand = BonusRepository.Brands.Single(b => b.Id == _brand.Id),
                    WalletTemplateId = walletId
                }
            };
            BonusRepository.Templates.Add(template);

            _vm = new TemplateVM
            {
                Id = templateId,
                Info = new TemplateInfoVM
                {
                    Name = TestDataGenerator.GetRandomString(),
                    BrandId = _brand.Id,
                    WalletTemplateId = walletId,
                    TemplateType = BonusType.FirstDeposit.ToString(),
                    Mode = IssuanceMode.Automatic.ToString()
                },
                Rules = new TemplateRulesVM
                {
                    RewardTiers = new List<RewardTierVM>
                    {
                        new RewardTierVM
                        {
                            CurrencyCode = currency,
                            BonusTiers = new List<TemplateTierVM>
                            {
                                new TemplateTierVM {Reward = 25}
                            }
                        }
                    }
                }
            };
        }

        [Test]
        public void Name_is_empty_validation_works()
        {
            _vm.Info.Name = string.Empty;

            var buildResult = _bonusBuilder.BuildTemplate(_vm);
            Assert.AreEqual(false, buildResult.IsValid);

            var message = buildResult.Errors.Single().ErrorMessage;
            Assert.AreEqual(ValidatorMessages.NameIsNotSpecified, message);
        }

        [TestCase(1, true)]
        [TestCase(50, true)]
        [TestCase(51, false)]
        public void Name_length_validation_works(int length, bool isValid)
        {
            _vm.Info.Name = TestDataGenerator.GetRandomString(length);

            var buildResult = _bonusBuilder.BuildTemplate(_vm);
            Assert.AreEqual(isValid, buildResult.IsValid);

            if (isValid == false)
            {
                var message = buildResult.Errors.Single().ErrorMessage;
                Assert.AreEqual(string.Format(ValidatorMessages.NameLengthIsInvalid, 1, 50), message);
            }
        }

        [Test]
        public void Name_uniqueness_during_add_validation_works()
        {
            const string name = "ASD";
            var repository = Container.Resolve<IBonusRepository>();
            repository.Templates.Add(new Template { Info = new TemplateInfo { Name = name, Brand = _brand } });

            _vm.Info.Name = name;

            var buildResult = _bonusBuilder.BuildTemplate(_vm);
            Assert.AreEqual(false, buildResult.IsValid);

            var message = buildResult.Errors.Single().ErrorMessage;
            Assert.AreEqual(ValidatorMessages.NameIsNotUnique, message);
        }

        [Test]
        public void Name_uniqueness_validation_works_during_edit()
        {
            var id = Guid.NewGuid();
            const string name = "ASD";
            BonusRepository.Templates.Add(new Template { Id = id, Info = new TemplateInfo { Name = name, Brand = _brand } });
            BonusRepository.Templates.Add(new Template { Id = Guid.NewGuid(), Info = new TemplateInfo { Name = name, Brand = _brand } });

            _vm.Id = id;
            _vm.Info.Name = name;

            var buildResult = _bonusBuilder.BuildTemplate(_vm);
            Assert.AreEqual(false, buildResult.IsValid);

            Assert.That(buildResult.Errors.Any(e => e.ErrorMessage == ValidatorMessages.NameIsNotUnique));
        }

        [Test]
        public void Name_uniqueness_is_persisted_per_brand()
        {
            const string name = "ASD";
            var repository = Container.Resolve<IBonusRepository>();
            repository.Templates.Add(new Template { Info = new TemplateInfo { Name = name, Brand = new Core.Bonus.Data.Brand { Id = Guid.NewGuid() } } });

            _vm.Info.Name = name;

            var buildResult = _bonusBuilder.BuildTemplate(_vm);
            Assert.AreEqual(true, buildResult.IsValid);
        }

        [Test]
        public void Transaction_amount_limit_validation_works()
        {
            _vm.Rules.RewardTiers.Single().BonusTiers.Single().MaxAmount = -1;

            var buildResult = _bonusBuilder.BuildTemplate(_vm);
            Assert.AreEqual(false, buildResult.IsValid);

            var message = buildResult.Errors.Single().ErrorMessage;
            Assert.AreEqual(ValidatorMessages.TemplateTransactionAmountLimitIsNegative, message);
        }

        [Test]
        public void Player_count_limit_validation_works()
        {
            _vm.Availability = new TemplateAvailabilityVM { RedemptionsLimit = -1 };

            var buildResult = _bonusBuilder.BuildTemplate(_vm);
            Assert.AreEqual(false, buildResult.IsValid);

            var message = buildResult.Errors.Single().ErrorMessage;
            Assert.AreEqual(ValidatorMessages.TemplatePlayerRedemptionLimitIsNegative, message);
        }

        [Test]
        public void Reward_amount_limit_validation_works()
        {
            _vm.Rules.RewardTiers.Single().RewardAmountLimit = -1;

            var buildResult = _bonusBuilder.BuildTemplate(_vm);
            Assert.AreEqual(false, buildResult.IsValid);

            var message = buildResult.Errors.Single().ErrorMessage;
            Assert.AreEqual(ValidatorMessages.TemplateRewardLimitIsNegative, message);
        }

        [TestCase(1, true)]
        [TestCase(200, true)]
        [TestCase(201, false)]
        public void Remarks_length_validation_works(int length, bool isValid)
        {
            _vm.Info.Description = TestDataGenerator.GetRandomString(length);

            var buildResult = _bonusBuilder.BuildTemplate(_vm);
            Assert.AreEqual(isValid, buildResult.IsValid);

            if (isValid == false)
            {
                var message = buildResult.Errors.Single().ErrorMessage;
                Assert.AreEqual(string.Format(ValidatorMessages.DescriptionLengthIsInvalid, 1, 200), message);
            }
        }

        [Test]
        public void Brand_validation_works()
        {
            _vm.Id = Guid.Empty;
            _vm.Info.BrandId = Guid.NewGuid();

            var buildResult = _bonusBuilder.BuildTemplate(_vm);
            Assert.AreEqual(false, buildResult.IsValid);

            var message = buildResult.Errors.Single().ErrorMessage;
            Assert.AreEqual(ValidatorMessages.TemplateBrandDoesNotExist, message);
        }

        [Test]
        public void Currency_not_specified_validation_works()
        {
            _vm.Rules.RewardTiers = new List<RewardTierVM>();

            var buildResult = _bonusBuilder.BuildTemplate(_vm);
            Assert.AreEqual(false, buildResult.IsValid);

            var message = buildResult.Errors.Single().ErrorMessage;
            Assert.AreEqual(ValidatorMessages.TemplateCurrenciesAreEmpty, message);
        }

        [Test]
        public void Wagering_condition_validation_works()
        {
            _vm.Wagering = new TemplateWageringVM
            {
                Multiplier = -1
            };

            var buildResult = _bonusBuilder.BuildTemplate(_vm);
            Assert.AreEqual(false, buildResult.IsValid);

            var message = buildResult.Errors.Single().ErrorMessage;
            Assert.AreEqual(ValidatorMessages.TemplateWageringConditionIsNegative, message);
        }

        [Test]
        public void Wagering_threshold_validaton_works()
        {
            _vm.Wagering = new TemplateWageringVM
            {
                Threshold = -1
            };

            var buildResult = _bonusBuilder.BuildTemplate(_vm);
            Assert.AreEqual(false, buildResult.IsValid);

            var message = buildResult.Errors.Single().ErrorMessage;
            Assert.AreEqual(ValidatorMessages.TemplateWageringThresholdIsNegative, message);
        }

        [Test]
        public void Wagering_contributions_validation_works()
        {
            var gameId = Guid.NewGuid();
            BonusRepository.Games.Add(new Core.Bonus.Data.Game { Id = gameId });
            _vm.Wagering = new TemplateWageringVM
            {
                GameContributions = new List<GameContributionVM>
                {
                    new GameContributionVM
                    {
                        GameId = gameId,
                        Contribution = -1
                    }
                }
            };

            var buildResult = _bonusBuilder.BuildTemplate(_vm);
            Assert.AreEqual(false, buildResult.IsValid);

            var message = buildResult.Errors.Single().ErrorMessage;
            Assert.AreEqual(ValidatorMessages.TemplateOneOfGameContributionsIsNegative, message);
        }

        [Test]
        public void Wagering_contribution_game_validation_works()
        {
            var gameId = Guid.NewGuid();
            BonusRepository.Games.Add(new Core.Bonus.Data.Game());
            BonusRepository.Games.Add(new Core.Bonus.Data.Game { Id = gameId });
            _vm.Wagering = new TemplateWageringVM
            {
                GameContributions = new List<GameContributionVM>
                {
                    new GameContributionVM
                    {
                        GameId = gameId,
                        Contribution = 0
                    },
                    new GameContributionVM
                    {
                        GameId = new Guid(),
                        Contribution = 1
                    }
                }
            };

            var buildResult = _bonusBuilder.BuildTemplate(_vm);
            Assert.AreEqual(false, buildResult.IsValid);

            var message = buildResult.Errors.Single().ErrorMessage;
            Assert.AreEqual(ValidatorMessages.TemplateOneOfGameContributionsPointsToInvalidGame, message);
        }

        [Test]
        public void After_wager_template_requires_wagering_condition_validation_works()
        {
            _vm.Wagering = new TemplateWageringVM { HasWagering = true, Multiplier = 0 };

            var buildResult = _bonusBuilder.BuildTemplate(_vm);
            Assert.AreEqual(false, buildResult.IsValid);

            var message = buildResult.Errors.Single().ErrorMessage;
            Assert.AreEqual(ValidatorMessages.TemplateWageringConditionIsZeroOrLess, message);
        }

        [TestCase(BonusType.ReloadDeposit, ExpectedResult = true)]
        [TestCase(BonusType.FirstDeposit, ExpectedResult = true)]
        [TestCase(BonusType.FundIn, ExpectedResult = true)]
        [TestCase(BonusType.HighDeposit, ExpectedResult = false)]
        [TestCase(BonusType.MobilePlusEmailVerification, ExpectedResult = false)]
        [TestCase(BonusType.ReferFriend, ExpectedResult = false)]
        public bool Wagering_method_validation_works(BonusType bonusType)
        {
            _vm.Info.TemplateType = bonusType.ToString();
            _vm.Rules.FundInWallets = _brand.WalletTemplates.Select(w => w.Id).ToList();
            _vm.Wagering = new TemplateWageringVM
            {
                Method = WageringMethod.BonusAndTransferAmount
            };

            var buildResult = _bonusBuilder.BuildTemplate(_vm);
            if (buildResult.IsValid == false)
            {
                Assert.True(
                    buildResult.Errors.Any(
                        error =>
                            error.ErrorMessage == ValidatorMessages.TemplateWageringMethodIsNotSupportedByBonusTrigger));
            }

            return buildResult.IsValid;
        }

        [TestCase(BonusType.FirstDeposit, BonusRewardType.Amount, ExpectedResult = true)]
        [TestCase(BonusType.FirstDeposit, BonusRewardType.Percentage, ExpectedResult = true)]
        [TestCase(BonusType.FirstDeposit, BonusRewardType.TieredAmount, ExpectedResult = true)]
        [TestCase(BonusType.FirstDeposit, BonusRewardType.TieredPercentage, ExpectedResult = true)]
        [TestCase(BonusType.ReloadDeposit, BonusRewardType.Amount, ExpectedResult = true)]
        [TestCase(BonusType.ReloadDeposit, BonusRewardType.Percentage, ExpectedResult = true)]
        [TestCase(BonusType.ReloadDeposit, BonusRewardType.TieredAmount, ExpectedResult = true)]
        [TestCase(BonusType.ReloadDeposit, BonusRewardType.TieredPercentage, ExpectedResult = true)]
        [TestCase(BonusType.HighDeposit, BonusRewardType.Amount, ExpectedResult = false)]
        [TestCase(BonusType.HighDeposit, BonusRewardType.Percentage, ExpectedResult = false)]
        [TestCase(BonusType.HighDeposit, BonusRewardType.TieredAmount, ExpectedResult = true)]
        [TestCase(BonusType.HighDeposit, BonusRewardType.TieredPercentage, ExpectedResult = false)]
        [TestCase(BonusType.FundIn, BonusRewardType.Amount, ExpectedResult = true)]
        [TestCase(BonusType.FundIn, BonusRewardType.Percentage, ExpectedResult = true)]
        [TestCase(BonusType.FundIn, BonusRewardType.TieredAmount, ExpectedResult = true)]
        [TestCase(BonusType.FundIn, BonusRewardType.TieredPercentage, ExpectedResult = true)]
        [TestCase(BonusType.MobilePlusEmailVerification, BonusRewardType.Amount, ExpectedResult = true)]
        [TestCase(BonusType.MobilePlusEmailVerification, BonusRewardType.Percentage, ExpectedResult = false)]
        [TestCase(BonusType.MobilePlusEmailVerification, BonusRewardType.TieredAmount, ExpectedResult = false)]
        [TestCase(BonusType.MobilePlusEmailVerification, BonusRewardType.TieredPercentage, ExpectedResult = false)]
        [TestCase(BonusType.ReferFriend, BonusRewardType.Amount, ExpectedResult = false)]
        [TestCase(BonusType.ReferFriend, BonusRewardType.Percentage, ExpectedResult = false)]
        [TestCase(BonusType.ReferFriend, BonusRewardType.TieredAmount, ExpectedResult = true)]
        [TestCase(BonusType.ReferFriend, BonusRewardType.TieredPercentage, ExpectedResult = false)]
        public bool Reward_type_validation_works(BonusType bonusType, BonusRewardType rewardType)
        {
            _vm.Info.TemplateType = bonusType.ToString();
            _vm.Rules.RewardType = rewardType;
            _vm.Rules.FundInWallets = _brand.WalletTemplates.Select(w => w.Id).ToList();
            if (rewardType == BonusRewardType.TieredAmount || rewardType == BonusRewardType.TieredPercentage)
            {
                if (bonusType == BonusType.HighDeposit)
                {
                    _vm.Rules.RewardTiers.Single().BonusTiers = new List<TemplateTierVM>
                    {
                        new TemplateTierVM {Reward = 1, From = 1, NotificationPercentThreshold = 1}
                    };
                }
                else
                {
                    _vm.Rules.RewardTiers.Single().BonusTiers = new List<TemplateTierVM>
                    {
                        new TemplateTierVM {From = 1, To = 2, Reward = 100}
                    };
                }
            }

            var buildResult = _bonusBuilder.BuildTemplate(_vm);
            if (buildResult.IsValid == false)
            {
                Assert.True(
                    buildResult.Errors.Any(
                        error => error.ErrorMessage == ValidatorMessages.TemplateRewardTypeIsNotSupported));
            }

            return buildResult.IsValid;
        }

        [Test]
        public void Bonus_tiers_at_least_one_is_required_validation_works()
        {
            _vm.Rules.RewardType = BonusRewardType.TieredAmount;
            _vm.Rules.RewardTiers.Single().BonusTiers = new List<TemplateTierVM>();

            var buildResult = _bonusBuilder.BuildTemplate(_vm);
            Assert.AreEqual(false, buildResult.IsValid);

            var message = buildResult.Errors.Single().ErrorMessage;
            Assert.AreEqual(ValidatorMessages.TemplateBonusTierAtLeastOneIsRequired, message);
        }

        [Test]
        public void Bonus_tiers_invalid_tier_validation_works()
        {
            _vm.Rules.RewardType = BonusRewardType.TieredAmount;
            _vm.Rules.RewardTiers.Single().BonusTiers = new List<TemplateTierVM>
            {
                new TemplateTierVM {From = 100, To = 50, Reward = 10},
                new TemplateTierVM {From = 101, To = 200, Reward = 20},
                new TemplateTierVM {From = 201, To = 300, Reward = 30}
            };
            var buildResult = _bonusBuilder.BuildTemplate(_vm);
            Assert.AreEqual(false, buildResult.IsValid);

            var message = buildResult.Errors.Single().ErrorMessage;
            Assert.AreEqual(ValidatorMessages.TemplateBonusTierIsInvalid, message);
        }

        [Test]
        public void Bonus_tiers_overlap_validation_works()
        {
            _vm.Rules.RewardType = BonusRewardType.TieredAmount;

            _vm.Rules.RewardTiers.Single().BonusTiers = new List<TemplateTierVM>
            {
                new TemplateTierVM {From = 10, Reward = 10},
                new TemplateTierVM {From = 5, Reward = 20},
                new TemplateTierVM {From = 100, Reward = 30}
            };
            var buildResult = _bonusBuilder.BuildTemplate(_vm);
            Assert.False(buildResult.IsValid);

            var message = buildResult.Errors.Single().ErrorMessage;
            Assert.AreEqual(ValidatorMessages.TemplateBonusTiersOverlap, message);
        }

        [Test]
        public void At_least_one_high_deposit_tier_is_required_validation_works()
        {
            _vm.Info.TemplateType = BonusType.HighDeposit.ToString();
            _vm.Rules.RewardType = BonusRewardType.TieredAmount;
            _vm.Rules.RewardTiers.Single().BonusTiers = new List<TemplateTierVM>();

            var buildResult = _bonusBuilder.BuildTemplate(_vm);
            Assert.AreEqual(false, buildResult.IsValid);

            var message = buildResult.Errors.Single().ErrorMessage;
            Assert.AreEqual(ValidatorMessages.TemplateBonusTierAtLeastOneIsRequired, message);
        }

        [Test]
        public void Deposit_duplicate_high_deposit_tiers_validation_works()
        {
            _vm.Info.TemplateType = BonusType.HighDeposit.ToString();
            _vm.Rules.RewardType = BonusRewardType.TieredAmount;
            _vm.Rules.RewardTiers.Single().BonusTiers = new List<TemplateTierVM>
            {
                new TemplateTierVM {From = 100, Reward = 10},
                new TemplateTierVM {From = 200, Reward = 10},
                new TemplateTierVM {From = 200, Reward = 10}
            };
            var buildResult = _bonusBuilder.BuildTemplate(_vm);
            Assert.AreEqual(false, buildResult.IsValid);

            var message = buildResult.Errors.Single().ErrorMessage;
            Assert.AreEqual(ValidatorMessages.TemplateDuplicateHighDepositTiers, message);
        }

        [Test]
        public void Parent_bonus_is_absent_validation_works()
        {
            _vm.Availability = new TemplateAvailabilityVM { ParentBonusId = Guid.NewGuid() };

            var buildResult = _bonusBuilder.BuildTemplate(_vm);
            Assert.AreEqual(false, buildResult.IsValid);

            var message = buildResult.Errors.Single().ErrorMessage;
            Assert.AreEqual(ValidatorMessages.TemplateParentBonusDoesNotExist, message);
        }

        [Test]
        public void Player_registration_date_range_validation_works()
        {
            _vm.Availability = new TemplateAvailabilityVM
            {
                PlayerRegistrationDateFrom = DateTimeOffset.Now.AddDays(-3).ToString("d"),
                PlayerRegistrationDateTo = DateTimeOffset.Now.AddDays(-4).ToString("d")
            };

            var buildResult = _bonusBuilder.BuildTemplate(_vm);
            Assert.AreEqual(false, buildResult.IsValid);

            var message = buildResult.Errors.Single().ErrorMessage;
            Assert.AreEqual(ValidatorMessages.TemplatePlayerRegistrationDateRangeIsInvalid, message);
        }

        [Test]
        public void Reward_amount_validation_works()
        {
            _vm.Rules.RewardTiers.Single().BonusTiers.Single().Reward = 0;
            var buildResult = _bonusBuilder.BuildTemplate(_vm);
            Assert.AreEqual(false, buildResult.IsValid);

            var message = buildResult.Errors.Single().ErrorMessage;
            Assert.AreEqual(ValidatorMessages.TemplateRewardValueIsInvalid, message);
        }

        [Test]
        public void Exclude_bonuses_contain_parent_bonus_validation_works()
        {
            var parentId = Guid.NewGuid();
            _vm.Availability = new TemplateAvailabilityVM
            {
                ParentBonusId = parentId,
                ExcludeBonuses = new List<Guid>
                {
                    parentId
                }
            };

            var buildResult = _bonusBuilder.BuildTemplate(_vm);
            Assert.AreEqual(false, buildResult.IsValid);

            Assert.True(buildResult.Errors.Any(a => a.ErrorMessage == ValidatorMessages.TemplateBonusExcludesContainsParentBonus));
        }

        [Test]
        public void Fund_in_no_wallets_validation_works()
        {
            _vm.Info.TemplateType = BonusType.FundIn.ToString();

            var buildResult = _bonusBuilder.BuildTemplate(_vm);
            Assert.AreEqual(false, buildResult.IsValid);

            var message = buildResult.Errors.Single().ErrorMessage;
            Assert.AreEqual(ValidatorMessages.TemplateNoFundInWallets, message);
        }

        [Test]
        public void Fund_in_wallets_should_be_brand_related()
        {
            _vm.Info.TemplateType = BonusType.FundIn.ToString();
            _vm.Rules.FundInWallets = new List<Guid> { Guid.NewGuid() };

            var buildResult = _bonusBuilder.BuildTemplate(_vm);
            Assert.AreEqual(false, buildResult.IsValid);

            var message = buildResult.Errors.Single().ErrorMessage;
            Assert.AreEqual(ValidatorMessages.TemplateFundInWalletsAreInvalid, message);
        }

        [Test]
        public void Player_redemption_limit_validation_works()
        {
            _vm.Availability = new TemplateAvailabilityVM { PlayerRedemptionsLimit = -1 };

            var buildResult = _bonusBuilder.BuildTemplate(_vm);
            Assert.AreEqual(false, buildResult.IsValid);

            var message = buildResult.Errors.Single().ErrorMessage;
            Assert.AreEqual(ValidatorMessages.TemplatePlayerRedemptionsIsNegative, message);
        }

        [Test]
        public void Can_not_update_non_existing_template()
        {
            _vm.Id = Guid.NewGuid();

            var buildResult = _bonusBuilder.BuildTemplate(_vm);
            Assert.False(buildResult.IsValid);
            Assert.AreEqual(1, buildResult.Errors.Count(e => e.ErrorMessage == ValidatorMessages.TemplateDoesNotExist));
        }

        [Test]
        public void Can_not_update_not_current_version_template()
        {
            var id = Guid.NewGuid();
            var repository = Container.Resolve<IBonusRepository>();
            repository.Templates.Add(new Template
            {
                Id = id,
                Version = 1000,
                Info = new TemplateInfo(),
                Rules = new TemplateRules(),
                Availability = new TemplateAvailability(),
                Wagering = new TemplateWagering()
            });

            _vm.Id = id;

            var buildResult = _bonusBuilder.BuildTemplate(_vm);
            Assert.False(buildResult.IsValid);
            Assert.AreEqual(1, buildResult.Errors.Count(e => e.ErrorMessage == ValidatorMessages.TemplateVersionIsNotCurrent));
        }

        [Test]
        public void Can_build_new_template_info()
        {
            var uiData = new TemplateVM
            {
                Info = new TemplateInfoVM
                {
                    Name = TestDataGenerator.GetRandomString(),
                    BrandId = _brand.Id,
                    WalletTemplateId = _brand.WalletTemplates.Single().Id,
                    TemplateType = BonusType.FirstDeposit.ToString(),
                    Mode = IssuanceMode.Automatic.ToString()
                }
            };

            var result = _bonusBuilder.BuildTemplate(uiData);
            Assert.IsEmpty(result.Errors);

            var template = result.Entity;

            Assert.Null(template.Availability);
            Assert.Null(template.Rules);
            Assert.Null(template.Notification);
            Assert.Null(template.Wagering);

            Assert.NotNull(template.Info);
            Assert.AreEqual(_brand.Id, template.Info.Brand.Id);
        }

        [Test]
        public void Can_attach_availability_to_template_with_info()
        {
            var uiData = new TemplateVM
            {
                Info = new TemplateInfoVM
                {
                    Name = TestDataGenerator.GetRandomString(),
                    BrandId = _brand.Id,
                    WalletTemplateId = _brand.WalletTemplates.Single().Id,
                    TemplateType = BonusType.FirstDeposit.ToString(),
                    Mode = IssuanceMode.Automatic.ToString()
                }
            };

            var result = _bonusBuilder.BuildTemplate(uiData);
            Assert.IsEmpty(result.Errors);

            var template = result.Entity;
            template.Id = Guid.NewGuid();
            BonusRepository.Templates.Add(template);

            uiData = new TemplateVM
            {
                Id = template.Id,
                Availability = new TemplateAvailabilityVM
                {
                    WithinRegistrationDays = 10
                }
            };

            result = _bonusBuilder.BuildTemplate(uiData);
            Assert.IsEmpty(result.Errors);

            template = result.Entity;

            Assert.Null(template.Rules);
            Assert.Null(template.Notification);
            Assert.Null(template.Wagering);

            Assert.NotNull(template.Info);
            Assert.NotNull(template.Availability);
            Assert.AreEqual(uiData.Availability.WithinRegistrationDays, template.Availability.WithinRegistrationDays);
        }

        [Test]
        public void Template_should_have_receiving_wallet()
        {
            _vm.Info.WalletTemplateId = Guid.Empty;

            var buildResult = _bonusBuilder.BuildTemplate(_vm);
            Assert.AreEqual(false, buildResult.IsValid);

            var message = buildResult.Errors.Single().ErrorMessage;
            Assert.AreEqual(ValidatorMessages.TemplateReceivingWalletIsNotSpecified, message);
        }

        [Test]
        public void Template_should_have_receiving_wallet_that_is_brand_related()
        {
            _vm.Info.WalletTemplateId = Guid.NewGuid();

            var buildResult = _bonusBuilder.BuildTemplate(_vm);
            Assert.AreEqual(false, buildResult.IsValid);

            var message = buildResult.Errors.Single().ErrorMessage;
            Assert.AreEqual(ValidatorMessages.TemplateReceivingWalletIsInvalid, message);
        }

        [Test]
        public void Template_with_no_rollover_can_not_be_issued_after_wagering()
        {
            _vm.Wagering = new TemplateWageringVM
            {
                HasWagering = false,
                IsAfterWager = true
            };

            var buildResult = _bonusBuilder.BuildTemplate(_vm);
            Assert.AreEqual(false, buildResult.IsValid);

            var message = buildResult.Errors.Single().ErrorMessage;
            Assert.AreEqual(ValidatorMessages.TemplateWageringIsAfterWagerIsNotApplicable, message);
        }

        [TestCase(BonusType.ReloadDeposit, ExpectedResult = true)]
        [TestCase(BonusType.FirstDeposit, ExpectedResult = true)]
        [TestCase(BonusType.FundIn, ExpectedResult = true)]
        [TestCase(BonusType.HighDeposit, ExpectedResult = false)]
        [TestCase(BonusType.MobilePlusEmailVerification, ExpectedResult = false)]
        [TestCase(BonusType.ReferFriend, ExpectedResult = false)]
        public bool IssuanceMode_with_template_type_compartability_validation_works(BonusType bonusType)
        {
            _vm.Info.TemplateType = bonusType.ToString();
            _vm.Info.Mode = IssuanceMode.AutomaticWithCode.ToString();
            _vm.Rules.FundInWallets = _brand.WalletTemplates.Select(w => w.Id).ToList();

            var buildResult = _bonusBuilder.BuildTemplate(_vm);
            if (buildResult.IsValid == false)
            {
                Assert.True(
                    buildResult.Errors.Any(
                        error =>
                            error.ErrorMessage == ValidatorMessages.TemplateModeIsIncorrect));
            }

            return buildResult.IsValid;
        }

        [Test]
        public void Template_with_currencies_of_bonus_tiers_that_are_not_brand_related_is_invalid()
        {
            _vm.Rules.RewardTiers.Add(new RewardTierVM
            {
                CurrencyCode = "InvalidCode",
                BonusTiers = new List<TemplateTierVM>
                            {
                                new TemplateTierVM {Reward = 25}
                            }
            });

            var buildResult = _bonusBuilder.BuildTemplate(_vm);
            Assert.AreEqual(false, buildResult.IsValid);

            var message = buildResult.Errors.Single().ErrorMessage;
            Assert.AreEqual(ValidatorMessages.TemplateRewardCurrenciesAreInvalid, message);
        }

        [Test]
        public void Template_VIPs_are_not_brand_related_validation_works()
        {
            _vm.Availability = new TemplateAvailabilityVM
            {
                VipLevels = new List<string> { TestDataGenerator.GetRandomString() }
            };

            var buildResult = _bonusBuilder.BuildTemplate(_vm);
            Assert.AreEqual(false, buildResult.IsValid);

            var message = buildResult.Errors.Single().ErrorMessage;
            Assert.AreEqual(ValidatorMessages.TemplateVipsAreInvalid, message);
        }

        [Test]
        public void Template_RiskLevels_are_not_brand_related_validation_works()
        {
            _vm.Availability = new TemplateAvailabilityVM
            {
                ExcludeRiskLevels = new List<Guid> { Guid.NewGuid() }
            };

            var buildResult = _bonusBuilder.BuildTemplate(_vm);
            Assert.AreEqual(false, buildResult.IsValid);

            var message = buildResult.Errors.Single().ErrorMessage;
            Assert.AreEqual(ValidatorMessages.TemplateRiskLevelsAreInvalid, message);
        }
    }
}