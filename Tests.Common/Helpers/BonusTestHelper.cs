using System;
using System.Collections.Generic;
using System.Linq;
using AFT.RegoV2.Core.Bonus;
using AFT.RegoV2.Core.Bonus.ApplicationServices;
using AFT.RegoV2.Core.Bonus.Data;
using AFT.RegoV2.Core.Bonus.Data.ViewModels;
using AFT.RegoV2.Core.Common.Utils;
using AFT.RegoV2.Core.Game.Interfaces;

namespace AFT.RegoV2.Tests.Common.Helpers
{
    public class BonusTestHelper
    {
        private readonly IBonusRepository _bonusRepository;
        private readonly BonusManagementCommands _bonusManagementCommands;
        private readonly GamesTestHelper _gamesTestHelper;
        private readonly PlayerTestHelper _playerTestHelper;
        private readonly PaymentTestHelper _paymentTestHelper;
        private readonly IGameRepository _gameRepository;

        public BonusTestHelper(
            IBonusRepository bonusRepository,
            BonusManagementCommands bonusManagementCommands,
            GamesTestHelper gamesHelper,
            PlayerTestHelper playerTestHelper,
            PaymentTestHelper paymentTestHelper,
            IGameRepository gameRepository)
        {
            _bonusRepository = bonusRepository;
            _bonusManagementCommands = bonusManagementCommands;
            _gamesTestHelper = gamesHelper;
            _playerTestHelper = playerTestHelper;
            _paymentTestHelper = paymentTestHelper;
            _gameRepository = gameRepository;
        }

        public Bonus CreateBasicBonus(bool isActive = true, IssuanceMode mode = IssuanceMode.Automatic)
        {
            return CreateBonus(CreateFirstDepositTemplate(mode: mode), isActive);
        }

        public Bonus CreateBonus(Template bonusTemplate, bool isActive = true)
        {
            var now = DateTimeOffset.Now.ToBrandOffset(bonusTemplate.Info.Brand.TimezoneId);
            var bonus = new Bonus
            {
                Id = Guid.Empty,
                Name = TestDataGenerator.GetRandomString(5),
                Code = TestDataGenerator.GetRandomString(5),
                Template = bonusTemplate,
                IsActive = isActive,
                ActiveFrom = now.Date.ToBrandDateTimeOffset(bonusTemplate.Info.Brand.TimezoneId),
                ActiveTo = now.AddDays(1),
                DurationStart = now.Date.ToBrandDateTimeOffset(bonusTemplate.Info.Brand.TimezoneId),
                DurationEnd = now.AddDays(1)
            };
            _bonusManagementCommands.AddBonus(bonus);

            return bonus;
        }

        public Template CreateFirstDepositTemplate(string name = null, Brand brand = null, IssuanceMode mode = IssuanceMode.Automatic)
        {
            brand = brand ?? _bonusRepository.Brands.First();
            name = name ?? TestDataGenerator.GetRandomString();
            var template = new Template
            {
                Id = Guid.Empty,
                Info = new TemplateInfo
                {
                    Name = name,
                    BonusTrigger = Trigger.Deposit,
                    DepositKind = DepositKind.First,
                    Brand = brand,
                    WalletTemplateId = brand.WalletTemplates.First().Id,
                    Mode = mode
                },
                Availability = new TemplateAvailability(),
                Rules = new TemplateRules
                {
                    RewardTiers = new List<RewardTier>
                    {
                        new RewardTier
                        {
                            CurrencyCode = "CAD",
                            BonusTiers = new List<TierBase> {new BonusTier {Reward = 25, DateCreated = DateTime.Now}}
                        }
                    }
                },
                Wagering = new TemplateWagering(),
                Notification = new TemplateNotification(),
                Status = TemplateStatus.Complete
            };
            _bonusManagementCommands.AddUpdateTemplate(template);

            return template;
        }

        public Template CreateReloadDepositTemplate(string name = null, Brand brand = null, TemplateInfo tempInfo = null, IssuanceMode mode = IssuanceMode.ManualByPlayer)
        {
            brand = brand ?? _bonusRepository.Brands.First();
            name = name ?? TestDataGenerator.GetRandomString();
            //tempInfo = tempInfo ?? new TemplateInfo();
            var template = new Template
            {
                Id = Guid.Empty,
                Info = new TemplateInfo
                {
                    Name = name,
                    BonusTrigger = Trigger.Deposit,
                    DepositKind = DepositKind.Reload,
                    Brand = brand,
                    WalletTemplateId = brand.WalletTemplates.First().Id,
                    Mode = mode 
                },
                Availability = new TemplateAvailability(),
                Rules = new TemplateRules
                {
                    RewardTiers = new List<RewardTier>
                    {
                        new RewardTier
                        {
                            CurrencyCode = "CAD",
                            BonusTiers = new List<TierBase> {new BonusTier {Reward = 25, DateCreated = DateTime.Now}}
                        }
                    }
                },
                Wagering = new TemplateWagering(),
                Notification = new TemplateNotification(),
                Status = TemplateStatus.Complete
            };
            _bonusManagementCommands.AddUpdateTemplate(template);

            return template;
        }


