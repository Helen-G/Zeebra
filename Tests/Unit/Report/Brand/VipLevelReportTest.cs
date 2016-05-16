using System;
using System.Linq;
using System.Text;
using AFT.RegoV2.AdminWebsite.Controllers;
using AFT.RegoV2.ApplicationServices.Report;
using AFT.RegoV2.BoundedContexts.Report;
using AFT.RegoV2.BoundedContexts.Report.Data;
using AFT.RegoV2.Core.Brand;
using AFT.RegoV2.Core.Brand.ApplicationServices;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Game;
using AFT.RegoV2.Core.Game.Interfaces;
using AFT.RegoV2.Tests.Common;
using AFT.RegoV2.Tests.Common.Base;
using AFT.RegoV2.Tests.Common.Helpers;
using AFT.RegoV2.WinService.Workers;
using Microsoft.Practices.Unity;
using NUnit.Framework;

namespace AFT.RegoV2.Tests.Unit.Report.Brand
{
    internal class VipLevelReportTest : ReportsTestsBase
    {
        private IReportRepository _reportRepository;
        private ReportQueries _reportQueries;
        private BrandCommands _brandCommands;
        private IBrandRepository _brandRepository;
        private IGameRepository _gameRepository;
        private GamesTestHelper _gamesTestHelper;

        public override void BeforeEach()
        {
            base.BeforeEach();
            _reportRepository = Container.Resolve<IReportRepository>();
            _reportQueries = Container.Resolve<ReportQueries>();
            _brandCommands = Container.Resolve<BrandCommands>();
            _brandRepository = Container.Resolve<IBrandRepository>();
            _gameRepository = Container.Resolve<IGameRepository>();
            _gamesTestHelper = Container.Resolve<GamesTestHelper>();
        }

        protected override void StartWorkers()
        {
            Container.Resolve<VipLevelReportWorker>().Start();
        }

        [Test]
        public void Can_process_VipLevel_registered()
        {
            // Arrange
            var oldVipLevelRecordCount = _reportRepository.VipLevelRecords.Count();

            // Act
            var vipLevel = BrandTestHelper.CreateVipLevel(CurrentBrand.Id, isDefault: false);

            // Assert
            Assert.AreEqual(oldVipLevelRecordCount + 1, _reportRepository.VipLevelRecords.Count());
            var record = _reportRepository.VipLevelRecords.Last();
            Assert.AreEqual(vipLevel.Id, record.VipLevelId);
            Assert.AreEqual(null, record.VipLevelLimitId);
            Assert.AreEqual(vipLevel.Brand.Licensee.Name, record.Licensee);
            Assert.AreEqual(vipLevel.Brand.Name, record.Brand);
            Assert.AreEqual(vipLevel.Code, record.Code);
            Assert.AreEqual(vipLevel.Rank, record.Rank);
            Assert.AreNotEqual(vipLevel.Id, CurrentBrand.DefaultVipLevelId);
            Assert.AreEqual(vipLevel.Status.ToString(), record.Status);
            Assert.AreEqual(null, record.GameProvider);
            Assert.AreEqual(null, record.Currency);
            Assert.AreEqual(null, record.BetLevel);
            Assert.AreEqual(vipLevel.DateCreated, record.Created);
            Assert.AreEqual(vipLevel.CreatedBy, record.CreatedBy);
        }

        [Test]
        public void Can_process_VipLevel_registered_with_vip_level_limits()
        {
            // Arrange
            const int vipLevelLimitCount = 3;
            var oldVipLevelRecordCount = _reportRepository.VipLevelRecords.Count();

            // Act
            var vipLevel = BrandTestHelper.CreateVipLevel(CurrentBrand.Id, vipLevelLimitCount, false);

            // Assert
            Assert.AreEqual(oldVipLevelRecordCount + vipLevelLimitCount, _reportRepository.VipLevelRecords.Count());
            var records = _reportRepository.VipLevelRecords.Skip(oldVipLevelRecordCount);
            var i = 0;
            foreach (var record in records)
            {
                Assert.AreEqual(vipLevel.Id, record.VipLevelId);
                Assert.AreEqual(vipLevel.Brand.Licensee.Name, record.Licensee);
                Assert.AreEqual(vipLevel.Brand.Name, record.Brand);
                Assert.AreEqual(vipLevel.Code, record.Code);
                Assert.AreEqual(vipLevel.Rank, record.Rank);
                Assert.AreNotEqual(vipLevel.Id, CurrentBrand.DefaultVipLevelId);
                Assert.AreEqual(vipLevel.Status.ToString(), record.Status);
                Assert.AreEqual(vipLevel.DateCreated, record.Created);
                Assert.AreEqual(vipLevel.CreatedBy, record.CreatedBy);

                var vipLevelLimit = vipLevel.VipLevelLimits.ElementAt(i);
                Assert.AreEqual(vipLevelLimit.Id, record.VipLevelLimitId);
                Assert.AreEqual(GameProvider(vipLevelLimit.GameProviderId), record.GameProvider);
                Assert.AreEqual(vipLevelLimit.Currency.Code, record.Currency);
                Assert.AreEqual(BetLimit(vipLevelLimit.BetLimitId), record.BetLevel);
                i++;
            }
        }

