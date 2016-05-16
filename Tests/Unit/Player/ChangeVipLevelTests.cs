using System;
using System.Linq;
using AFT.RegoV2.BoundedContexts.Event;
using AFT.RegoV2.Core.ApplicationServices.Player;
using AFT.RegoV2.Core.Common.Events;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Security.Data;
using AFT.RegoV2.Tests.Common.Base;
using AFT.RegoV2.Tests.Common.Helpers;
using FluentAssertions;
using Microsoft.Practices.Unity;
using Moq;
using NUnit.Framework;

namespace AFT.RegoV2.Tests.Unit.Player
{
    public class ChangeVipLevelTests : AdminWebsiteUnitTestsBase
    {
        [Test]
        public void Changing_Vip_level_triggers_event()
        {
            //setup
            Container.Resolve<SecurityTestHelper>().SignInUser();
            var brandHelper = Container.Resolve<BrandTestHelper>();
            var brand = brandHelper.CreateBrand();

            var vipLevel = brandHelper.CreateVipLevel(brand.Id, isDefault:false);
            
            var playerHelper = Container.Resolve<PlayerTestHelper>();
            var player = playerHelper.CreatePlayer();

            //act
            var playerCommands = Container.Resolve<PlayerCommands>();
            playerCommands.ChangeVipLevel(player.Id, vipLevel.Id, "test");

            //assert
            var eventRepository = Container.Resolve<IEventRepository>();
            eventRepository.Events.Where(x => x.DataType == typeof (PlayerVipLevelChanged).Name).Should().NotBeEmpty();
        }
    }
}
