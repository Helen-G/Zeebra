using System;
using System.Web;
using System.Web.Mvc;
using AFT.RegoV2.AdminWebsite.Controllers;

namespace AFT.RegoV2.AdminWebsite.Common.ErrorHandling
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
    public class SessionExpireAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);
            HttpContext ctx = HttpContext.Current;

            // check if session is supported
            if (ctx.Session != null)
            {
                if (filterContext.HttpContext.Session.IsSessionExpired())
                {
                    if (filterContext.HttpContext.Request.IsAjaxRequest())
                    {
                        if (filterContext.ActionDescriptor.ActionName == "getsecuritydata")
                            return;
                        filterContext.HttpContext.Response.StatusCode = 504;
                        filterContext.Result = new JsonResult()
                        {
                            Data = new {Message = "Session expired"},
                            JsonRequestBehavior = JsonRequestBehavior.AllowGet
                        };
                    }
                    else
                    {
                        var controller = filterContext.Controller as BaseController;
                        if (controller != null)
                        {
                            controller.LogoutUser();
                        }
                        filterContext.Result = new RedirectResult("~/");
                    }
                }
            }
        }

    }

    public static class SessionExtensions
    {
        public static bool IsSessionExpired(this HttpSessionStateBase session)
        {
            if (session == null || !session.IsNewSession)
            {
                return false;
            }

            // If it says it is a new session, but an existing cookie exists, then it must have timed out   
            string sessionCookie = HttpContext.Current.Request.Headers["Cookie"];
            return sessionCookie != null && sessionCookie.Contains("ASP.NET_SessionId");
        }
    }
}