using AFT.RegoV2.ApplicationServices.Player;
using AFT.RegoV2.Core.ApplicationServices.Player;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Domain.Player;
using AFT.RegoV2.Infrastructure.DataAccess.Player.Repository;
using AFT.RegoV2.Infrastructure.Sms;
using AFT.RegoV2.Shared;
using Microsoft.Practices.Unity;
namespace AFT.RegoV2.Tests.Common
{/*
    public class ServicesHost : AppHostHttpListenerBase
    {
        private readonly UnityContainer _unityContainer;

        public ServicesHost(UnityContainer unityContainer)
            : base("HttpListener Self-Host", typeof(PlayerWebservice).Assembly)
        {
            _unityContainer = unityContainer;
        }

        public override void Configure(Container container)
        {
            
            _unityContainer.RegisterType<IPlayerRepository, PlayerRepository>(new ContainerControlledLifetimeManager());

            container.Adapter = new UnityContainerAdapter(_unityContainer);

            JsConfig.EmitCamelCaseNames = true;
            Plugins.Add(new ValidationFeature());

            Plugins.Add(new AuthFeature(() => new AuthUserSession(), new IAuthProvider[] { new CustomAuthProvider() }));
            Plugins.Add(new SessionFeature());

            container.RegisterAs<EntityFrameworkCacheClient, ICacheClient>();

            container.RegisterAs<EmailNotifier, IEmailNotifier>().ReusedWithin(ReuseScope.Request);
            container.RegisterAs<SmsNotifier, ISmsNotifier>().ReusedWithin(ReuseScope.Request);

            container.RegisterAutoWired<PlayerCommands>();
            container.RegisterAutoWired<PlayerQueries>();

            RequestFilters.Add((req, res, requestDto) => CultureHelper.SetThreadCurrentCulture(requestDto));
        }
    }*/
}