        public void CreateFirstDepositTemplate(string name, string brandName)
        {
            var brand = _bonusRepository.Brands.Single(b => b.Name == brandName);
            CreateFirstDepositTemplate(name, brand);
        }

        public Template CreateFirstDepositWithContributionTemplate(string name = null, Brand brand = null, IssuanceMode mode = IssuanceMode.Automatic, bool withdrawable = true)
        {
            brand = brand ?? _bonusRepository.Brands.First();
            name = name ?? TestDataGenerator.GetRandomString();
            var template = new Template
            {
                Id = Guid.Empty,
                Info = new TemplateInfo
                {
                    Name = name,
                    BonusTrigger = Trigger.Deposit,
                    DepositKind = DepositKind.First,
                    Brand = brand,
                    WalletTemplateId = brand.WalletTemplates.First().Id,
                    IsWithdrawable = withdrawable, 
                    Mode = mode
                },
                Availability = new TemplateAvailability(),
                Rules = new TemplateRules
                {
                    RewardTiers = new List<RewardTier>
                    {
                        new RewardTier
                        {
                            CurrencyCode = "CAD",
                            BonusTiers = new List<TierBase> {new BonusTier {Reward = 13, DateCreated = DateTime.Now}}
                        }
                    }
                },
                Wagering = new TemplateWagering
                {
                    HasWagering = true,
                    Method = WageringMethod.Bonus,
                    Multiplier = 1,
                    GameContributions = new List<GameContribution>
                    {
                       new GameContribution
                       {
                           GameId = _gameRepository.Games.Single(g => g.Name == "Football").Id,
                           Contribution = 0.5m //50%
                       },
                       new GameContribution
                       {
                           GameId = _gameRepository.Games.Single(g => g.Name == "Horses").Id,
                           Contribution = 1m  //100%
                       }
                    }
                },
                Notification = new TemplateNotification(),
                Status = TemplateStatus.Complete
            };
            _bonusManagementCommands.AddUpdateTemplate(template);

            return template;
        }

        public Bonus CreateBonusWithBonusTiers(BonusRewardType rewardType)
        {
            var bonus = CreateBasicBonus();

            bonus.Template.Rules.RewardType = rewardType;
            bonus.Template.Rules.RewardTiers.Single().BonusTiers = (rewardType == BonusRewardType.TieredPercentage
                ? new List<TierBase>
                {
                    new BonusTier {From = 1, Reward = 0.1m, MaxAmount = 40, DateCreated = DateTime.Now},
                    new BonusTier {From = 101, Reward = 0.2m, MaxAmount = 50, DateCreated = DateTime.Now}
                }
                : new List<TierBase>
                {
                    new BonusTier {From = 1, Reward = 10, DateCreated = DateTime.Now},
                    new BonusTier {From = 101, Reward = 20, DateCreated = DateTime.Now},
                    new BonusTier {From = 201, Reward = 30, DateCreated = DateTime.Now}
                });

            return bonus;
        }

        public Bonus CreateBonusWithHighDepositTiers(bool isAutoGenerate = true)
        {
            var bonus = CreateBasicBonus();

            bonus.Template.Rules.RewardType = BonusRewardType.TieredAmount;
            bonus.Template.Info.DepositKind = DepositKind.High;
            bonus.Template.Rules.RewardTiers.Single().BonusTiers = new List<TierBase>
            {
                new HighDepositTier
                {
                    From = 500,
                    Reward = 50,
                    NotificationPercentThreshold = 0.9m
                }
            };
            if (isAutoGenerate)
            {
                bonus.Template.Rules.IsAutoGenerateHighDeposit = true;
            }
            else
            {
                bonus.Template.Rules.RewardTiers.Single().BonusTiers.Add(new HighDepositTier
                {
                    From = 1000,
                    Reward = 100,
                    NotificationPercentThreshold = 0.9m
                });
            }

            return bonus;
        }

        public Bonus CreateBonusWithReferFriendTiers()
        {
            var bonus = CreateBasicBonus();

            bonus.Template.Info.BonusTrigger = Trigger.ReferFriend;
            bonus.Template.Rules.ReferFriendMinDepositAmount = 100;
            bonus.Template.Rules.ReferFriendWageringCondition = 1;
            bonus.Template.Rules.RewardTiers.Single().BonusTiers = new List<TierBase>
            {
                new BonusTier {From = 1, To = null, Reward = 10, DateCreated = DateTime.Now},
                new BonusTier {From = 4, To = null, Reward = 20, DateCreated = DateTime.Now},
                new BonusTier {From = 7, To = null, Reward = 30, DateCreated = DateTime.Now}
            };

            return bonus;
        }

        public void CompleteReferAFriendRequirments(Guid referrerId, Guid gameId)
        {
            var referredId = _playerTestHelper.CreatePlayer(referrerId);
            _paymentTestHelper.MakeDeposit(referredId);
            _gamesTestHelper.PlaceAndLoseBet(200, referredId, gameId);
        }
    }
}