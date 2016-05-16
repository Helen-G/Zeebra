using System;
using System.Linq;
using EasyNetQ.Management.Client;
using EasyNetQ.Management.Client.Model;
using MassTransit;

namespace AFT.RegoV2.Infrastructure
{
    public class MassTransitBusFactory
    {
        public static IServiceBus GetBus(string queueName)
        {
            var bus = ServiceBusFactory.New(sbc =>
            {
                ConfigureVHost();

                sbc.UseRabbitMq();
                var url = String.Format(
                    "rabbitmq://{0}/{1}/RegoV2_" + (queueName != null ? "{2}" : "{3}?temporary=true"),
                    ConfigurationParameters.RmqUrl,
                    ConfigurationParameters.RmqVhost,
                    queueName,
                    Guid.NewGuid().ToString("n").Substring(0, 8));

                sbc.ReceiveFrom(url);
            });

            return bus;
        }

        private static void ConfigureVHost()
        {
            var initial = new ManagementClient("http://"+ConfigurationParameters.RmqUrl, ConfigurationParameters.RmqLogin, ConfigurationParameters.RmqPassword);
            var hosts = initial.GetVHosts();
            var isHaveVhost = hosts.Any(x => x.Name == ConfigurationParameters.RmqVhost);
            if (!isHaveVhost)
            {
                var vhost = initial.CreateVirtualHost(ConfigurationParameters.RmqVhost);
                var user = initial.GetUser(ConfigurationParameters.RmqLogin);
                initial.CreatePermission(new PermissionInfo(user, vhost));
            }
        }
    }
}