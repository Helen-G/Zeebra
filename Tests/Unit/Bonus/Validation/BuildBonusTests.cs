using System;
using System.Linq;
using AFT.RegoV2.Core.Bonus;
using AFT.RegoV2.Core.Bonus.Data;
using AFT.RegoV2.Core.Bonus.Data.ViewModels;
using AFT.RegoV2.Core.Bonus.DomainServices;
using AFT.RegoV2.Core.Bonus.Resources;
using AFT.RegoV2.Core.Common.Utils;
using AFT.RegoV2.Tests.Common;
using AFT.RegoV2.Tests.Common.Base;
using AFT.RegoV2.Tests.Common.Helpers;
using AFT.RegoV2.WinService.Workers;
using Microsoft.Practices.Unity;
using NUnit.Framework;

namespace AFT.RegoV2.Tests.Unit.Bonus.Validation
{
    class BuildBonusTests : AdminWebsiteUnitTestsBase
    {
        private BonusBuilder _bonusBuilder;
        private BonusVM _vm;
        private BonusTestHelper _bonusHelper;
        private string _brandTimezoneId;

        public override void BeforeEach()
        {
            base.BeforeEach();
            Container.Resolve<BonusWorker>().Start();
            _bonusHelper = Container.Resolve<BonusTestHelper>();
            _bonusBuilder = Container.Resolve<BonusBuilder>();

            Container.Resolve<SecurityTestHelper>().SignInUser();
            Container.Resolve<BrandTestHelper>().CreateBrand(isActive: true);
            var template = _bonusHelper.CreateFirstDepositTemplate();
            template.Rules.RewardTiers.Single().BonusTiers.Single().Reward = 100;
            _brandTimezoneId = template.Info.Brand.TimezoneId;
            _vm = new BonusVM
            {
                Name = TestDataGenerator.GetRandomString(),
                Code = TestDataGenerator.GetRandomString(),
                TemplateId = template.Id,
                TemplateVersion = template.Version,
                ActiveFrom = DateTimeOffset.Now.ToBrandOffset(_brandTimezoneId).ToString("d"),
                ActiveTo = DateTimeOffset.Now.ToBrandOffset(_brandTimezoneId).AddDays(1).ToString("d"),
                DurationType = DurationType.None.ToString()
            };
        }

        [Test]
        public void Activity_dates_forms_range_validation_works()
        {
            _vm.ActiveFrom = DateTimeOffset.Now.ToBrandOffset(_brandTimezoneId).AddDays(2).ToString("d");

            var buildResult = _bonusBuilder.BuildBonus(_vm);
            Assert.AreEqual(false, buildResult.IsValid);

            var message = buildResult.Errors.Single().ErrorMessage;
            Assert.AreEqual(ValidatorMessages.BonusActivityRangeIsInvalid, message);
        }

        [Test]
        public void Bonus_activeTo_forms_active_daterange_validation_works()
        {
            _vm.ActiveFrom = DateTimeOffset.Now.AddDays(-2).ToBrandOffset(_brandTimezoneId).ToString("d");
            _vm.ActiveTo = DateTimeOffset.Now.AddDays(-1).ToBrandOffset(_brandTimezoneId).ToString("d");

            var buildResult = _bonusBuilder.BuildBonus(_vm);
            Assert.AreEqual(false, buildResult.IsValid);

            var message = buildResult.Errors.Single().ErrorMessage;
            Assert.AreEqual(ValidatorMessages.BonusActiveToIsInvalid, message);
        }

        [Test]
        public void Name_is_required_validation_works()
        {
            _vm.Name = null;

            var buildResult = _bonusBuilder.BuildBonus(_vm);
            Assert.AreEqual(false, buildResult.IsValid);

            var message = buildResult.Errors.Single().ErrorMessage;
            Assert.AreEqual(ValidatorMessages.NameIsNotSpecified, message);
        }

        [TestCase(1, true)]
        [TestCase(50, true)]
        [TestCase(51, false)]
        public void Name_length_validation_works(int length, bool isValid)
        {
            _vm.Name = TestDataGenerator.GetRandomString(length);

            var buildResult = _bonusBuilder.BuildBonus(_vm);
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
            repository.Bonuses.Add(new Core.Bonus.Data.Bonus { Name = name });

            _vm.Name = name;

            var buildResult = _bonusBuilder.BuildBonus(_vm);
            Assert.AreEqual(false, buildResult.IsValid);

            var message = buildResult.Errors.Single().ErrorMessage;
            Assert.AreEqual(ValidatorMessages.NameIsNotUnique, message);
        }

