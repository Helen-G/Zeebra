using System;
using AFT.RegoV2.ApplicationServices.Security;
using AFT.RegoV2.Core.Brand.ApplicationServices;
using AFT.RegoV2.Core.Brand.Validators.ContentTranslations;
using AFT.RegoV2.Core.Security.Common;
using AFT.RegoV2.Tests.Common.Base;
using NUnit.Framework;
using Microsoft.Practices.Unity;

namespace AFT.RegoV2.Tests.Unit.Brand
{
    class ContentTranslationPermissionsTests : PermissionsTestsBase
    {
        private ContentTranslationCommands _contentTranslationCommands { get; set; }
        private ContentTranslationQueries _contentTranslationQueries { get; set; }

        public override void BeforeEach()
        {
            base.BeforeEach();

            _contentTranslationCommands = Container.Resolve<ContentTranslationCommands>();
            _contentTranslationQueries = Container.Resolve<ContentTranslationQueries>();
        }

        [Test]
        public void Cannot_execute_ContentTranslationCommands_without_permissions()
        {
            /* Arrange */
            LogWithNewUser(Modules.VipLevelManager, Permissions.Add);

            /* Act */
            Assert.Throws<InsufficientPermissionsException>(() => _contentTranslationCommands.CreateContentTranslation(new AddContentTranslationData()));
            Assert.Throws<InsufficientPermissionsException>(() => _contentTranslationCommands.UpdateContentTranslation(new EditContentTranslationData()));
            Assert.Throws<InsufficientPermissionsException>(() => _contentTranslationCommands.ActivateContentTranslation(new Guid(), "Some remark"));
            Assert.Throws<InsufficientPermissionsException>(() => _contentTranslationCommands.DeactivateContentTranslation(new Guid(), "Some remark"));
            Assert.Throws<InsufficientPermissionsException>(() => _contentTranslationCommands.DeleteContentTranslation(new Guid()));
        }

        [Test]
        public void Cannot_execute_ContentTranslationQueries_without_permissions()
        {
            /* Arrange */
            LogWithNewUser(Modules.VipLevelManager, Permissions.Add);

            /* Act */
            Assert.Throws<InsufficientPermissionsException>(() => _contentTranslationQueries.GetContentTranslations());
            Assert.Throws<InsufficientPermissionsException>(() => _contentTranslationQueries.GetCultureByCode("Some code"));
            Assert.Throws<InsufficientPermissionsException>(() => _contentTranslationQueries.GetCultureByCodes());
        }
    }
}