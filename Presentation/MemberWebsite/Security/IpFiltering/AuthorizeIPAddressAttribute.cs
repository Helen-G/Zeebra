using System;
using System.Configuration;
using System.Web;
using System.Web.Mvc;
using AFT.RegoV2.MemberApi.Interface.Proxy;


namespace AFT.RegoV2.MemberApi.Interface.Security.IpFiltering
{
    public class AuthorizeIpAddressAttribute : ActionFilterAttribute
    {
        private string _brandName;

        public AuthorizeIpAddressAttribute(string brandName)
        {
            _brandName = brandName;
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            //Get users IP Address 
            string ipAddress = HttpContext.Current.Request.UserHostAddress;

            var playerServiceProxy = new MemberApiProxy(ConfigurationManager.AppSettings["MemberApiUrl"], Guid.NewGuid().ToString());
            var result = playerServiceProxy.VerifyIp(new VerifyIpRequest { IpAddress = ipAddress, BrandName = _brandName });

            if (!result.Allowed)
            {
                switch (result.BlockingType)
                {
                    case BlockingTypes.Redirection:
                        filterContext.Result = new RedirectResult(result.RedirectionUrl);
                        break;
                    case BlockingTypes.LoginRegistration:
                        throw new ApplicationException("IP address is not allowed to access this site");
                    default:
                        throw new Exception("Unrecognized blocking type");
                }
            }


            base.OnActionExecuting(filterContext);
        }
    } 
}
