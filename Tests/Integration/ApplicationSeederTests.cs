using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AFT.RegoV2.Core.Brand;
using AFT.RegoV2.Infrastructure.DataAccess;
using AFT.RegoV2.Tests.Common.Base;
using AFT.RegoV2.Tests.Common.TestDoubles;
using Microsoft.Practices.Unity;
using NUnit.Framework;

namespace AFT.RegoV2.Tests.Integration
{
    public class ApplicationSeederTests : MultiprocessTestsBase
    {
        private ApplicationSeeder _seeder;

        public override void BeforeEach()
        {
            base.BeforeEach();

            this.Container.RegisterType<IBrandRepository, FakeBrandRepository>();

            _seeder = CreateSeederOrNull();
        }

        [Test]
        public void Seeder_can_be_created()
        {
            Assert.DoesNotThrow(() =>
            {
                _seeder = Container.Resolve<ApplicationSeeder>(); 
            });
        }

        [Test, Ignore]
        public void Seeding_in_memory_database_does_not_throw()
        {
            Assert.DoesNotThrow(() =>
            {
                _seeder.Seed();
            });
        }

        private ApplicationSeeder CreateSeederOrNull()
        {
            try
            {
                return Container.Resolve<ApplicationSeeder>();
            }
            catch
            {
                //ignoring any errors because there is a separate test case specifically for seeding
                return null;
            }
        }
    }
}
