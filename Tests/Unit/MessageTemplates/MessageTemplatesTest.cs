using System;
using System.Linq;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Content;
using AFT.RegoV2.Core.Content.Data;
using AFT.RegoV2.Domain;
using AFT.RegoV2.Tests.Common.Base;
using Microsoft.Practices.Unity;
using NUnit.Framework;

namespace AFT.RegoV2.Tests.Unit.MessageTemplates
{
    internal class MessageTemplatesTest : AdminWebsiteUnitTestsBase
    {
        private IMessageTemplateService _messageTemplateService;
        private IContentRepository _contentRepository;
        private readonly Guid _testId;

        public MessageTemplatesTest()
        {
            _testId = Guid.NewGuid();
        }

        public override void BeforeEach()
        {
            base.BeforeEach();
            _contentRepository = Container.Resolve<IContentRepository>();
            _messageTemplateService =
                Container.Resolve<IMessageTemplateService>(new ParameterOverride("repository", _contentRepository));
        
            _contentRepository.MessageTemplates.Add(new MessageTemplate()
            {
                Id = _testId,
                MessageDeliveryMethod = MessageDeliveryMethod.Email,
                TemplateName = "test",
                MessageContent = "Test @Model.Test"
            });
        }

        [Test]
        public void Test_can_build_template()
        {
            var model = new {Test = "check"};
            var message = _messageTemplateService.GetMessage(_testId, model);
            Assert.IsTrue(message.Equals("Test check", StringComparison.CurrentCultureIgnoreCase));
        }

        public override void AfterEach()
        {
            base.AfterEach();
            var template = _contentRepository.MessageTemplates.FirstOrDefault(x => x.Id == _testId);
            if (template != null)
                _contentRepository.MessageTemplates.Remove(template);
        }
    }
}