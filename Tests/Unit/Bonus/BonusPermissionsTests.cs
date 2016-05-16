using System;
using AFT.RegoV2.ApplicationServices.Security;
using AFT.RegoV2.Core.Bonus.ApplicationServices;
using AFT.RegoV2.Core.Bonus.Data;
using AFT.RegoV2.Core.Bonus.Data.ViewModels;
using AFT.RegoV2.Core.Security.Common;
using AFT.RegoV2.Tests.Common.Base;
using Microsoft.Practices.Unity;
using NUnit.Framework;

namespace AFT.RegoV2.Tests.Unit.Bonus
{
    class BonusPermissionsTests : PermissionsTestsBase
    {
        private BonusQueries _bonusQueries { get; set; }
        private BonusManagementCommands _bonusManagementCommands { get; set; }

        public override void BeforeEach()
        {
            base.BeforeEach();

            _bonusQueries = Container.Resolve<BonusQueries>();
            _bonusManagementCommands = Container.Resolve<BonusManagementCommands>();
        }

        [Test]
        public void Cannot_execute_BonusQueries_without_permissions()
        {
            /* Arrange */
            LogWithNewUser(Modules.BonusManager, Permissions.Add);

            /* Act */
            Assert.Throws<InsufficientPermissionsException>(() => _bonusQueries.GetCurrentVersionBonuses());
            Assert.Throws<InsufficientPermissionsException>(() => _bonusQueries.GetCurrentVersionTemplates());
        }

        [Test]
        public void Cannot_execute_BonusManagementCommands_without_permissions()
        {
            /* Arrange */
            LogWithNewUser(Modules.BonusManager, Permissions.View);

            /* Act */
            Assert.Throws<InsufficientPermissionsException>(() => _bonusManagementCommands.AddBonus(new Core.Bonus.Data.Bonus()));
            Assert.Throws<InsufficientPermissionsException>(() => _bonusManagementCommands.UpdateBonus(new Core.Bonus.Data.Bonus()));
            Assert.Throws<InsufficientPermissionsException>(() => _bonusManagementCommands.ChangeBonusStatus(new ToggleBonusStatusVM { Id = Guid.Empty }));
            Assert.Throws<InsufficientPermissionsException>(() => _bonusManagementCommands.AddUpdateTemplate(new Template()));
            Assert.Throws<InsufficientPermissionsException>(() => _bonusManagementCommands.DeleteTemplate(new Guid()));
        }
    }
}