        [Test]
        public void Name_uniqueness_during_edit_validation_works()
        {
            var id = Guid.NewGuid();
            const string name = "ASD";
            var repository = Container.Resolve<IBonusRepository>();
            repository.Bonuses.Add(new Core.Bonus.Data.Bonus { Id = id, Name = name });
            repository.Bonuses.Add(new Core.Bonus.Data.Bonus { Id = Guid.NewGuid(), Name = name });

            _vm.Id = id;
            _vm.Name = name;

            var buildResult = _bonusBuilder.BuildBonus(_vm);
            Assert.AreEqual(false, buildResult.IsValid);

            var message = buildResult.Errors.Single().ErrorMessage;
            Assert.AreEqual(ValidatorMessages.NameIsNotUnique, message);
        }

        [TestCase(1, true)]
        [TestCase(200, true)]
        [TestCase(201, false)]
        public void Description_length_validation_works(int length, bool isValid)
        {
            _vm.Description = TestDataGenerator.GetRandomString(length);

            var buildResult = _bonusBuilder.BuildBonus(_vm);
            Assert.AreEqual(isValid, buildResult.IsValid);

            if (isValid == false)
            {
                var message = buildResult.Errors.Single().ErrorMessage;
                Assert.AreEqual(string.Format(ValidatorMessages.DescriptionLengthIsInvalid, 1, 200), message);
            }
        }

        [Test]
        public void Template_does_not_exist_validation_works()
        {
            _vm.TemplateId = Guid.NewGuid();

            var buildResult = _bonusBuilder.BuildBonus(_vm);
            Assert.AreEqual(false, buildResult.IsValid);

            var message = buildResult.Errors.Single().ErrorMessage;
            Assert.AreEqual(ValidatorMessages.BonusTemplateIsNotAssigned, message);
        }

        [Test]
        public void Code_is_required_validation_works_for_AutomaticWithBonusCode_issuance_mode()
        {
            var template = _bonusHelper.CreateFirstDepositTemplate(mode: IssuanceMode.AutomaticWithCode);
            _vm.TemplateId = template.Id;
            _vm.Code = null;

            var buildResult = _bonusBuilder.BuildBonus(_vm);
            Assert.AreEqual(false, buildResult.IsValid);

            var message = buildResult.Errors.Single().ErrorMessage;
            Assert.AreEqual(ValidatorMessages.BonusCodeIsNotSpecified, message);
        }

        [Test]
        public void Code_contain_no_special_characters_validation_works()
        {
            _vm.Code = "Foo Bar$";

            var buildResult = _bonusBuilder.BuildBonus(_vm);
            Assert.AreEqual(false, buildResult.IsValid);

            var message = buildResult.Errors.Single().ErrorMessage;
            Assert.AreEqual(ValidatorMessages.BonusCodeIsInvalid, message);
        }

        [TestCase(1, true)]
        [TestCase(20, true)]
        [TestCase(21, false)]
        public void Code_length_validation_works(int length, bool isValid)
        {
            _vm.Code = TestDataGenerator.GetRandomString(length);

            var buildResult = _bonusBuilder.BuildBonus(_vm);
            Assert.AreEqual(isValid, buildResult.IsValid);

            if (isValid == false)
            {
                var message = buildResult.Errors.Single().ErrorMessage;
                Assert.AreEqual(string.Format(ValidatorMessages.BonusCodeLengthIsInvalid, 1, 20), message);
            }
        }

        [Test]
        public void Code_uniqueness_during_add_validation_works()
        {
            const string code = "ASD";
            var repository = Container.Resolve<IBonusRepository>();
            repository.Bonuses.Add(new Core.Bonus.Data.Bonus { Code = code });

            _vm.Code = code;

            var buildResult = _bonusBuilder.BuildBonus(_vm);
            Assert.AreEqual(false, buildResult.IsValid);

            var message = buildResult.Errors.Single().ErrorMessage;
            Assert.AreEqual(ValidatorMessages.BonusCodeIsNotUnique, message);
        }

