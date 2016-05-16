using System;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Infrastructure;
using AFT.RegoV2.Tests.Common.Base;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace AFT.RegoV2.Tests.Unit.Infrastructure
{
    public class BusTests : AdminWebsiteUnitTestsBase
    {
        private Bus _bus;

        public override void BeforeEach()
        {
            base.BeforeEach();
            try
            {
                _bus = new Bus();
            }
            catch (Exception)
            {
                // ignored
            }
        }

        [Test]
        public void Can_be_resolved_by_type_name()
        {
            _bus.Should().NotBeNull();
        }

        [Test]
        public void Publish_will_throw_if_there_were_no_subscribers_registered()
        {
            Assert.Throws<BusException>(() =>
            {
                _bus.Publish(new TestMessage1());
            });
        }

        [Test]
        public void Will_throw_if_subscriber_does_not_implement_IConsume_interface()
        {
            Assert.Throws<BusException>(() =>
            {
                _bus.Subscribe<TestInvalidSubscriber>();
            });
        }

        [Test]
        public void Subscriber_instantiated_per_each_Publish_call()
        {
            //setup
            var timesInstantiated = 0;

            //act
            _bus.Subscribe(() =>
            {
                timesInstantiated++;
                return new TestSubscriberSingle();
            });
            _bus.Publish(new TestMessage1());
            _bus.Publish(new TestMessage1());
            _bus.Publish(new TestMessage1());

            //assert
            timesInstantiated.ShouldBeEquivalentTo(3);
        }

        [Test]
        public void Subscriber_handles_Publish_calls()
        {
            //setup
            var mock = new Mock<TestSubscriberSingle>();

            //act
            _bus.Subscribe(() => mock.Object);
            _bus.Publish(new TestMessage1());
            _bus.Publish(new TestMessage1());
            _bus.Publish(new TestMessage1());

            mock.Verify(subscriber => subscriber.Consume(It.IsAny<TestMessage1>()), Times.Exactly(3));
        }

        [Test]
        public void Subscriber_handles_Publish_calls_only_of_the_right_type()
        {
            //setup
            var mock = new Mock<TestSubscriberSingle>();
            var timesInstantiated = 0;
            _bus.Subscribe(() =>
            {
                timesInstantiated++;
                return mock.Object;
            });

            //act
            _bus.Publish(new TestMessage1());
            _bus.Publish(new TestMessage1());

            //assert
            Assert.Throws<BusException>(() =>
            {
                _bus.Publish(new TestMessage2());
            });

            timesInstantiated.ShouldBeEquivalentTo(2);
            mock.Verify(subscriber => subscriber.Consume(It.IsAny<TestMessage1>()), Times.Exactly(2));
        }

        [Test]
        public void One_message_can_be_received_by_multiple_subscribers()
        {
            //setup
            var sub1 = new Mock<TestSubscriberSingle>();
            var sub2 = new Mock<TestSubscriberMultiple>();
            _bus.Subscribe(() => sub1.Object);
            _bus.Subscribe(() => sub2.Object);

            //act
            _bus.Publish(new TestMessage1());

            //assert
            sub1.Verify(subscriber => subscriber.Consume(It.IsAny<TestMessage1>()), Times.Exactly(1));
            sub2.Verify(subscriber => subscriber.Consume(It.IsAny<TestMessage1>()), Times.Exactly(1));
        }

        [Test]
        public void Subscriber_can_consume_interface()
        {
            //setup
            var sub = new Mock<TestSubscriberInterface>();
            _bus.Subscribe(() => sub.Object);

            //act
            _bus.Publish(new TestMessage2());
            _bus.Publish(new TestMessage3());

            //assert
            Assert.Throws<BusException>(() =>
            {
                _bus.Publish(new TestMessage1());
            });
            sub.Verify(subscriber => subscriber.Consume(It.IsAny<IMessageMarker>()), Times.Exactly(2));
        }


        public void Subscribing_more_than_one_instance_distributes_messages_among_instances()
        {
            throw new NotImplementedException();
        }


        public void Poison_message_is_moved_to_the_dead_letter_queue_after_specified_number_of_retries()
        {
            throw new NotImplementedException();
        }


        public void Subscriptions_are_persisted_after_the_first_call()
        {
            //todo: in this test we should implement persistance mechanism for our queues
            //todo: so that a separate class is responsible for storing and retrieving queues (in-memory for now)
            throw new NotImplementedException();
        }


        public void Can_subscribe_and_retrieve_all_stored_messages()
        {
            //setup
            var mock = new Mock<TestSubscriberSingle>();

            //act
            _bus.Subscribe(() => mock.Object);
        }


        #region Helper classes
        public interface IMessageMarker : IMessage { }

        public class TestMessage1 : IMessage
        {
            public TestMessage1()
            {
                Created = DateTimeOffset.Now;
            }

            public string Text { get; set; }
            public DateTimeOffset Created { get; set; }
        }

        public class TestMessage2 : IMessage, IMessageMarker
        {
            public TestMessage2()
            {
                Created = DateTimeOffset.Now;
            }

            public string Text { get; set; }
            public DateTimeOffset Created { get; set; }
        }

        public class TestMessage3 : IMessage, IMessageMarker
        {
            public TestMessage3()
            {
                Created = DateTimeOffset.Now;
            }

            public string Text { get; set; }
            public DateTimeOffset Created { get; set; }
        }

        public class TestSubscriberSingle : IConsumes<TestMessage1>
        {
            public static int TimesInstantiated = 0;

            public TestSubscriberSingle()
            {
                TimesInstantiated++;
            }

            public virtual void Consume(TestMessage1 message) { }
        }

        public class TestSubscriberMultiple :
            IConsumes<TestMessage1>,
            IConsumes<TestMessage2>
        {
            public virtual void Consume(TestMessage1 message) {}

            public virtual void Consume(TestMessage2 message) {}
        }

        public class TestSubscriberInterface : IConsumes<IMessageMarker>
        {
            public virtual void Consume(IMessageMarker message) { }
        }

        public class TestInvalidSubscriber : IBusSubscriber { }

        #endregion
    }
}