        [Test]
        public void Can_process_VipLevel_updated()
        {
            // Arrange
            var oldVipLevelRecordCount = _reportRepository.VipLevelRecords.Count();
            var vipLevel = BrandTestHelper.CreateVipLevel(CurrentBrand.Id, 3, false);
            var vipLevelLimit = vipLevel.VipLevelLimits.First();
            _brandCommands.DeactivateVipLevel(vipLevel.Id, "deactivated", null);

            var vipLevelName = TestDataGenerator.GetRandomString();
            int rank;
            do
            {
                rank = TestDataGenerator.GetRandomNumber(100);
            }
            while (_brandRepository.VipLevels.Any(vl => vl.Rank == rank));
            var gameProvider = _gamesTestHelper.CreateGameProvider();
            var editVipLevel = new VipLevelViewModel
            {
                Id = vipLevel.Id,
                Name = vipLevelName,
                Code = vipLevelName.Remove(3),
                Brand = CurrentBrand.Id,
                Rank = rank,
                Limits = new[]
                {
                    new VipLevelLimitViewModel
                    {
                        Id = vipLevelLimit.Id,
                        GameProviderId = vipLevelLimit.GameProviderId,
                        CurrencyCode = vipLevelLimit.Currency.Code,
                        BetLimitId = vipLevelLimit.BetLimitId
                    },
                    new VipLevelLimitViewModel
                    {
                        Id = Guid.NewGuid(),
                        GameProviderId = gameProvider.Id,
                        CurrencyCode = BrandTestHelper.CreateCurrency("UAH", "Hryvnia").Code,
                        BetLimitId = _gamesTestHelper.CreateBetLevel(gameProvider, CurrentBrand.Id).Id
                    }
                }
            };

            // Act
            _brandCommands.EditVipLevel(editVipLevel);

            // Assert
            Assert.AreEqual(oldVipLevelRecordCount + 2, _reportRepository.VipLevelRecords.Count());
            var records = _reportRepository.VipLevelRecords.Skip(oldVipLevelRecordCount);
            var vipLevelLimits = _brandRepository.VipLevels.Single(vl => vl.Id == editVipLevel.Id).VipLevelLimits;
            var i = 0;
            foreach (var record in records)
            {
                Assert.AreEqual(vipLevel.Id, record.VipLevelId);
                Assert.AreEqual(CurrentBrand.Licensee.Name, record.Licensee);
                Assert.AreEqual(CurrentBrand.Name, record.Brand);
                Assert.AreEqual(editVipLevel.Code, record.Code);
                Assert.AreEqual(editVipLevel.Rank, record.Rank);
                Assert.AreNotEqual(editVipLevel.Id, CurrentBrand.DefaultVipLevelId);
                Assert.Less(DateTimeOffset.Now.AddDays(-1), record.Updated);
                Assert.AreEqual(SecurityTestHelper.CurrentUser.UserName, record.UpdatedBy);

                var limit = vipLevelLimits.ElementAt(i);
                Assert.AreEqual(limit.Id, record.VipLevelLimitId);
                Assert.AreEqual(GameProvider(limit.GameProviderId), record.GameProvider);
                Assert.AreEqual(limit.Currency.Code, record.Currency);
                Assert.AreEqual(BetLimit(limit.BetLimitId), record.BetLevel);
                i++;
            }
        }

        [Test]
        public void Can_export_report_data()
        {
            var filteredRecords = ReportController.FilterAndOrder(
                _reportQueries.GetVipLevelRecordsForExport(),
                new VipLevelRecord(),
                "Created", "asc");

            // Act
            var content = Encoding.Unicode.GetString(ReportController.ExportToExcel(filteredRecords));

            // Assert
            Assert.AreNotEqual(content.IndexOf("<table"), -1);
        }

        private string GameProvider(Guid gameProviderId)
        {
            return _gameRepository.GameProviders.Single(gs => gs.Id == gameProviderId).Name;
        }

        private string BetLimit(Guid betLimitId)
        {
            return _gameRepository.BetLimits.Single(bl => bl.Id == betLimitId).Code;
        }
    }
}