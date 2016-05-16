using System.Linq;
using AFT.RegoV2.BoundedContexts.Event;
using AFT.RegoV2.Core.Common.Commands;
using AFT.RegoV2.Core.Common.Events.Notifications;
using AFT.RegoV2.Tests.Common.Base;
using AFT.RegoV2.Tests.Common.TestDoubles;
using Microsoft.Practices.Unity;
using Newtonsoft.Json;
using NUnit.Framework;
using WinService.Workers;

namespace AFT.RegoV2.Tests.Unit.Event
{
    public class NotificationEventsTest : AdminWebsiteUnitTestsBase
    {
        private EmailNotificationWorker _emailNotificationWorker;
        private SmsNotificationWorker _smsNotificationWorker;
        private IEventRepository _eventRepository;
        private FakeServiceBus _serviceBus;

        public override void BeforeEach()
        {
            base.BeforeEach();
            _emailNotificationWorker = Container.Resolve<EmailNotificationWorker>();
            _smsNotificationWorker = Container.Resolve<SmsNotificationWorker>();
            _serviceBus = Container.Resolve<FakeServiceBus>();
            _eventRepository = Container.Resolve<FakeEventRepository>();
        }

        [Test]
        public void Can_send_email_send_event()
        {
            // Arrange
            _emailNotificationWorker.Start();
            var message = new EmailCommandMessage("from@from.com", "Fromy Fromerson", "to@to.com", "Toto Toterson", "subject", "message");
            _serviceBus.PublishMessage(message);

            var events = _eventRepository.Events.Where(x => x.DataType == typeof (NotificationSentEvent).Name);
            Assert.IsNotEmpty(events);
            Assert.AreEqual(1, events.Count());

            var notificationSendEvent = JsonConvert.DeserializeObject<NotificationSentEvent>(events.First().Data, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                ReferenceLoopHandling = ReferenceLoopHandling.Serialize
            });

            Assert.AreEqual(notificationSendEvent.Message, message.Body);
            Assert.AreEqual(notificationSendEvent.Status, NotificationStatus.Send);
            Assert.AreEqual(notificationSendEvent.Type, NotificationType.Email);
        }

        [Test]
        public void Can_send_sms_send_event()
        {
            // Arrange
            _smsNotificationWorker.Start();
            _serviceBus.PublishMessage(new SmsCommandMessage("1111111111", "test"));

            var events = _eventRepository.Events.Where(x => x.DataType == typeof(NotificationSentEvent).Name);
            Assert.IsNotEmpty(events);
            Assert.AreEqual(1, events.Count());

            var notificationSendEvent = JsonConvert.DeserializeObject<NotificationSentEvent>(events.First().Data, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                ReferenceLoopHandling = ReferenceLoopHandling.Serialize
            });

            Assert.AreEqual(notificationSendEvent.Message, "test");
            Assert.AreEqual(notificationSendEvent.Status, NotificationStatus.Send);
            Assert.AreEqual(notificationSendEvent.Type, NotificationType.Sms);
        }
    }
}