        [Test]
        public void Code_uniqueness_during_edit_validation_works()
        {
            var id = Guid.NewGuid();
            const string code = "ASD";
            var repository = Container.Resolve<IBonusRepository>();
            repository.Bonuses.Add(new Core.Bonus.Data.Bonus { Id = id, Code = code });
            repository.Bonuses.Add(new Core.Bonus.Data.Bonus { Id = Guid.NewGuid(), Code = code });

            _vm.Id = id;
            _vm.Code = code;

            var buildResult = _bonusBuilder.BuildBonus(_vm);
            Assert.AreEqual(false, buildResult.IsValid);

            var message = buildResult.Errors.Single().ErrorMessage;
            Assert.AreEqual(ValidatorMessages.BonusCodeIsNotUnique, message);
        }

        [Test]
        public void Days_to_claim_validation_works()
        {
            _vm.DaysToClaim = -1;

            var buildResult = _bonusBuilder.BuildBonus(_vm);
            Assert.AreEqual(false, buildResult.IsValid);

            var message = buildResult.Errors.Single().ErrorMessage;
            Assert.AreEqual(ValidatorMessages.BonusDaysToClaimIsNegative, message);
        }

        [Test]
        public void Duration_based_on_active_from_date_correctly_translates_to_duration_end_date()
        {
            _vm.ActiveTo = DateTimeOffset.Now.ToBrandOffset(_brandTimezoneId).AddDays(2).ToString("d");
            _vm.DurationType = DurationType.StartDateBased.ToString();
            _vm.DurationDays = 1;
            _vm.DurationHours = 2;
            _vm.DurationMinutes = 3;

            var buildResult = _bonusBuilder.BuildBonus(_vm);
            var expectedDurationEnd = buildResult.Entity.ActiveFrom
                .AddDays(_vm.DurationDays)
                .AddHours(_vm.DurationHours)
                .AddMinutes(_vm.DurationMinutes);

            Assert.AreEqual(expectedDurationEnd, buildResult.Entity.DurationEnd);
        }

        [Test]
        public void Zero_length_duration_validation_works()
        {
            _vm.DurationType = DurationType.StartDateBased.ToString();

            var buildResult = _bonusBuilder.BuildBonus(_vm);
            Assert.AreEqual(false, buildResult.IsValid);

            var message = buildResult.Errors.Single().ErrorMessage;
            Assert.AreEqual(ValidatorMessages.BonusDurationIsZeroLength, message);
        }

        [Test]
        public void Duration_is_over_activity_date_range_validation_works()
        {
            _vm.DurationType = DurationType.StartDateBased.ToString();
            _vm.DurationDays = 5;

            var buildResult = _bonusBuilder.BuildBonus(_vm);
            Assert.AreEqual(false, buildResult.IsValid);

            var message = buildResult.Errors.Single().ErrorMessage;
            Assert.AreEqual(ValidatorMessages.BonusDurationDaterangeIsIncorrect, message);
        }

        [Test]
        public void Duration_dates_out_of_acivity_date_range_validation_works()
        {
            _vm.DurationType = DurationType.Custom.ToString();
            _vm.DurationStart = DateTimeOffset.Now.ToBrandOffset(_brandTimezoneId).ToString("g");
            _vm.DurationEnd = DateTimeOffset.Now.AddDays(2).ToBrandOffset(_brandTimezoneId).ToString("g");

            var buildResult = _bonusBuilder.BuildBonus(_vm);
            Assert.AreEqual(false, buildResult.IsValid);

            var message = buildResult.Errors.Single().ErrorMessage;
            Assert.AreEqual(ValidatorMessages.BonusDurationDaterangeIsIncorrect, message);
        }

        [Test]
        public void Can_not_update_non_existing_bonus()
        {
            _vm.Id = Guid.NewGuid();

            var buildResult = _bonusBuilder.BuildBonus(_vm);
            Assert.False(buildResult.IsValid);
            Assert.AreEqual(ValidatorMessages.BonusDoesNotExist, buildResult.Errors.Single().ErrorMessage);
        }

        [Test]
        public void Can_not_update_not_current_version_bonus()
        {
            var id = Guid.NewGuid();
            var repository = Container.Resolve<IBonusRepository>();
            repository.Bonuses.Add(new Core.Bonus.Data.Bonus { Id = id, Version = 1000 });

            _vm.Id = id;

            var buildResult = _bonusBuilder.BuildBonus(_vm);
            Assert.False(buildResult.IsValid);
            Assert.AreEqual(ValidatorMessages.BonusVersionIsNotCurrent, buildResult.Errors.Single().ErrorMessage);
        }
    }
}