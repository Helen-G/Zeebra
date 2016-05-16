using System;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Script.Serialization;
using AFT.RegoV2.Infrastructure.DataAccess.Base;
using AFT.RegoV2.Infrastructure.Synchronization;
using AFT.RegoV2.Shared;
using FluentValidation.Mvc;
using Microsoft.Practices.Unity;

namespace AFT.RegoV2.AdminWebsite
{
    public class MvcApplication : HttpApplication
    {
        private IUnityContainer _container;
        private const string DbExceptionMessage = "Connection to the MS SQL Server was made, but REGO database was not found. " +
                                            "Have you forgot to run WinService first?";

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            FluentValidationModelValidatorProvider.Configure();
            _container = Bootstrapper.Initialise();

            var synchronizationService = _container.Resolve<SynchronizationService>();
            synchronizationService.Execute("WinService", () =>
            {
                if (!RepositoryBase.IsDatabaseSeeded())
                    throw new RegoException(DbExceptionMessage);
            });
        }

        protected void  Application_BeginRequest(object sender, EventArgs e)
        {
            Response.AddHeader("Cache-Control", "no-cache, no-store, must-revalidate");
            Response.AddHeader("Pragma", "no-cache"); // HTTP 1.0.
            Response.AddHeader("Expires", "0"); // Proxies.
        }

        protected void Application_Error(object sender, EventArgs e)
        {
            var exc = Server.GetLastError();
            
            var isAjaxCall = string.Equals("XMLHttpRequest", Context.Request.Headers["x-requested-with"], StringComparison.OrdinalIgnoreCase);
           
            if (isAjaxCall)
            {
                Context.ClearError();
                Context.Response.ContentType = "application/json";
                Context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                Context.Response.Write(
                    new JavaScriptSerializer().Serialize(
                        new
                        {
                            exc.Message,
                            Detail = exc.StackTrace,
                            MethodName = exc.TargetSite.Name,
                            exc.Source,
                            User = Context.User.Identity.Name,
                            Time = DateTimeOffset.Now
                        }
                    )
                );
            }
        }
    }
}
