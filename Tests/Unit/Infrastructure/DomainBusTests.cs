using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Infrastructure;
using AFT.RegoV2.Tests.Common.Base;
using FluentAssertions;
using Microsoft.Practices.Unity;
using NUnit.Framework;

namespace AFT.RegoV2.Tests.Unit.Infrastructure
{
    public class DomainBusTests : AdminWebsiteUnitTestsBase
    {
        private EventBus _eventBus;
        public override void BeforeEach()
        {
            base.BeforeEach();
            try
            {
                _eventBus = new EventBus();
            }
            catch (Exception)
            {
                // ignored
            }
        }

        [Test]
        public void Can_be_resolved_by_type_name()
        {
            _eventBus.Should().NotBeNull();
        }

        [Test]
        public void Can_be_resolved_by_interface()
        {
            var domainBus = Container.Resolve<IEventBus>();
            domainBus.Should().NotBeNull();
        }
    }
}
