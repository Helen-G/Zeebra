using System;
using System.Net;
using System.Web.Mvc;
using AFT.RegoV2.AdminWebsite.Controllers;
using AFT.RegoV2.Shared;

namespace AFT.RegoV2.AdminWebsite.Common
{
    public class HandleApplicationErrorAttribute : HandleErrorAttribute
    {
        public override void OnException(ExceptionContext filterContext)
        {
            Elmah.ErrorSignal.FromCurrentContext().Raise(filterContext.Exception);

            filterContext.ExceptionHandled = true;
            filterContext.HttpContext.Response.Clear();

            filterContext.HttpContext.Response.TrySkipIisCustomErrors = true;

            var controller = filterContext.Controller as BaseController;

            if (controller == null)
            {
                return;
            }

            var time = DateTime.Now.ToString("MM/dd/yy H:mm:ss zzz");

            var applicationException = filterContext.Exception as RegoException;
            if (applicationException != null)
            {
                filterContext.Result = new JsonResult
                {
                    Data = new
                    {
                        applicationException.Message,
                        Detail = applicationException.StackTrace,
                        MethodName = applicationException.TargetSite.Name,
                        applicationException.Source,
                        User = controller.CurrentUser.UserName,
                        Time = time
                    },
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
            }
            else 
            {
                var exception = filterContext.Exception;

                filterContext.Result = new JsonResult()
                {
                    Data = new
                    {
                        Message = string.Format("Unexpected error occured {0}", 
                        string.IsNullOrEmpty(exception.Message) ? string.Empty : " (" + exception.Message + ")"),
                        Detail = exception.StackTrace,
                        MethodName = exception.TargetSite.Name,
                        exception.Source,
                        User = controller.CurrentUser.UserName,
                        Time = time
                    },
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
            }

            filterContext.HttpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
        }
    }
}