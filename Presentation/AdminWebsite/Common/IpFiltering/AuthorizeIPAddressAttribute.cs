using System;
using System.Configuration;
using System.Web;
using System.Web.Mvc;
using AFT.RegoV2.Core.Security.ApplicationServices;
using AFT.RegoV2.Domain.BoundedContexts.Security.ApplicationServices;

namespace AFT.RegoV2.AdminWebsite.Common.IpFiltering
{
    public class AuthorizeIpAddressAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var service = DependencyResolver.Current.GetService<BackendIpRegulationService>();

            //Get users IP Address 
            string ipAddress = HttpContext.Current.Request.UserHostAddress;

            var isIpAddressAllowed = service.VerifyIpAddress(ipAddress);

            if (!isIpAddressAllowed)
            {
                //Send back a HTTP Status code of 403 Forbidden  
                filterContext.Result = new HttpStatusCodeResult(403);
            }

            base.OnActionExecuting(filterContext);
        }
    } 
}