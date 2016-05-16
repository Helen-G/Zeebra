using System.Web.Mvc;
using System.Web.Routing;

namespace AFT.RegoV2.MemberWebsite
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
           // routes.IgnoreRoute("api/{*pathInfo}");

            routes.MapRoute(
                "ConfirmOfflineDeposit",
                "OfflineDepositConfirm",
                 new { controller = "Home", action = "OfflineDepositConfirm" });

            routes.MapRoute(
                name: "Default",
                url: "{culture}/{controller}/{action}/{id}",
                defaults: new { culture = "en-US", controller = "Home", action = "Login", id = UrlParameter.Optional });

        }
    }
}