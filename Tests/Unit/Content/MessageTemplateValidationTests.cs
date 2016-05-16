using System;
using System.Linq;
using AFT.RegoV2.Core.Common.Data.Content;
using AFT.RegoV2.Core.Content.Data;
using AFT.RegoV2.Core.Content.Validators;
using AFT.RegoV2.Tests.Common;
using AFT.RegoV2.Tests.Common.Base;
using NUnit.Framework;

namespace AFT.RegoV2.Tests.Unit.Content
{
    class MessageTemplateValidationTests : ContentTestsBase
    {
        [TestCase(MessageDeliveryMethod.Email)]
        [TestCase(MessageDeliveryMethod.Sms)]
        public void Can_validate_add(MessageDeliveryMethod messageDeliveryMethod)
        {
            var data = ContentTestHelper.CreateAddMessageTemplateData(null, null, null, messageDeliveryMethod);

            var result = MessageTemplatesQueries.ValidateCanAdd(data);

            Assert.That(result.Errors.Count, Is.EqualTo(0));
        }

        [TestCase(MessageDeliveryMethod.Email)]
        [TestCase(MessageDeliveryMethod.Sms)]
        public void Can_validate_edit(MessageDeliveryMethod messageDeliveryMethod)
        {
            var data = ContentTestHelper.CreateEditMessageTemplateData(null, null, null, null, messageDeliveryMethod);

            var result = MessageTemplatesQueries.ValidateCanEdit(data);

            Assert.That(result.Errors.Count, Is.EqualTo(0));
        }

        [Test]
        public void Can_fail_missing_required_fields_email()
        {
            var data = new AddMessageTemplateData
            {
                BrandId = Brand.Id,
                MessageType = TestDataGenerator.GetRandomMessageType(),
                MessageDeliveryMethod = MessageDeliveryMethod.Email,
            };

            var result = MessageTemplatesQueries.ValidateCanAdd(data);

            Assert.That(result.Errors.Count, Is.EqualTo(6));
            Assert.That(result.Errors.All(x => x.ErrorMessage == MessageTemplateValidationError.Required.ToString()));
            Assert.That(result.Errors.SingleOrDefault(x => x.PropertyName == "LanguageCode"), Is.Not.Null);
            Assert.That(result.Errors.SingleOrDefault(x => x.PropertyName == "TemplateName"), Is.Not.Null);
            Assert.That(result.Errors.SingleOrDefault(x => x.PropertyName == "SenderName"), Is.Not.Null);
            Assert.That(result.Errors.SingleOrDefault(x => x.PropertyName == "SenderEmail"), Is.Not.Null);
            Assert.That(result.Errors.SingleOrDefault(x => x.PropertyName == "Subject"), Is.Not.Null);
            Assert.That(result.Errors.SingleOrDefault(x => x.PropertyName == "MessageContent"), Is.Not.Null);
        }

        [Test]
        public void Can_fail_missing_required_fields_sms()
        {
            var data = new AddMessageTemplateData
            {
                BrandId = Brand.Id,
                MessageType = TestDataGenerator.GetRandomMessageType(),
                MessageDeliveryMethod = MessageDeliveryMethod.Sms,
            };

            var result = MessageTemplatesQueries.ValidateCanAdd(data);

            Assert.That(result.Errors.Count, Is.EqualTo(4));
            Assert.That(result.Errors.All(x => x.ErrorMessage == MessageTemplateValidationError.Required.ToString()));
            Assert.That(result.Errors.SingleOrDefault(x => x.PropertyName == "LanguageCode"), Is.Not.Null);
            Assert.That(result.Errors.SingleOrDefault(x => x.PropertyName == "TemplateName"), Is.Not.Null);
            Assert.That(result.Errors.SingleOrDefault(x => x.PropertyName == "SenderNumber"), Is.Not.Null);
            Assert.That(result.Errors.SingleOrDefault(x => x.PropertyName == "MessageContent"), Is.Not.Null);
        }

        [Test]
        public void Can_fail_invalid_brand()
        {
            var data = ContentTestHelper.CreateAddMessageTemplateData();
            data.BrandId = Guid.Empty;

            var result = MessageTemplatesQueries.ValidateCanAdd(data);

            Assert.That(result.Errors.Count, Is.EqualTo(1));
            Assert.That(result.Errors.First().ErrorMessage, Is.EqualTo(MessageTemplateValidationError.InvalidBrand.ToString()));
        }

        [Test]
        public void Can_fail_invalid_culture()
        {
            var data = ContentTestHelper.CreateAddMessageTemplateData();
            data.LanguageCode = TestDataGenerator.GetRandomString(5);

            var result = MessageTemplatesQueries.ValidateCanAdd(data);

            Assert.That(result.Errors.Count, Is.EqualTo(1));
            Assert.That(result.Errors.First().ErrorMessage, Is.EqualTo(MessageTemplateValidationError.InvalidLanguage.ToString()));
        }

        [Test]
        public void Can_fail_invalid_message_type()
        {
            var data = ContentTestHelper.CreateAddMessageTemplateData();
            var messageTypes = Enum.GetValues(typeof (MessageType));
            data.MessageType = (MessageType) messageTypes.Length;

            var result = MessageTemplatesQueries.ValidateCanAdd(data);

            Assert.That(result.Errors.Count, Is.EqualTo(1));
            Assert.That(result.Errors.First().ErrorMessage, Is.EqualTo(MessageTemplateValidationError.InvalidMessageType.ToString()));
        }

        [Test]
        public void Can_fail_invalid_delivery_type()
        {
            var data = ContentTestHelper.CreateAddMessageTemplateData();
            var messageDeliveryMethods = Enum.GetValues(typeof (MessageDeliveryMethod));
            data.MessageDeliveryMethod = (MessageDeliveryMethod) messageDeliveryMethods.Length;

            var result = MessageTemplatesQueries.ValidateCanAdd(data);

            Assert.That(result.Errors.Count, Is.EqualTo(1));
            Assert.That(result.Errors.First().ErrorMessage, Is.EqualTo(MessageTemplateValidationError.InvalidMessageDeliveryMethod.ToString()));
        }

        [Test]
        public void Can_fail_content_compile_error()
        {
            var data = ContentTestHelper.CreateAddMessageTemplateData(null, null, null, null, "@Model.asdf234");

            var result = MessageTemplatesQueries.ValidateCanAdd(data);

            Assert.That(result.Errors.Count, Is.EqualTo(1));
            var error = result.Errors.First();
            Assert.That(error.ErrorMessage, Is.EqualTo(MessageTemplateValidationError.InvalidMessageContent.ToString()));
            Assert.That(((string)error.CustomState).Contains("AFT.RegoV2.Core.Content.Validators.MessageTemplateValidator"));
        }
    }
}