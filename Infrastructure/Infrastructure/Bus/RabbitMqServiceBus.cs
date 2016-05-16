using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Infrastructure.Consumers;
using log4net;
using MassTransit;
using Microsoft.Practices.Unity;
using IServiceBus = AFT.RegoV2.Core.Common.Interfaces.IServiceBus;

namespace AFT.RegoV2.Infrastructure
{
    /// <summary>
    /// Represents a message bus interface and combines 2 approaches: 
    /// - synchronous in-process events publishing (method Publish)
    /// - asyncronous inter-process events and commands publishing (method PublishMessage)
    /// </summary>
    public class RabbitMqServiceBus : IServiceBus
    {
        private readonly IUnityContainer _container;
        private readonly Func<string, MassTransit.IServiceBus> _serviceBusFactory;
        private readonly ILog _logger;
        private readonly ServiceBusPublisherCache _serviceBusPublisherCache;

        private const int RetriesMax = 5;
        private readonly TimeSpan RetriesPheriod = TimeSpan.FromSeconds(1);

        public RabbitMqServiceBus(
            IUnityContainer container,
            Func<string, MassTransit.IServiceBus> serviceBusFactory,
            ILog logger)
        {
            _container = container;
            _serviceBusFactory = serviceBusFactory;
            _logger = logger;
            _serviceBusPublisherCache = container.Resolve<ServiceBusPublisherCache>();
        }

        public void PublishMessage<T>(T message) where T : class, IMessage
        {
            PerformServiceBusAction(
                queueName: "Publish",
                action: serviceBus => serviceBus.Publish(message)
            );
        }

        public void Subscribe<T>(Action<T> messageHandler, string queueName) where T : class, IMessage, new()
        {
            PerformServiceBusAction(
                queueName,
                action: serviceBus => serviceBus.SubscribeConsumer(() =>
                {
                    return new Consumer<T>(message =>
                    {
                        //retries logic goes here
                        var retryCount = 0;
                        var handled = false;
                        while (!handled && ++retryCount <= RetriesMax)
                        {
                            try
                            {
                                if (retryCount > 1)
                                    _logger.Info(string.Format("RabbitMq: attempt number {0} to process message '{1}' in queue '{2}'", retryCount, message.GetType().Name, queueName));
                                messageHandler(message);
                                handled = true;
                            }
                            catch (Exception e)
                            {
                                var wasLastAttempt = retryCount >= RetriesMax;
                                if (wasLastAttempt)
                                {
                                    _logger.Error(string.Format("RabbitMq: message '{0}' caused unexpected error! '{1}'", message.GetType().Name, e.Message), e);
                                    throw;
                                }
                                _logger.Warn(string.Format("RabbitMq: failed to process message '{0}' in queue '{3}' from the {1} try. Will try again in {2} sec.", message.GetType().Name, retryCount, RetriesPheriod.TotalSeconds, queueName));
                                Thread.Sleep(RetriesPheriod);
                            }
                        }
                    });
                }
            ));
        }

        private void PerformServiceBusAction(string queueName, Action<MassTransit.IServiceBus> action)
        {
            var attemptCount = 0;
            while (true)
            {
                try
                {
                    var serviceBus = _serviceBusPublisherCache.Get(queueName);
                    if (serviceBus == null)
                    {
                        serviceBus = _serviceBusFactory(queueName);
                        _serviceBusPublisherCache.Put(queueName, serviceBus);
                    }
                    action(serviceBus);
                    break;
                }
                catch (Exception exception)
                {
                    if (++attemptCount >= RetriesMax)
                        throw;

                    var isTimeoutException = false;
                    var ex = exception;
                    {
                        while (ex != null)
                        {
                            if (ex.Message.Contains("timed out"))
                            {
                                isTimeoutException = true;
                                break;
                            }
                            ex = ex.InnerException;
                        }
                    }
                    if (!isTimeoutException)
                    {
                        throw;
                    }

                    Thread.Sleep(RetriesPheriod);
                }
            }
        }

        public void Dispose()
        {
            _container.Resolve<MassTransit.IServiceBus>().Dispose();
        }
    }

    internal class ServiceBusPublisherCache
    {
        private readonly IDictionary<string, MassTransit.IServiceBus> _cache;

        public ServiceBusPublisherCache()
        {
            _cache = new Dictionary<string, MassTransit.IServiceBus>();
        }

        public void Put(string key, MassTransit.IServiceBus serviceBus)
        {
            lock (_cache)
            {
                if (_cache.ContainsKey(key))
                {
                    _cache[key] = serviceBus;
                }
                else
                {
                    _cache.Add(key, serviceBus);
                }
            }
        }

        public MassTransit.IServiceBus Get(string key)
        {
            if (_cache.ContainsKey(key))
                return _cache[key];

            return null;
        }

        public IEnumerable<MassTransit.IServiceBus> GetAll()
        {
            return _cache.Select(x => x.Value);
        }
    }
}