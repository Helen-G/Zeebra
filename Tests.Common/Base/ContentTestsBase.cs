using System.Collections.Generic;
using System.Linq;
using AFT.RegoV2.Core.Content;
using AFT.RegoV2.Core.Content.ApplicationServices;
using AFT.RegoV2.Core.Content.Data;
using AFT.RegoV2.Tests.Common.Helpers;
using Microsoft.Practices.Unity;

namespace AFT.RegoV2.Tests.Common.Base
{
    public abstract class ContentTestsBase : AdminWebsiteUnitTestsBase
    {
        protected IContentRepository ContentRepository { get; set; }
        protected IMessageTemplatesQueries MessageTemplatesQueries { get; set; }
        protected IMessageTemplatesCommands MessageTemplatesCommands { get; set; }
        protected ContentTestHelper ContentTestHelper { get; set; }
        protected BrandTestHelper BrandTestHelper { get; set; }
        protected Brand Brand { get; set; }

        public override void BeforeEach()
        {
            base.BeforeEach();

            ContentRepository = Container.Resolve<IContentRepository>();
            MessageTemplatesQueries = Container.Resolve<IMessageTemplatesQueries>();
            MessageTemplatesCommands = Container.Resolve<IMessageTemplatesCommands>();
            ContentTestHelper = Container.Resolve<ContentTestHelper>();
            BrandTestHelper = Container.Resolve<BrandTestHelper>();
            var securityTestHelper = Container.Resolve<SecurityTestHelper>();
            securityTestHelper.PopulatePermissions();
            securityTestHelper.SignInUser();

            var brand = BrandTestHelper.CreateBrand();

            Brand = new Brand
            {
                Id = brand.Id,
                Name = brand.Name,
                Languages = brand.BrandCultures
                    .Select(x => new Language
                    {
                        Code = x.Culture.Code,
                        Name = x.Culture.Name
                    }).ToArray()
            };
        }
    }
}
