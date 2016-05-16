using System;
using System.Linq;
using AFT.RegoV2.Core.Brand;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Fraud.ApplicationServices;
using AFT.RegoV2.Core.Fraud.Data;
using AFT.RegoV2.Shared;
using AFT.RegoV2.Tests.Common.Base;
using AFT.RegoV2.Tests.Common.Helpers;
using AFT.RegoV2.Tests.Common.TestDoubles;
using AFT.RegoV2.WinService.Workers;
using Microsoft.Practices.Unity;
using NUnit.Framework;
using ServiceStack.Validation;

namespace AFT.RegoV2.Tests.Unit.Fraud
{
    public class RiskLevelTests : AdminWebsiteUnitTestsBase
    {
        private IRiskLevelQueries _riskQueries;
        private IRiskLevelCommands _riskCommands;

        public override void BeforeEach()
        {
            base.BeforeEach();

            this._riskQueries = Container.Resolve<IRiskLevelQueries>();
            this._riskCommands = Container.Resolve<IRiskLevelCommands>();

            Container.Resolve<SecurityTestHelper>().SignInUser();
            Container.Resolve<RiskLevelWorker>().Start();
        }

        private void CreateRiskLevel(Guid id)
        {
            Container.Resolve<BrandTestHelper>().CreateBrand(isActive: true);

            var entity = new RiskLevel();
            entity.Id = id;
            entity.BrandId = Container.Resolve<IBrandRepository>().Brands.First().Id;
            entity.Name = "dao_test";
            entity.Level = 1001;
            entity.Description = "remarks";

            this._riskCommands.Create(entity);
        }

        private void UpdateStatusTest(Guid id, Status expectedStatus, string expectedRemarks)
        {
            if (expectedStatus == Status.Active)
            {
                this._riskCommands.Activate(id, expectedRemarks);
            }
            else
            {
                this._riskCommands.Deactivate(id, expectedRemarks);
            }

            var newRisk = this._riskQueries.GetById(id);

            Assert.IsNotNull(newRisk);
            Assert.AreEqual(expectedStatus, newRisk.Status);
            Assert.AreEqual(expectedRemarks, newRisk.Description);
        }

        [Test]
        public void Can_Create_RiskLevel()
        {
            //Container.Resolve<ISharedData>().SetUser(new User()
            //{
            //    Username = "SuperAdmin"
            //});

            var id = Guid.NewGuid();
            this.CreateRiskLevel(id);

            Assert.IsNotNull(this._riskQueries.GetById(id));
        }

        [Test]
        public void Can_Update_RiskLevel()
        {
            Guid id = Guid.NewGuid();
            this.CreateRiskLevel(id);

            var risk = this._riskQueries.GetById(id);
            risk.Name = "dao_edit";
            risk.Description = "edit remarks";

            this._riskCommands.Update(risk);

            var newRisk = this._riskQueries.GetById(id);
            Assert.IsNotNull(newRisk);
            Assert.AreEqual(risk.Name, newRisk.Name);
            Assert.AreEqual(risk.Description, newRisk.Description);
        }

        [Test, ExpectedException(typeof(RegoValidationException))]
        public void Cannot_Update_RiskLevel()
        {
            Guid id = Guid.NewGuid();
            this.CreateRiskLevel(id);

            var risk = this._riskQueries.GetById(id);
            risk.Description = "\"Long, Long Ago\" is a song dealing with nostalgia, written in 1833 by English composer Thomas Haynes Bayly. Originally called \"The Long Ago\", its name was apparently changed by the editor Rufus Wilmot Griswold when it was first published, posthumously, in a Philadelphia magazine, along with a collection of other songs and poems by Bayly. The song was well received, and became one of the most popular songs in the United States in 1844.";

            this._riskCommands.Update(risk);
        }

        [Test]
        public void Can_Activate_Or_Deactivate_RiskLevel()
        {
            Guid id = Guid.NewGuid();
            this.CreateRiskLevel(id);

            // deactivate
            var expectedStatus = Status.Inactive;
            var expectedRemarks = "deactivate remarks";
            this.UpdateStatusTest(id, expectedStatus, expectedRemarks);

            // activate
            expectedStatus = Status.Active;
            expectedRemarks = "activate remarks";
            this.UpdateStatusTest(id, expectedStatus, expectedRemarks);
        }
    }
}
